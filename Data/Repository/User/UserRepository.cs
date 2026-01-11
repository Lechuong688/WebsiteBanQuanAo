using Data.Entity;
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
        public UserRepository(DataContext context)
        {
            _context = context;
        }

        //public UserEntity? GetUserById(int Id)
        //{
        //    return _context.User.FirstOrDefault(x => x.Id == Id);
        //}
        //public List<UserEntity> GetAllUser()
        //{
        //    return _context.User.ToList();
        //}
        //public UserEntity? GetByUsername(string username)
        //{
        //    return _context.User
        //                   .FirstOrDefault(x => x.UserName == username);
        //}
    }
}
