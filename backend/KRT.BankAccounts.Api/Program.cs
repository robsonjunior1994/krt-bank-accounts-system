using KRT.BankAccounts.Api._02_Application.DependencyInjection;
using KRT.BankAccounts.Api._04_Infrastructure.Data;
using KRT.BankAccounts.Api._04_Infrastructure.DependencyInjection;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddApplication();
builder.Services.AddInfrastructure();

//  Configurar cache/mensageria no futuro
// builder.Services.AddStackExchangeRedisCache(...);
// builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMQ"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
