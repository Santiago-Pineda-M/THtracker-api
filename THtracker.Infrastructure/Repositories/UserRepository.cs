using Microsoft.EntityFrameworkCore;
using THtracker.Domain.DTOs;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;
using THtracker.Infrastructure.Persistence;

namespace THtracker.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<UserDto>> GetAllAsync()
    {
        return await _context.Users
            .Select(u => new UserDto(u.Id, u.Name, u.Email))
            .ToListAsync();
    }

    public async Task<UserDto?> GetByIdAsync(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        
        return user is null 
            ? null 
            : new UserDto(user.Id, user.Name, user.Email);
    }

    public async Task<UserDto> CreateAsync(CreateUserDto dto)
    {
        var user = new User(dto.Name, dto.Email);
        
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        
        return new UserDto(user.Id, user.Name, user.Email);
    }

    public async Task<UserDto?> UpdateAsync(Guid id, UpdateUserDto dto)
    {
        var user = await _context.Users.FindAsync(id);
        
        if (user is null)
            return null;
        
        user.Update(dto.Name, dto.Email);
        
        await _context.SaveChangesAsync();
        
        return new UserDto(user.Id, user.Name, user.Email);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        
        if (user is null)
            return false;
        
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        
        return true;
    }
}
