using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
      
        private readonly IUserRepository userRepository;

        public UsersController(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }

        [HttpGet]
      
        public async Task<ActionResult<IEnumerable<AppUser>>> GetUsers()
        {
            return Ok(await this.userRepository.GetUsersAsync());
        }

        /// api/users/id
        [HttpGet("{id}")]
   
        public async Task<ActionResult<AppUser>> GetUserById(int id)
        {
            return await this.userRepository.GetUserByIdAsync(id);
        }


         [HttpGet("{username}")]
   
        public async Task<ActionResult<AppUser>> GetUserByUsername(string username)
        {
            return await this.userRepository.GetUserByUsernameAsync(username);
        }
    }
}