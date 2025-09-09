using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Selfix.Application;
using Selfix.EntryPoint;
using Selfix.Infrastructure;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting web application");
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseDefaultServiceProvider((_, options) =>
    {
        options.ValidateScopes = true;
        options.ValidateOnBuild = true;
    });

    // if (builder.Environment.EnvironmentName == "Local")
    //     builder.Configuration.AddJsonFile("appsettings.Local.json", true, true);

    builder.Configuration
        .AddEnvironmentVariables()
        .AddJsonFile("appsettings.Local.json", true, true);

    builder
        .AddSettings()
        .AddInfrastructure();

    builder.Services
        .AddApplication();

    var app = builder.Build();
    
    app.UseInfrastructure();

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}