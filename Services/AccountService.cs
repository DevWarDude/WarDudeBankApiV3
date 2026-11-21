using Microsoft.EntityFrameworkCore;
using WarDudeBankApiV3.Data;
using WarDudeBankApiV3.Models;
using WarDudeBankApiV3.Interface;
using WarDudeBankApiV3.Validation;

namespace WarDudeBankApiV3.Services;

public class AccountService : IAccountService
{
    private readonly BankDbContext _db;

    public AccountService(BankDbContext db)
    {
        _db = db;
    }

    public async Task<List<Account>> GetAllAsync()
    {
        return await _db.Accounts.ToListAsync();
    }


    public async Task<Account?> GetByIdAsync(int id)
    {

        var account = await _db.Accounts.FindAsync(id);

        return account;
    }

    public async Task<Account> CreateAsync(
        string owner,
        string accountType,
        decimal initialBalance)
    {
        var account = new Account
        {
            Owner = owner.Trim(),
            AccountType = accountType,
            Balance = initialBalance
        };

        _db.Accounts.Add(account);

        await _db.SaveChangesAsync();

        return account;
    }



    public async Task<Account?> UpdateAsync(
    string owner,
    string accountType,
    int id)
    {
        var account = await _db.Accounts.FindAsync(id);

        if (account is null)
        {
            return null;
        }

        account.Owner = owner.Trim();
        account.AccountType = accountType;

        await _db.SaveChangesAsync();

        return account;
    }


    public async Task<bool> DeleteAsync(int id)
    {
        var account = await _db.Accounts.FindAsync(id);

        if (account is null)
        {
            return false;
        }



        _db.Accounts.Remove(account);


        await _db.SaveChangesAsync();


        return true;
    }



