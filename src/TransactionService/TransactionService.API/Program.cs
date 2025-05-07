using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;
using TransactionService.Application.Services;
using TransactionService.Domain.Repositories;
using TransactionService.Domain.Services;
using TransactionService.Infrastructure.Kafka;
using TransactionService.Infrastructure.Persistence;
using TransactionService.Infrastructure.Repositories;
using TransactionService.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Transaction Service API", Version = "v1" });
});

// Register application services
builder.Services.AddScoped<ITransactionApplicationService, TransactionApplicationService>();

// Register domain services
builder.Services.AddScoped<ITransactionDomainService, TransactionDomainService>();
builder.Services.AddScoped<IAntiFraudService, KafkaAntiFraudService>();

// Register repositories
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();

// Configure database
builder.Services.AddDbContext<TransactionDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure());
});

// Configure Kafka
builder.Services.AddSingleton<IKafkaProducer, KafkaProducer>();
builder.Services.AddSingleton<IKafkaConsumer, KafkaConsumer>();
builder.Services.AddHostedService<KafkaConsumerHostedService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TransactionDbContext>();
    dbContext.Database.EnsureCreated();
}

app.Run();
