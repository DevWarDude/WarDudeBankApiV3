using Microsoft.EntityFrameworkCore;
using WarDudeBankApiV3.Data;
using WarDudeBankApiV3.Endpoints;
using WarDudeBankApiV3.Services;
using WarDudeBankApiV3.Interface;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<BankDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("BankDatabase")
    )
);

builder.Services.AddScoped<IAccountService, AccountService>();

var app = builder.Build();
app.MapAccountEndpoints();


app.Run();