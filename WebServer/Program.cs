using Microsoft.AspNetCore.Identity;
using WebServer.DAL;
using WebServer.DAL.Repositories;
using WebServer.DAL.Repositories.Abstractions;
using WebServer.Services;
using WebServer.Services.Abstractions;

namespace WebServer;

public static class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = 
            WebApplication.CreateBuilder(args);

        RegisterServices(builder.Services);

        WebApplication app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.MapControllers();
        app.Run();
    }

    private static void RegisterServices(IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddControllers();

        services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddSingleton<IUserRepository, InMemoryUserRepository>();
        services.AddSingleton<IBase64Encoder, Base64Encoder>();
    }
}