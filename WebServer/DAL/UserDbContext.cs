using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebServer.DAL.Entity;

namespace WebServer.DAL;

public sealed class UserDbContext : IdentityDbContext<User, IdentityRole<int>, int>
{
    private readonly IConfiguration _configuration;
    
    public UserDbContext(IConfiguration configuration, 
        DbContextOptions optionsBuilder) : base(optionsBuilder)
    {
        _configuration = configuration;
        Database.Migrate();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_configuration.GetConnectionString("IdentityConnection"));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(builder =>
        {
            builder.HasKey(u => u.Id);
            builder.HasIndex(u => u.UserName).IsUnique();
            builder.Property(u => u.UserName).HasMaxLength(100);
            builder.Property(u => u.PasswordHash).HasMaxLength(100);
        });
        
        modelBuilder.Entity<IdentityUserLogin<int>>()
            .HasKey(login => new { login.UserId, login.LoginProvider, login.ProviderKey });
        modelBuilder.Entity<IdentityUserRole<int>>()
            .HasKey(role => new { role.UserId, role.RoleId });
        modelBuilder.Entity<IdentityUserToken<int>>()
            .HasKey(token => new { token.UserId, token.Name, token.LoginProvider });
    }
}