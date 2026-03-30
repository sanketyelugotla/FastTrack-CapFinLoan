using System.Text;
using CapFinLoan.Admin.Application.Interfaces;
using CapFinLoan.Admin.Application.Services;
using CapFinLoan.Admin.Infrastructure.Messaging;
using CapFinLoan.Admin.Persistence.Data;
using CapFinLoan.Admin.Persistence.Repositories;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var rabbitHost = builder.Configuration["RabbitMQ:Host"] ?? "localhost";
var rabbitUsername = builder.Configuration["RabbitMQ:Username"] ?? "guest";
var rabbitPassword = builder.Configuration["RabbitMQ:Password"] ?? "guest";
var authServiceBaseUrl = builder.Configuration["AdminDependencies:AuthServiceBaseUrl"] ?? "http://localhost:5021";
var documentServiceBaseUrl = builder.Configuration["AdminDependencies:DocumentServiceBaseUrl"] ?? "http://localhost:5023";

builder.Services.AddDbContext<AdminDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CapFinLoanDb")));

builder.Services.AddScoped<IAdminLoanApplicationRepository, AdminLoanApplicationRepository>();
builder.Services.AddScoped<IAdminLoanApplicationService, AdminLoanApplicationService>();
builder.Services.AddScoped<IEventPublisher, RabbitMqEventPublisher>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ApplicationSubmittedConsumer>();
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
    });

builder.Services.AddHttpClient("AuthServiceClient", client =>
{
    client.BaseAddress = new Uri(authServiceBaseUrl);
});

builder.Services.AddHttpClient("DocumentServiceClient", client =>
{
    client.BaseAddress = new Uri(documentServiceBaseUrl);
});

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "CapFinLoan Admin API",
        Version = "1.0"
    });

    // Add JWT Bearer support
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your valid JWT token."
    });
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AdminDbContext>();
    await dbContext.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => "Admin Service running");

app.Run();
