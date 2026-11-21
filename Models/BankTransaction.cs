
namespace WarDudeBankApiV3.Models;

public class BankTransaction
{
    public int Id { get; set; }

    public int AccountId { get; set; }

    public Account Account { get; set; } = null!;

    public string Type { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public decimal BalanceAfter { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
