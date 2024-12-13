using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

using HF.EventHorizon.Infrastructure.Data;
using HF.EventHorizon.Web.Data;
using HF.EventHorizon.App;
using MySql.EntityFrameworkCore.Extensions;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace HF.EventHorizon.Web;

public class Program
{
    public static void Main(string[] args)
    {
        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .WriteTo.File(
                path: "Logs/log-.txt",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30)
            .CreateLogger();

        var builder = WebApplication.CreateBuilder(args);

        // Add Serilog to the logging pipeline
        builder.Host.UseSerilog();

        // Add services to the container.
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Add Home Manager Core Database Context
        builder.Services.AddDbContext<EvtHorizonContext>(options =>
        {
            var dbType = builder.Configuration.GetValue<string>("DatabaseType");
            switch (dbType)
            {
                case "MySQL":
                    options.UseMySQL(connectionString);
                    break;
                case "PostgreSQL":
                    options.UseNpgsql(connectionString);
                    break;
                default:
                    options.UseSqlServer(connectionString);
                    break;
            }
        });

        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
            .AddEntityFrameworkStores<ApplicationDbContext>();

        builder.Services.AddControllersWithViews();

        builder.Services.AddEventBus(builder.Configuration);
        builder.Services.AddServerEventHandlers(builder.Configuration);

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
        app.MapRazorPages();

        app.Run();
    }
}
