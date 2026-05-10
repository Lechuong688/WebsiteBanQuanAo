using Data.DTO.User;
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

        public async Task<ProfileDTO?> GetProfileAsync(string userId)
        {
            var user = await _userManager.Users
                .Include(x => x.Avatar)
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
            {
                return null;
            }

            return new ProfileDTO
            {
                Id = user.Id,
                FullName = user.Name,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                AvatarId = user.AvatarId,
                AvatarPath = user.Avatar?.FilePath
            };
        }

        public async Task<bool> UpdateProfileAsync(ProfileDTO model)
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(x => x.Id == model.Id);

            if (user == null)
            {
                return false;
            }

            user.Name = model.FullName;
            user.PhoneNumber = model.PhoneNumber;
            user.Address = model.Address;
            user.AvatarId = model.AvatarId;

            var result = await _userManager.UpdateAsync(user);

            return result.Succeeded;
        }
    }
}
