using Elastic.Ingest.Elasticsearch;
using Elastic.Ingest.Elasticsearch.DataStreams;
using Elastic.Serilog.Sinks;
using Microsoft.OpenApi.Models;
using PayBridge.Api.Authorization;
using PayBridge.Api.Errors;
using PayBridge.Api.Exceptions;
using PayBridge.Api.Middleware;
using PayBridge.Api.Modules;
using PayBridge.BuildingBlocks.Security;
using PayBridge.BuildingBlocks.Security.IntegrationTokens;
using PayBridge.Modules.Merchants.Infrastructure.Persistence.Seed;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);
var elasticSearchUri =
    builder.Configuration["ElasticSearch:Uri"]
    ?? throw new InvalidOperationException(
        "ElasticSearch Uri configuration was not found.");
builder.Services.AddSerilog((services, configuration) =>
{
    configuration
        .MinimumLevel.Information()
        .MinimumLevel.Override(
            "Microsoft.AspNetCore",
            LogEventLevel.Warning)
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.Elasticsearch(
            new[] { new Uri(elasticSearchUri) },
            options =>
            {
                options.DataStream =
                    new DataStreamName(
                        "logs",
                        "paybridge-api",
                        "development");

                options.BootstrapMethod =
                    BootstrapMethod.Failure;
            });
});

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PayBridge API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT token giriniz. Sadece token deðerini yazýn, baþýna Bearer eklemeyin."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddSingleton<IErrorCatalog, ErrorCatalog>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddIntegrationJwtSecurity(builder.Configuration);

builder.Services.AddScoped<
    IIntegrationPaymentAccessService,
    AllowAllIntegrationPaymentAccessService>();

builder.Services.AddModules(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();
app.UseMiddleware<CorrelationIdMiddleware>();

if (app.Environment.IsDevelopment())
{
    await app.Services.SeedMerchantMockDataAsync();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();