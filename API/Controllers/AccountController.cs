using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using API.Interfaces;
using AutoMapper;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext context;
        private readonly ITokenService tokenService;
        private readonly IMapper mapper;

        public AccountController(DataContext context, ITokenService tokenService, IMapper mapper)
        {
            this.tokenService = tokenService;
            this.mapper = mapper;
            this.context = context;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            
            if (await UserExist(registerDto.Username))
            {
                return BadRequest("Username is taken");
            }
            var user = this.mapper.Map<AppUser>(registerDto);
            using var hmac = new HMACSHA512();



           

                user.Username = registerDto.Username.ToLower();
                user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password));
                user.PasswordSalt = hmac.Key;

           

            await this.context.Users.AddAsync(user);
            await this.context.SaveChangesAsync();

            var userDto = new UserDto ();
        
               userDto.Username = user.Username;
               userDto.Token = this.tokenService.CreateToken(user);
               userDto.KnowAs = user.KnowAs;
               userDto.Gender = user.Gender;
            

            return userDto;
            
        }


        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {

            var user = await this.context.Users
            .Include(p=> p.Photos)
            .SingleOrDefaultAsync(x => x.Username == loginDto.Username);

            if (user == null) return Unauthorized("Invalid username!");

            using var hmac = new HMACSHA512(user.PasswordSalt);

            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid password!");
            }


            return new UserDto
            {
                Username = user.Username,
                Token = this.tokenService.CreateToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                KnowAs= user.KnowAs,
                Gender = user.Gender

            };
        }

        private async Task<bool> UserExist(string username)
        {
            return await this.context.Users.AnyAsync(x => x.Username.ToLower() == username.ToLower());

        }
    }
}