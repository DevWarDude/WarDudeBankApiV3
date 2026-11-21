namespace WarDudeBankApiV3.DTOs;


public record UpdateAccountRequest(
    string Owner,
    string AccountType
);