using Serilog;

namespace SafeReport.API.Extensions
{
	public static class SerilogExtensions
	{
		public static IHostBuilder UseSerilogConfiguration(this IHostBuilder hostBuilder, IConfiguration configuration)
		{
			Log.Logger = new LoggerConfiguration()
				.ReadFrom.Configuration(configuration)
				.Enrich.FromLogContext()
				.CreateLogger();

			return hostBuilder.UseSerilog();
		}
	}
}
