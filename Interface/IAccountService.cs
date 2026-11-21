using WarDudeBankApiV3.Models;

namespace WarDudeBankApiV3.Interface;

public interface IAccountService
{
    Task<List<Account>> GetAllAsync();

    Task<Account?> GetByIdAsync(int id);

    Task<Account> CreateAsync(
        string owner,
        string accountType,
        decimal initialBalance);

    Task<Account?> UpdateAsync(
        string owner,
        string accountType,
        int id);

    Task<bool> DeleteAsync(int id);

    Task<Account?> DepositAsync(int id, decimal amount);

    Task<Account?> WithdrawAsync(int id, decimal amount);

    Task<List<BankTransaction>> GetTransactionsAsync(int accountId);

    Task TransferAsync(int fromAccountId, int toAccountId, decimal amount);
}
