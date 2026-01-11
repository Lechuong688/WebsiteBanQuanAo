using Data.Entity;
using Data.Repository.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Service.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        public AuthService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        //public UserEntity? Login(string username, string password)
        //{
        //    var user = _userRepository.GetByUsername(username);

        //    if (user == null)
        //        return null;

        //    //if (user.Password.Trim() != password.Trim())
        //    //    return null;

        //    return user;
        //}
    }
}
