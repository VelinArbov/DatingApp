using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly DataContext context;
        public UsersController(DataContext context)
        {
            this.context = context;
        }

        [HttpGet]
      
        public async Task<ActionResult<IEnumerable<AppUser>>> GetUsers()
        {
            return await context.Users.ToListAsync();
        }

        /// api/users/id
        [HttpGet("{id}")]
   
        public async Task<ActionResult<AppUser>> GetUserById(int id)
        {
            return await this.context.Users.FindAsync(id);
        }
    }
}