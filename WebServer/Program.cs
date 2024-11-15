using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using WebServer.Auth;
using WebServer.DAL;
using WebServer.DAL.Entity;
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
        
        RegisterServices(builder);
        
        WebApplication app = builder.Build();

        using (IServiceScope scope = app.Services.CreateScope())
        {
            Initialize(scope.ServiceProvider).Wait();
        }

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

    private static async Task Initialize(IServiceProvider provider)
    {
        UserManager<User> userManager = provider.GetRequiredService<UserManager<User>>();
        RoleManager<IdentityRole<int>> roleManager = provider.GetRequiredService<RoleManager<IdentityRole<int>>>();
        IPasswordHasher<User> passwordHasher = provider.GetRequiredService<IPasswordHasher<User>>();
        
        const string adminRoleName = "Admin";
        const string userRoleName = "User";
        
        if (!await roleManager.RoleExistsAsync(adminRoleName))
        {
            await roleManager.CreateAsync(
                new IdentityRole<int>(adminRoleName)
                );
        }
        if (!await roleManager.RoleExistsAsync(userRoleName))
        {
            await roleManager.CreateAsync(
                new IdentityRole<int>(userRoleName)
                );
        }
        
        User? storedRoot = await userManager.FindByNameAsync("root");
        if (storedRoot is not null)
        {
            return;
        }

        User root = new User { UserName = "root" };
        await userManager.CreateAsync(root, passwordHasher.HashPassword(root, "root"));
        await userManager.AddToRoleAsync(root, adminRoleName);
    }

    private static void RegisterServices(WebApplicationBuilder builder)
    {
        IServiceCollection services = builder.Services;
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

        services.AddAuthentication(BasicAuthenticationDefaults.Scheme)
            .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>(BasicAuthenticationDefaults.Scheme, null);
        

        services.AddScoped<BasicAuthenticationHandler>();
        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOrOwner", policy =>
            {
                policy.AddRequirements(new AdminOrOwnerRequirement());
                policy.RequireAuthenticatedUser();
            });
        });
        
        services.AddControllers();
        services.AddScoped<IAuthorizationHandler, AdminOrOwnerHandler>();
        services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddDbContext<UserDbContext>(options =>
        {
            options.UseNpgsql(builder.Configuration.GetConnectionString("IdentityConnection"));
        });
        services.AddIdentity<User, IdentityRole<int>>()
            .AddEntityFrameworkStores<UserDbContext>();
        
        services.AddSingleton<IBase64Encoder, Base64Encoder>();
    }
}