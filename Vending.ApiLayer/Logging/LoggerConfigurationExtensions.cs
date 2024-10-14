using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

public static class LoggerConfigurationExtensions
{
    public static ILoggingBuilder AddCustomLoggingFilters(this ILoggingBuilder builder)
    {
        // Serilog yapılandırması
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.ControlledBy(new Serilog.Core.LoggingLevelSwitch(LogEventLevel.Information)) // Varsayılan log seviyesi
            .MinimumLevel.Override("Vending.ApiLayer.Controllers.WeatherForecastController", LogEventLevel.Information) // Belirli controller için log seviyesi
            .CreateLogger();

        // Denetleyici filtrelerini ayarlayın
        builder.AddFilter("Vending.ApiLayer.Controllers.WeatherForecastController", LogLevel.Information);  // Log level for WeatherForecastController
        builder.AddFilter("Vending.ApiLayer.Controllers.TestController", LogLevel.Information);  // Log level for TestController
        builder.AddFilter("Vending.ApiLayer.Controllers.*", LogLevel.None);  // Hide other controllers' logs

        return builder;
    }
}
