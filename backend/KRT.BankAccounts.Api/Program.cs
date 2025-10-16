using KRT.BankAccounts.Api._02_Application.DependencyInjection;
using KRT.BankAccounts.Api._02_Application.Interfaces.Infra;
using KRT.BankAccounts.Api._04_Infrastructure.Cache;
using KRT.BankAccounts.Api._04_Infrastructure.Data;
using KRT.BankAccounts.Api._04_Infrastructure.DependencyInjection;
using KRT.BankAccounts.Api._04_Infrastructure.Messaging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// Lê configurações de TTL do cache do appsettings.json
builder.Services.Configure<CacheSettings>(
    builder.Configuration.GetSection("CacheSettings")
);

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = builder.Configuration.GetConnectionString("RedisConnection");
    return ConnectionMultiplexer.Connect(configuration!);
});

//  Configurar mensageria no futuro
// builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMQ"));

// Configura para não retornar automaticamente 400 Bad Request em caso de ModelState inválido
// Isso permite customizar a resposta de erro e ele cai no IF do controller
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.AddSingleton<IMessagePublisher, RabbitMqMessagePublisher>();

builder.Services.AddApplication();
builder.Services.AddInfrastructure();

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