    public async Task<Account?> DepositAsync(int id, decimal amount)
    {
        // 1. Reject invalid deposit amounts
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(amount),
                "Deposit amount must be greater than zero."
            );
        }

        // 2. Begin database transaction
        await using var tx =
            await _db.Database.BeginTransactionAsync();


        try
        {

            // 3. Update balance directly in PostgreSQL
            int updatedRows = await _db.Accounts
                .Where(a => a.Id == id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(a => a.Balance, a => a.Balance + amount));

            if (updatedRows == 0)
            {
                return null;

            }


            // Retrieve the updated account
            var account = await _db.Accounts.AsNoTracking().SingleAsync(a => a.Id == id);

            //Create a BankTransaction record
            var transaction = new BankTransaction
            {
                AccountId = account.Id,
                Type = "Deposit",
                Amount = amount,
                BalanceAfter = account.Balance
            };

            //Save the transaction history
            _db.BankTransactions.Add(transaction);


            // STEP 8: Save the transaction record
            await _db.SaveChangesAsync();

            //Commit the database transaction
            await tx.CommitAsync();

            //Return the updated account
            return account;
        }
        catch
        {
            // Undo uncommitted changes if anything fails
            await tx.RollbackAsync();
            throw;
        }


    }


    public async Task<Account?> WithdrawAsync(int id, decimal amount)
    {
        // STEP 1: Validate the amount
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(amount),
                "Withdrawal amount must be greater than zero."
            );
        }

        // STEP 2: Begin a database transaction
        await using var tx =
            await _db.Database.BeginTransactionAsync();

        try
        {
            // STEP 3: Update the balance only if funds are sufficient
            int updatedRows = await _db.Accounts
                .Where(a => a.Id == id && a.Balance >= amount)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(
                        a => a.Balance,
                        a => a.Balance - amount
                    ));

            // STEP 4: Handle an unsuccessful update
            if (updatedRows == 0)
            {
                bool accountExists = await _db.Accounts
                    .AnyAsync(a => a.Id == id);

                if (!accountExists)
                {
                    return null;
                }

                throw new InvalidOperationException(
                    "Insufficient balance."
                );
            }

            // STEP 5: Retrieve the updated account
            var account = await _db.Accounts
                .AsNoTracking()
                .SingleAsync(a => a.Id == id);

            // STEP 6: Create a withdrawal history record
            var transaction = new BankTransaction
            {
                AccountId = account.Id,
                Type = "Withdrawal",
                Amount = amount,
                BalanceAfter = account.Balance
            };

            // STEP 7: Add the new transaction record
            _db.BankTransactions.Add(transaction);

            // STEP 8: Save the transaction record
            await _db.SaveChangesAsync();

            // STEP 9: Commit everything
            await tx.CommitAsync();

            return account;
        }
        catch
        {
            // Undo uncommitted changes if anything fails
            await tx.RollbackAsync();
            throw;
        }
    }


    public async Task<List<BankTransaction>> GetTransactionsAsync(int accountId)
    {

        return await _db.BankTransactions
            .Where(t => t.AccountId == accountId)

            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }




    public async Task TransferAsync(
        int fromAccountId,
        int toAccountId,
        decimal amount)
    {
        // STEP 1: Validate before changing anything
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(amount),
                "Transfer amount must be greater than zero."
            );
        }

        if (fromAccountId == toAccountId)
        {
            throw new InvalidOperationException(
                "Cannot transfer to the same account."
            );
        }

        // STEP 2: Begin database transaction
        await using var tx =
            await _db.Database.BeginTransactionAsync();

        try
        {
            // STEP 3: Determine a consistent locking order
            var accountIds = new[] { fromAccountId, toAccountId }
                .OrderBy(id => id)
                .ToArray();

            // STEP 4: Lock accounts in ascending ID order
            foreach (var accountId in accountIds)
            {
                await _db.Accounts
                    .FromSqlInterpolated(
                        $"SELECT * FROM \"Accounts\" WHERE \"Id\" = {accountId} FOR UPDATE"
                    )
                    .AsNoTracking()
                    .SingleOrDefaultAsync();
            }

            // STEP 5: Debit sender atomically
            int senderUpdatedRows = await _db.Accounts
                .Where(a =>
                    a.Id == fromAccountId &&
                    a.Balance >= amount)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(
                        a => a.Balance,
                        a => a.Balance - amount
                    ));

            // STEP 6: Check sender debit
            if (senderUpdatedRows == 0)
            {
                bool senderExists = await _db.Accounts
                    .AnyAsync(a => a.Id == fromAccountId);

                if (!senderExists)
                {
                    throw new KeyNotFoundException(
                        "Sender account not found."
                    );
                }

                throw new InvalidOperationException(
                    "Insufficient balance."
                );
            }

            // STEP 7: Credit receiver atomically
            int receiverUpdatedRows = await _db.Accounts
                .Where(a => a.Id == toAccountId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(
                        a => a.Balance,
                        a => a.Balance + amount
                    ));

            if (receiverUpdatedRows == 0)
            {
                throw new KeyNotFoundException(
                    "Receiver account not found."
                );
            }

            // STEP 8: Get both updated balances
            var fromAccount = await _db.Accounts
                .AsNoTracking()
                .SingleAsync(a => a.Id == fromAccountId);

            var toAccount = await _db.Accounts
                .AsNoTracking()
                .SingleAsync(a => a.Id == toAccountId);

            // STEP 9: Record sender's transaction
            var fromTransaction = new BankTransaction
            {
                AccountId = fromAccount.Id,
                Type = "TransferOut",
                Amount = amount,
                BalanceAfter = fromAccount.Balance
            };

            // STEP 10: Record receiver's transaction
            var toTransaction = new BankTransaction
            {
                AccountId = toAccount.Id,
                Type = "TransferIn",
                Amount = amount,
                BalanceAfter = toAccount.Balance
            };

            // STEP 11: Save both history records
            _db.BankTransactions.Add(fromTransaction);
            _db.BankTransactions.Add(toTransaction);

            await _db.SaveChangesAsync();

            // STEP 12: Commit the transfer
            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

}
