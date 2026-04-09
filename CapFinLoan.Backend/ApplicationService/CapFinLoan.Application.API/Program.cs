using System.Text;
using CapFinLoan.Application.Application.Interfaces;
using CapFinLoan.Application.Application.Services;
using CapFinLoan.Application.Infrastructure.Messaging;
using CapFinLoan.Application.Persistence.Data;
using CapFinLoan.Application.Persistence.Repositories;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

var rabbitHost = builder.Configuration["RabbitMQ:Host"] ?? "localhost";
var rabbitUsername = builder.Configuration["RabbitMQ:Username"] ?? "guest";
var rabbitPassword = builder.Configuration["RabbitMQ:Password"] ?? "guest";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CapFinLoanDb")));

builder.Services.AddScoped<ILoanApplicationRepository, LoanApplicationRepository>();
builder.Services.AddScoped<ILoanApplicationService, LoanApplicationService>();
builder.Services.AddScoped<IEventPublisher, RabbitMqEventPublisher>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ApplicationStatusChangedConsumer>();
    x.AddConsumer<UserRegisteredConsumer>();
    x.AddConsumer<DocumentVerifiedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbitHost, "/", h =>
        {
            h.Username(rabbitUsername);
            h.Password(rabbitPassword);
        });
        cfg.ConfigureEndpoints(context);
    });
});

var jwtSection = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSection["Key"] ?? throw new InvalidOperationException("JWT key is missing.");
var issuer = jwtSection["Issuer"] ?? throw new InvalidOperationException("JWT issuer is missing.");
var audience = jwtSection["Audience"] ?? throw new InvalidOperationException("JWT audience is missing.");
var key = Encoding.UTF8.GetBytes(secretKey);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.FromMinutes(1),
            RoleClaimType = System.Security.Claims.ClaimTypes.Role,
            NameClaimType = System.Security.Claims.ClaimTypes.Name
        };

        // DEBUG: Log incoming JWT tokens
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (!string.IsNullOrEmpty(context.Token))
                {
                    Console.WriteLine($"[DEBUG] Incoming JWT Token: {context.Token}");
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CapFinLoan Application API",
        Version = "1.0"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your valid JWT token."
    });

    options.AddSecurityRequirement(document =>
    {
        return new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecuritySchemeReference("Bearer", document),
                new List<string>()
            }
        };
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}

app.UseMiddleware<CapFinLoan.Application.API.Middleware.GlobalExceptionHandlerMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => "Application Service running");

app.Run();
