namespace  Logginng;
public static partial class Log
{

    [LoggerMessage(20,LogLevel.Information,"weather forecast requested at {date}.")]
    public static partial void WeatherForecastRequested(this ILogger logger,DateTime  date);
}