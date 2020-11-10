using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using API.Entities;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
      
        private readonly IUserRepository userRepository;
        private readonly IMapper mapper;
        private readonly IPhotoService photoService;

        public UsersController(IUserRepository userRepository , IMapper mapper, IPhotoService photoService)
        {
            this.userRepository = userRepository;
            this.mapper = mapper;
            this.photoService = photoService;
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


         [HttpGet("{username}",Name = "GetUser")]
   
        public async Task<ActionResult<MemberDto>> GetUserByUsername(string username)
        {
            return await this.userRepository.GetMemberAsync(username);
            

           
        }

        [HttpPut]

        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            var user = await this.userRepository.GetUserByUsernameAsync(User.GetUsername());

            this.mapper.Map(memberUpdateDto,user);

            this.userRepository.Update(user);

            if(await this.userRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Failed to update user");
        }


        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
             var user = await this.userRepository.GetUserByUsernameAsync(User.GetUsername());

             var result = await this.photoService.AddPhotoAsync(file);

             if(result.Error != null) return BadRequest(result.Error.Message);

             var photo = new Photo
             {
                 Url = result.SecureUrl.AbsoluteUri,
                 PublicId = result.PublicId
             };

             if( user.Photos.Count == 0)
             {
                 photo.IsMain = true;
             }
             user.Photos.Add(photo);

             if(await this.userRepository.SaveAllAsync())
             {
                 return CreatedAtRoute("GetUser",new {username = user.Username},this.mapper.Map<PhotoDto>(photo));
         
             }
            

             return BadRequest("Problem adding photo");
        }
    }
}