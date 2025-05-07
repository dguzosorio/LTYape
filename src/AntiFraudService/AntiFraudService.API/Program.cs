using Microsoft.EntityFrameworkCore;
using AntiFraudService.Application.Services;
using AntiFraudService.Domain.Repositories;
using AntiFraudService.Domain.Services;
using AntiFraudService.Infrastructure.Kafka;
using AntiFraudService.Infrastructure.Persistence;
using AntiFraudService.Infrastructure.Repositories;
using AntiFraudService.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

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
builder.Services.AddScoped<ITransactionService, KafkaTransactionService>();

// Register validation rules
builder.Services.AddScoped<IValidationRuleService, MaximumAmountValidationService>();
builder.Services.AddScoped<IValidationRuleService, DailyLimitValidationService>();

// Register repositories
builder.Services.AddScoped<ITransactionValidationRepository, TransactionValidationRepository>();

// Configure database
builder.Services.AddDbContext<AntiFraudDbContext>(options =>
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
    var dbContext = scope.ServiceProvider.GetRequiredService<AntiFraudDbContext>();
    dbContext.Database.EnsureCreated();
}

app.Run();
