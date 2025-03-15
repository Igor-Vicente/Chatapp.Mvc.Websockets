using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Websockets.Mvc.Configuration;
using Websockets.Mvc.Data;
using Websockets.Mvc.Extensions;
using Websockets.Mvc.Factories;
using Websockets.Mvc.Repository;

namespace Websockets.Mvc
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
            builder.Services.AddSqlServer<ApplicationDbContext>(builder.Configuration.GetConnectionString("sqlserver"));
            builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Authentication/Login";
                options.AccessDeniedPath = "/Authentication/AccessDenied";
            });
            builder.Services.AddControllersWithViews();
            builder.Services.AddSingleton<MongoTableFactory>();
            builder.Services.AddSingleton<IChatConnectionManager, ChatConnectionManager>();
            builder.Services.AddSingleton<INotifierConnectionManager, NotifierConnectionManager>();
            builder.Services.AddScoped<IChatRepository, ChatRepository>();
            builder.Services.AddScoped<IUserInjection, UserInjection>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            var webSocketOptions = new WebSocketOptions
            {
                KeepAliveInterval = TimeSpan.FromMinutes(2)
            };
            app.UseWebSockets(webSocketOptions);
            app.ApplyMigrations();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
