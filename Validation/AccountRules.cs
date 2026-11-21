namespace WarDudeBankApiV3.Validation;

public static class AccountRules
{
    public static bool HasValidInitialBalance(decimal balance)
    {
        return balance >= 0;
    }

    public static bool CanWithdraw(decimal balance, decimal amount)
    {
        return amount > 0 && amount <= balance;
    }
}