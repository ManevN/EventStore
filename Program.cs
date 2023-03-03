namespace WebApp
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Npgsql;
    using Serilog;

    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<EventStoreContext>(x => x.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnectionSting")));

            builder.Services.AddControllers();

            builder.Services.AddScoped<NpgsqlConnection>(x => new NpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnectionSting")));

            builder.Host.UseSerilog((context, configuration) =>
                configuration.ReadFrom.Configuration(context.Configuration));

            var app = builder.Build();

            app.UseSerilogRequestLogging();

            app.MapControllerRoute(
                        name: "default",
                        pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}