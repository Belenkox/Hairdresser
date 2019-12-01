using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Hairdresser.Data;
using Hairdresser.Helpers;
using Hairdresser.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Hairdresser.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly HairdresserContext _contextDb;

        private readonly AppSettings _appSettings;

        public UsersController(IOptions<AppSettings> appSettings, HairdresserContext contextDb)
        {
            _contextDb = contextDb;
            _appSettings = appSettings.Value;
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody]User userParam)
        {
            var user = GenerateToken(userParam.Username, userParam.Password);

            if (user == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            return Ok(user);
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _contextDb.Users.ToListAsync();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            if (_contextDb.Users.Where(x => x.Username == user.Username).FirstOrDefault() != null) //patikrina ar nera jau tokio email
            {
                return ValidationProblem();
            }
            _contextDb.Users.Add(user);
            await _contextDb.SaveChangesAsync();

            return Ok("user created");
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("drtfygbhyjuiuhyghbjkniuhygjbhkjnlkmjuhyguvhjbknlmhgjhtututtuut")),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }

        public User GenerateToken(string username, string password)
        {
            var user = _contextDb.Users.SingleOrDefault(x => x.Username == username && x.Password == password);
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_appSettings.Secret));

            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.Role, _contextDb.Roles.SingleOrDefault(x => x.ID == user.fk_Role).Role));
            claims.Add(new Claim("id", user.Id.ToString()));

            var jwt = new JwtSecurityToken(issuer: "Blinkingcaret",
                audience: "Everyone",
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(60),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.WriteToken(jwt);
            user.Token = token;

            user.Password = null;
            return user;
        }

        /*public string GenerateToken(IEnumerable<Claim> claims)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appSettings.Secret));

            var jwt = new JwtSecurityToken(issuer: "Blinkingcaret",
                audience: "Everyone",
                claims: claims, //the user's claims, for example new Claim[] { new Claim(ClaimTypes.Name, "The username"), //... 
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(5),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(jwt); //the method is called WriteToken but returns a string
        }*/

        /*[HttpPost("refresh")]
        public IActionResult Refresh(string token, string refreshToken)
        {
            var principal = GetPrincipalFromExpiredToken(token);
            var username = principal.Identity.Name;
            var savedRefreshToken = GetRefreshToken(username); //retrieve the refresh token from a data store
            if (savedRefreshToken != refreshToken)
                throw new SecurityTokenException("Invalid refresh token");

            var identity = HttpContext.User.Identity as ClaimsIdentity;
            

            var newJwtToken = GenerateToken(principal.Claims);
            var newRefreshToken = GenerateRefreshToken();
            DeleteRefreshToken(username, refreshToken);
            SaveRefreshToken(username, newRefreshToken);

            return new ObjectResult(new
            {
                token = newJwtToken,
                refreshToken = newRefreshToken
            });
        }*/
    }
}