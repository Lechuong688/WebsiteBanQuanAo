using Data.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repository.User
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _context;
        private readonly UserManager<UserEntity> _userManager;
        public UserRepository(DataContext context, UserManager<UserEntity> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<(List<UserEntity> Users, int Total)> GetUserAsync(string? keyword, int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize <= 0) pageSize = 5;

            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(x =>
                    x.UserName.Contains(keyword) ||
                    x.Email.Contains(keyword) ||
                    x.Name.Contains(keyword)
                );
            }

            var totalUsers = await query.CountAsync();

            var users = await query
                .OrderByDescending(x => x.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToArrayAsync();


            return (users.ToList(), totalUsers);
        }
        public async Task<Dictionary<string, List<string>>> GetUserRolesAsync(List<UserEntity> users)
        {
            var userRoles = new Dictionary<string, List<string>>();

            foreach (var user in users)
            {
                userRoles[user.Id] = (await _userManager.GetRolesAsync(user)).ToList();
            }
            return userRoles;
        }
    }
}
