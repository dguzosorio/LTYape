using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using TransactionService.Application.Services;
using TransactionService.Domain.Repositories;
using TransactionService.Domain.Services;
using TransactionService.Infrastructure.Kafka;
using TransactionService.Infrastructure.Persistence;
using TransactionService.Infrastructure.Repositories;
using TransactionService.Infrastructure.Services;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace TransactionService.API;

public class Program
{
    public static void Main(string[] args)
    {
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
                sqlOptions => sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null));
        });

        // Configure Kafka
        builder.Services.AddSingleton<IKafkaProducer, KafkaProducer>();
        builder.Services.AddSingleton<IKafkaConsumer, KafkaConsumer>();
        builder.Services.AddHostedService<KafkaConsumerHostedService>();

        // Add CORS to allow requests from any origin
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            });
        });

        // Add Health Checks
        builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy());

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseRouting();
        
        // Enable CORS
        app.UseCors("AllowAll");

        app.UseAuthorization();

        app.MapControllers();

        // Add health check endpoint
        app.MapHealthChecks("/healthz", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"status\": \"ok\"}");
            }
        });

        // Ensure database is created
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<TransactionDbContext>();
            try
            {
                dbContext.Database.EnsureCreated();
                Console.WriteLine("Database created or verified successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating database: {ex.Message}");
            }
        }

        app.Run();
    }
}
