
var configuration = GetConfiguration();
//var host = CreateHostBuilder(configuration, args);
Log.Logger = CreateSerilogLogger(configuration);


try
{
    var host = CreateHostBuilder(configuration, args);
    Log.Information("Configuring web host ({ApplicationContext})...", Program.AppName);
    Log.Information("Applying migrations ({ApplicationContext})...", Program.AppName);


    Log.Information("Starting web host ({ApplicationContext})...", Program.AppName);
    host.Run();

}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}





IHost CreateHostBuilder(IConfiguration configuration, string[] args) =>
Host.CreateDefaultBuilder(args)
   .ConfigureWebHostDefaults(webBuilder =>
   {
       webBuilder.UseStartup<Startup>()
      .UseContentRoot(Directory.GetCurrentDirectory())
      .UseWebRoot("Pics")
      .ConfigureAppConfiguration(x => x.AddConfiguration(configuration))
      .CaptureStartupErrors(false);
   })
   .UseSerilog()
   .Build();



IConfiguration GetConfiguration()
{
    var path = Directory.GetCurrentDirectory();

    var builder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddEnvironmentVariables();
    return builder.Build();
}


Serilog.ILogger CreateSerilogLogger(IConfiguration configuration)
{
    var seqServerUrl = configuration["SeqServerUrl"];
    var logstashUrl = configuration["LogstashgUrl"];
    return new LoggerConfiguration()
          .MinimumLevel.Verbose()
              .Enrich.WithProperty("Environment", "Development")
              .Enrich.WithProperty("Application", Program.AppName)
              .Enrich.FromLogContext()
              .WriteTo.Console()
              .WriteTo.File(new RenderedCompactJsonFormatter(), "log.ndjson", restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Verbose)
              .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day,
              rollOnFileSizeLimit: true, restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Warning)
              .WriteTo.Seq(string.IsNullOrWhiteSpace(seqServerUrl) ? "http://seq" : seqServerUrl)
              .WriteTo.Http(string.IsNullOrWhiteSpace(logstashUrl) ? "http://logstash:8080" : logstashUrl, null)
              .ReadFrom.Configuration(configuration).CreateLogger();
}

public partial class Program
{

    public static string? Namespace = typeof(Startup).Namespace;
    public static string? AppName = "Catalog.API";
}




//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.

//builder.Services.AddControllers();
//// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();

//app.UseAuthorization();

//app.MapControllers();

//app.Run();
