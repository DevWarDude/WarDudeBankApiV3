namespace WarDudeBankApiV3.DTOs;




public record CreateAccountRequest(
    string Owner,
    string AccountType,
    decimal InitialBalance
);
