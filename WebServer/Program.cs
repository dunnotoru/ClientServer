using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;
using WebServer.Auth;
using WebServer.DAL;
using WebServer.DAL.Repositories;
using WebServer.DAL.Repositories.Abstractions;
using WebServer.Services;
using WebServer.Services.Abstractions;

namespace WebServer;

public static class Program
{
    //dotnet run --urls "http://localhost:7777" to run with required url
    //dotnet run --environment Production to run with prod
    //dotnet run --launch-profile https to run profile from launch-settings (not actually important)
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

        app.UseAuthentication();
        app.UseAuthorization();
        
        app.MapControllers();
        app.Run();
    }

    private static void RegisterServices(IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "Demo API", Version = "v1" });
            options.AddSecurityDefinition("basic", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "basic",
                In = ParameterLocation.Header,
                Description = "Basic Authorization header."
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "basic"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });
        services.AddControllers();
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = BasicAuthenticationDefaults.Scheme;
            options.DefaultChallengeScheme = BasicAuthenticationDefaults.Scheme;
        }).AddScheme<BasicAuthenticationOptions, BasicAuthenticationHandler>(BasicAuthenticationDefaults.Scheme, null);
        services.AddAuthorization();

        services.AddSingleton<BasicAuthenticationService>();
        services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddSingleton<IUserRepository, InMemoryUserRepository>();
        services.AddSingleton<IBase64Encoder, Base64Encoder>();
    }
}