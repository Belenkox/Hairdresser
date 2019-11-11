using Hairdresser.Data;
using Hairdresser.Helpers;
using Hairdresser.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Hairdresser.Services
{
    public interface IUserService
    {
        User Authenticate(string username, string password);
        IEnumerable<User> GetAll();
    }

    public class UserService : IUserService
    {
        private readonly HairdresserContext _contextDb;
        // users hardcoded for simplicity, store in a db with hashed passwords in production applications
        private List<User> _users = new List<User>
        {
            new User {Id = 1, First_Name = "test", Last_Name = "User", Username = "test", Password = "test"}
        };

        private readonly AppSettings _appSettings;

        public UserService(IOptions<AppSettings> appSettings, HairdresserContext contextDb)
        {
            _contextDb = contextDb;
            _appSettings = appSettings.Value;
        }

        public User Authenticate(string username, string password)
        {
            var user = _contextDb.Users.SingleOrDefault(x => x.Username == username && x.Password == password);

            // return null if user not found
            if (user == null)
                return null;

            // authentication successful so generate jwt token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Role, _contextDb.Roles.SingleOrDefault(x => x.ID == user.fk_Role).Role)
                }),
                Expires = DateTime.UtcNow.AddMinutes(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            user.Token = tokenHandler.WriteToken(token);

            //remove password before returning
            user.Password = null;
            return user;

        }

        public IEnumerable<User> GetAll()
        {
            // return users without passwords
            return _contextDb.Users.ToList();/*(x =>
            {
                x.Password = null;
                return x;
            });*/
        }
    }
}
