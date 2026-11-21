cat > README.md <<'EOF'
# WarDude Bank API V3

A RESTful banking API built with C# and ASP.NET Core. It provides account management, deposits, withdrawals, transaction history, and account-to-account transfers using PostgreSQL.

## Tech Stack

- C#
- .NET 10
- ASP.NET Core Minimal APIs
- Entity Framework Core
- PostgreSQL
- Npgsql

## Features

- Create, retrieve, update and delete bank accounts.
- Deposit and withdraw money.
- View account transaction history.
- Transfer money between accounts.
- Atomic balance updates.
- Database transactions and rollback handling.
- Concurrent transfer protection using database row locks.
- Input validation and error handling.

## API Endpoints

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/accounts` | Retrieve all accounts |
| GET | `/api/accounts/{id}` | Retrieve an account |
| POST | `/api/accounts` | Create an account |
| PUT | `/api/accounts/{id}` | Update account details |
| DELETE | `/api/accounts/{id}` | Delete an account |
| POST | `/api/accounts/{id}/deposit` | Deposit money |
| POST | `/api/accounts/{id}/withdraw` | Withdraw money |
| GET | `/api/accounts/{id}/transactions` | Retrieve transaction history |
| POST | `/api/transfers` | Transfer money between accounts |

## Getting Started

### Prerequisites

- .NET 10 SDK
- PostgreSQL
- Git

### Installation

Clone the repository:

```bash
git clone YOUR_REPOSITORY_URL
cd WarDudeBankApiV3
