using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
      
        private readonly IUserRepository userRepository;
        private readonly IMapper mapper;

        public UsersController(IUserRepository userRepository , IMapper mapper)
        {
            this.userRepository = userRepository;
            this.mapper = mapper;
        }

        [HttpGet]
      
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
        {
            var users = await this.userRepository.GetMembersAsync();

            return Ok(users);
        }

        /// api/users/id
        // [HttpGet("{id}")]
   
        // public async Task<ActionResult<AppUser>> GetUserById(int id)
        // {
        //     return await this.userRepository.GetUserByIdAsync(id);
        // }


         [HttpGet("{username}")]
   
        public async Task<ActionResult<MemberDto>> GetUserByUsername(string username)
        {
            return await this.userRepository.GetMemberAsync(username);

           
        }
    }
}