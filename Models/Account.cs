namespace WarDudeBankApiV3.Models;

public class Account
{
    public int Id { get; set; }

    public string Owner { get; set; } = "";

    public string AccountType { get; set; } = "";

    public decimal Balance { get; set; }
}