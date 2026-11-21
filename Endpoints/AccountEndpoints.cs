using Microsoft.EntityFrameworkCore;
using WarDudeBankApiV3.Data;
using WarDudeBankApiV3.DTOs;
using WarDudeBankApiV3.Models;
using WarDudeBankApiV3.Services;
using WarDudeBankApiV3.Interface;
using WarDudeBankApiV3.Validation;

namespace WarDudeBankApiV3.Endpoints;

public static class AccountEndpoints
{
    public static IEndpointRouteBuilder MapAccountEndpoints(
        this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/accounts", async (IAccountService service) =>
        {
            var accounts = await service.GetAllAsync();

            return Results.Ok(accounts);
        });


        app.MapGet("/api/accounts/{id:int}", async (int id, IAccountService service) =>
        {
            var account = await service.GetByIdAsync(id);

            if (account is null)
            {
                return Results.NotFound("Account not found!");
            }

            return Results.Ok(account);
        });


        app.MapPost("/api/accounts", async (CreateAccountRequest request, IAccountService service) =>
        {
            if (string.IsNullOrWhiteSpace(request.Owner) ||
                request.Owner.Length > 100 ||
                request.AccountType is not ("Savings" or "Current") ||
                !AccountRules.HasValidInitialBalance(request.InitialBalance))
            {
                return Results.BadRequest("Invalid account details.");
            }

            var account = await service.CreateAsync(request.Owner, request.AccountType, request.InitialBalance);

            return Results.Created($"/api/accounts/{account.Id}", account);
        });



        app.MapPut("/api/accounts/{id:int}",
            async (int id, UpdateAccountRequest request, IAccountService service) =>
        {

            if (string.IsNullOrWhiteSpace(request.Owner) ||
                request.Owner.Length > 100 ||
                request.AccountType is not ("Savings" or "Current"))
            {
                return Results.BadRequest("Invalid account details.");
            }

            var account = await service.UpdateAsync(request.Owner, request.AccountType, id);

            if (account is null)
            {
                return Results.NotFound("Account not found.");
            }

            return Results.Ok(account);
        });


        app.MapDelete("/api/accounts/{id:int}", async (int id, IAccountService service) =>
        {
            bool result = await service.DeleteAsync(id);

            if (result is false)
            {
                return Results.NotFound("Account not found.");
            }


            return Results.NoContent();
        });


        app.MapPost("/api/accounts/{id:int}/deposit",
        async (
            int id,
            DepositRequest request,
            IAccountService accountService
        ) =>
        {
            if (request.Amount <= 0)
            {
                return Results.BadRequest(
                    "Deposit amount must be greater than zero."
                );
            }

            var account = await accountService.DepositAsync(
                id,
                request.Amount
            );

            if (account is null)
            {
                return Results.NotFound("Account not found.");
            }

            return Results.Ok(account);
        });



        app.MapPost("/api/accounts/{id:int}/withdraw",
            async (
                int id,
                WithdrawalRequest request,
                IAccountService accountService
            ) =>
            {
                if (request.Amount <= 0)
                {
                    return Results.BadRequest(
                        "Withdrawal amount must be greater than zero."
                    );
                }

                try
                {
                    var account = await accountService.WithdrawAsync(
                        id,
                        request.Amount
                    );

                    if (account is null)
                    {
                        return Results.NotFound("Account not found.");
                    }

                    return Results.Ok(account);
                }
                catch (InvalidOperationException ex)
                {
                    return Results.BadRequest(ex.Message);
                }
            });

        app.MapGet("/api/accounts/{id:int}/transactions",
        async (int id, IAccountService accountService) =>
        {
            var account = await accountService.GetByIdAsync(id);

            if (account is null)
            {
                return Results.NotFound("Account not found.");
            }

            var transactions =
                await accountService.GetTransactionsAsync(id);

            return Results.Ok(transactions.Select(t => new
            {
                t.Id,
                t.Type,
                t.Amount,
                t.BalanceAfter,
                t.CreatedAt
            }));
        });


        app.MapPost("/api/transfers",
        async (TransferRequest request, IAccountService accountService) =>
        {
            if (request.Amount <= 0)
            {
                return Results.BadRequest(
                    "Tranfer amount must be greater than zero."
                );
            }

            try
            {
                await accountService.TransferAsync(
                    request.FromAccountId,
                    request.ToAccountId,
                    request.Amount
                );



                return Results.Ok("Transfer completed successfully.");
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);

            }
        });

        return app;
    }
}





