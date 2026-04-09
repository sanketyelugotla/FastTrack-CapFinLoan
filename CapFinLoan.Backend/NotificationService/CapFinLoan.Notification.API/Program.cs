using CapFinLoan.Notification.Application.Interfaces;
using CapFinLoan.Notification.Infrastructure.Clients;
using CapFinLoan.Notification.Infrastructure.Email;
using CapFinLoan.Notification.Infrastructure.Messaging;
using MassTransit;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

var rabbitHost = builder.Configuration["RabbitMQ:Host"] ?? "localhost";
var rabbitUsername = builder.Configuration["RabbitMQ:Username"] ?? "guest";
var rabbitPassword = builder.Configuration["RabbitMQ:Password"] ?? "guest";

builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("Email"));
builder.Services.Configure<NotificationDependencyOptions>(builder.Configuration.GetSection("NotificationDependencies"));

builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
builder.Services.AddHttpClient<IUserProfileClient, AuthUserProfileClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<NotificationDependencyOptions>>().Value;
    client.BaseAddress = new Uri(options.AuthServiceBaseUrl);
});

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<UserRegisteredConsumer>();
    x.AddConsumer<ApplicationSubmittedConsumer>();
    x.AddConsumer<ApplicationStatusChangedConsumer>();
    x.AddConsumer<DocumentVerifiedConsumer>(); x.AddConsumer<OtpSendConsumer>();
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

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CapFinLoan Notification API",
        Version = "1.0"
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => "Notification Service running");
app.MapControllers();

app.Run();
