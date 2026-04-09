using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using MMLib.SwaggerForOcelot.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

var ocelotFileName = builder.Environment.IsEnvironment("Docker") ? "ocelot.Docker.json" : "ocelot.json";
builder.Configuration.AddJsonFile(ocelotFileName, optional: false, reloadOnChange: true);

builder.Services.AddOcelot(builder.Configuration);
builder.Services.AddSwaggerForOcelot(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseCors();
app.UseStaticFiles();

app.UseSwaggerForOcelotUI(options =>
{
    options.PathToSwaggerGenerator = "/swagger/docs";
}, uiOptions =>
{
    uiOptions.DocumentTitle = "CapFinLoan API Gateway";
    uiOptions.InjectStylesheet("/swagger-ui/SwaggerDark.css");
});

app.MapGet("/", () => "API Gateway running");

await app.UseOcelot();

app.Run();