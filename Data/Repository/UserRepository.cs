using Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repository
{
    public class UserRepository
    {
        public UserEntity GetUserById(int Id)
        {
            return new UserEntity();
        }
        public List<UserEntity> GetAllUser()
        {
            return new List<UserEntity>();
        }
    }
}
