using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RealtimeLeaderboardAPI.Application;
using RealtimeLeaderboardAPI.Domain;

namespace RealtimeLeaderboardAPI.Infrastructure;

public class DbSeeder {
    private readonly RealtimeLeaderboardAPIDbContext _context;
    private readonly PasswordHasher _passwordHasher;
    private readonly IConfiguration _config;
    public DbSeeder(RealtimeLeaderboardAPIDbContext context, PasswordHasher passwordHasher, IConfiguration configuration) {
        _context = context;
        _passwordHasher = passwordHasher;
        _config = configuration;
    }

    public async Task SeedAdmin() {
        if (_context.UserTable.Any(u => u.Role == "Admin"))
            return;

        var adminEmail = _config["Admin:Email"];
        var adminPassword = _config["Admin:Password"];

        if (string.IsNullOrEmpty(adminPassword))
            throw new Exception("Missing ADMIN_PASSWORD");

        var theAdminOne = new User {
            Email = adminEmail,
            PasswordHashed = _passwordHasher.HashPassword(adminPassword),
            Role = "Admin",
            Name = "theChosenOne"
        };

        _context.UserTable.Add(theAdminOne);
        await _context.SaveChangesAsync();
    }
}