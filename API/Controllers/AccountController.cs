using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext _context;
        public AccountController(DataContext context)
        {
            _context = context;
        }


        [HttpPost("register")]
        public async Task<ActionResult<AppUser>> Register(RegisterDto registerDto)
        {
            if (await UserExist(registerDto.UserName)) return BadRequest("UserName is taken");
            using var hmac = new HMACSHA512();

            var user = new AppUser{
                UserName = registerDto.UserName.ToLower(),
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
                PasswordSalt = hmac.Key
            };
             _context.Users.Add(user);

             await _context.SaveChangesAsync();

             return user;
        }

        private async Task<bool> UserExist(string userName){

            return await _context.Users.AnyAsync( x => x.UserName == userName.ToLower());
        }

        [HttpPost("login")]
        public async Task<ActionResult<AppUser>> Login(LoginDto logindto){
            var user = await _context.Users.SingleOrDefaultAsync(x => x.UserName == logindto.UserName);

            if(user == null) return Unauthorized("Invalid Username");

             using var hmac = new HMACSHA512(user.PasswordSalt);

             var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(logindto.Password));

             for (int i=0; i < computedHash.Length; i++)
             {
                 if(computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid Password");
             }
            return user;

        }
    }
}