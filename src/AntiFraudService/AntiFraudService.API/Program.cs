using Microsoft.EntityFrameworkCore;
using AntiFraudService.Application.Services;
using AntiFraudService.Domain.Ports;
using AntiFraudService.Domain.Services;
using AntiFraudService.Infrastructure.Kafka;
using AntiFraudService.Infrastructure.Persistence;
using AntiFraudService.Infrastructure.Services;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using FluentValidation.AspNetCore;
using FluentValidation;
using AntiFraudService.Application.Validators;
using AntiFraudService.Application.DTOs;
using AntiFraudService.Domain.Validators;
using AntiFraudService.Domain.Models;

namespace AntiFraudService.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers()
            .AddFluentValidation(fv => 
            {
                fv.ImplicitlyValidateChildProperties = true;
                fv.DisableDataAnnotationsValidation = true;
            });

        // Registrar validadores de FluentValidation
        builder.Services.AddScoped<IValidator<AntiFraudService.Application.DTOs.TransactionValidationRequest>, TransactionValidationRequestValidator>();
        builder.Services.AddScoped<IValidator<AntiFraudService.Domain.Models.TransactionData>, TransactionDataValidator>();
        
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new() { Title = "Anti-Fraud Service API", Version = "v1" });
        });

        // Register application services
        builder.Services.AddScoped<IAntiFraudApplicationService, AntiFraudApplicationService>();

        // Register domain services
        builder.Services.AddScoped<IAntiFraudDomainService, AntiFraudDomainService>();
        builder.Services.AddScoped<IValidationRuleService, MaximumAmountValidationService>();
        builder.Services.AddScoped<IValidationRuleService, DailyLimitValidationService>();

        // Register ports and adapters (hexagonal architecture)
        builder.Services.AddScoped<ITransactionEventPort, KafkaTransactionEventService>();
        builder.Services.AddScoped<ITransactionEventConsumerPort, KafkaTransactionEventConsumerService>();
        builder.Services.AddScoped<ITransactionValidationRepositoryPort, TransactionValidationRepositoryAdapter>();

        // Configure database
        builder.Services.AddDbContext<AntiFraudDbContext>(options =>
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
            var dbContext = scope.ServiceProvider.GetRequiredService<AntiFraudDbContext>();
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
