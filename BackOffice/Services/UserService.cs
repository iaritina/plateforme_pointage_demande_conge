using BackOffice.ViewModels;
using Microsoft.EntityFrameworkCore;
using Shared.Context;
using Shared.models;


namespace BackOffice.Services;

public class UserService
{
    private readonly MyDbContext _context;
    private readonly SoldeCongeService _soldeCongeService;

    public UserService(MyDbContext context, SoldeCongeService soldeCongeService)
    {
        _context = context;
        _soldeCongeService = soldeCongeService;
    }

    public async Task<List<User>> GetAllUserAsync()
    {
        return await _context.Users.ToListAsync();
    }

    public async Task<PagedResultViewModel<User>> GetUserPaginated(int page, int pageSize)
    {
        var query = _context.Users.AsNoTracking();

        var total = await query.CountAsync();

        var users = await query
            .OrderBy(u => u.LastName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResultViewModel<User>
        {
            Items = users,
            Page = page,
            PageSize = pageSize,
            TotalItems = total
        };
    }

    public async Task<User> CreateUser(User user)
    {
        user.Password = BCrypt.Net.BCrypt.HashPassword(user.LastName);
        
        if (user.HiringDate == default) user.HiringDate = DateTime.UtcNow;
        if (string.IsNullOrEmpty(user.Role)) user.Role = "User";

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        
        await _soldeCongeService.CreateAsync(user.Id);
        
        var schedules = new List<Schedule>();
        
        for (int day = 1; day <= 5; day++)
        {
            schedules.Add(new Schedule { Day = day, Start = "08:00", End = "12:00", Working = true, UserId = user.Id });
            schedules.Add(new Schedule { Day = day, Start = "12:00", End = "13:00", Working = false, UserId = user.Id }); 
            schedules.Add(new Schedule { Day = day, Start = "13:00", End = "17:00", Working = true, UserId = user.Id });
        }
            
        schedules.Add(new Schedule { Day = 6, Start = "00:00", End = "23:59", Working = false, UserId = user.Id });
        schedules.Add(new Schedule { Day = 7, Start = "00:00", End = "23:59", Working = false, UserId = user.Id });
            
        _context.Schedules.AddRange(schedules);
        await _context.SaveChangesAsync();
        
        
        return user;
    }

}