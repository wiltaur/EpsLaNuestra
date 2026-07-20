using EpsLaNuestra.Application.Features.Patients.Commands;
using EpsLaNuestra.Application.Utilities;
using EpsLaNuestra.Application.Validators;
using EpsLaNuestra.Application.Wrappers;
using EpsLaNuestra.Domain.Interfaces;
using EpsLaNuestra.Domain.Interfaces.Repositories;
using EpsLaNuestra.Domain.Persistence;
using EpsLaNuestra.Infrastructure;
using EpsLaNuestra.Infrastructure.Data;
using EpsLaNuestra.Infrastructure.Repositories;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using Polly;
using Polly.Retry;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using System.Text;

string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

var builder = WebApplication.CreateBuilder(args);

//Polly Resilience
builder.Services.AddResiliencePipeline("sql-retry-pipeline", pipelineBuilder =>
{
    var delays = new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(5) };

    pipelineBuilder.AddRetry(new RetryStrategyOptions
    {
        ShouldHandle = new PredicateBuilder().Handle<Exception>(),
        MaxRetryAttempts = 3,
        BackoffType = DelayBackoffType.Constant,
        DelayGenerator = args =>
        {
            var delay = args.AttemptNumber < delays.Length
                ? delays[args.AttemptNumber]
                : delays.Last();
            return ValueTask.FromResult<TimeSpan?>(delay);
        },
        OnRetry = args =>
        {
            Console.WriteLine($"[Polly] Intento #{args.AttemptNumber + 1} fallido debido a: {args.Outcome.Exception?.Message}. Esperando {args.RetryDelay.TotalSeconds} segundos...");
            return ValueTask.CompletedTask;
        }
    });
});

//IOptions - POCO
builder.Services.Configure<BlazorServerSettings>(
    builder.Configuration.GetSection("BlazorServerSettings")
);

builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings")
);

builder.Services.Configure<AuthenticationSettings>(
    builder.Configuration.GetSection("SecretsValues:Auth")
);

// EF Core & MongoDb
builder.Services.AddDbContext<SqlServerDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetSection("SecretsValues:ConStringSqlServer").Value));

builder.Services.AddSingleton<IMongoClient>(sp =>
    new MongoClient(builder.Configuration.GetSection("SecretsValues:ConStringMongoDb").Value));

builder.Services.AddScoped<MongoDbContext>();

// MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(AdmitPatientCommand).Assembly));

//FluentValidation
builder.Services.AddScoped<IValidator<PatientJsonWrapper>, PatientJsonValidator>();

// Repositorios y UoW
builder.Services.AddScoped<IPatientMongoRepository, PatientMongoRepository>();
builder.Services.AddScoped<IPatientSqlRepository, PatientSqlRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IBlazorConnectUtility, BlazorConnectUtility>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "EPS La Nuestra API",
        Version = "v1"
    });
    options.AddSecurityDefinition("Bearer",
      new OpenApiSecurityScheme
      {
          Description = "JWT Authorization header using the Bearer scheme (Example: 'Bearer 12345abcdef')",
          Name = "Authorization",
          In = ParameterLocation.Header,
          Type = SecuritySchemeType.ApiKey,
          Scheme = "Bearer"
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

//Coors
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      builderCoors =>
                      {
                          builderCoors.WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Value ?? string.Empty).
                                              AllowAnyHeader().
                                              AllowAnyMethod();
                      });
});

//JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => {
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration.GetSection("SecretsValues:Auth:AuthIssuer").Value,
        ValidAudience = builder.Configuration.GetSection("SecretsValues:Auth:AuthAudience").Value,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetSection("SecretsValues:Auth:AuthKey").Value ?? string.Empty))
    };
});

// For addd comments - swagger.
builder.Services.AddSwaggerGen(o => AddSwaggerDocumentation(o));

static void AddSwaggerDocumentation(SwaggerGenOptions o)
{
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    o.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(MyAllowSpecificOrigins);

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();