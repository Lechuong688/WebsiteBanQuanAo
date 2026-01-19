using Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repository.User
{
    public interface IUserRepository
    {
        Task<(List<UserEntity> Users, int Total)> GetUserAsync(string? keyword, int page, int pageSize);
        Task<Dictionary<string, List<string>>> GetUserRolesAsync(List<UserEntity> users);
    }
}
