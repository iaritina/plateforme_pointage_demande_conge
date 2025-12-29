using BackOffice.Context;
using BackOffice.Models;
using Microsoft.EntityFrameworkCore;


namespace BackOffice.Services;

public class UserService
{
    private readonly MyDbContext _context;

    public UserService(MyDbContext context)
    {
        _context = context;
    }

    public async Task<List<User>> GetAllUserAsync()
    {
        return await _context.Users.ToListAsync();
    }

    public async Task<User> CreateUser(User user)
    {
        user.Password = BCrypt.Net.BCrypt.HashPassword(user.LastName);
    
        // Valeurs par défaut si non fournies
        if (user.HiringDate == default) user.HiringDate = DateTime.UtcNow;
        if (string.IsNullOrEmpty(user.Role)) user.Role = "User";

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

}