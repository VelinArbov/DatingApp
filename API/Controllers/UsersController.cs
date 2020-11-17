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
      
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery]UserParams userParams)
        {
            var user = await this.userRepository.GetUserByUsernameAsync(User.GetUsername());
            userParams.CurrentUsername = user.Username;

            if(string.IsNullOrEmpty(userParams.Gender))
            {
                userParams.Gender = user.Gender == "male" ? "female" : "male";
            }

            var users = await this.userRepository.GetMembersAsync(userParams);


            Response.AddPaginationHeader(users.CurrentPage,users.PageSize,users.TotalCount,users.TotalPages);

            return Ok(users);
        }

   

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

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var user = await this.userRepository.GetUserByUsernameAsync(User.GetUsername());

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if(photo.IsMain) return BadRequest("This is already main photo");

            var currentMain = user.Photos.FirstOrDefault(x=> x.IsMain);

            if(currentMain != null) currentMain.IsMain= false;
            photo.IsMain = true;

            if(await this.userRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Failed to set main photo");


        }


        [HttpDelete("delete-photo/{photoId}")]
         public async Task<ActionResult> DeletePhoto(int photoId)
         {
               var user = await this.userRepository.GetUserByUsernameAsync(User.GetUsername());

               var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

               if( photo == null) return NotFound();

               if(photo.IsMain) return BadRequest("You cannot delete your main photo");

               if(photo.PublicId != null)
                {
                    var result = await this.photoService.DeletePhotoAsync(photo.PublicId);

                    if(result.Error != null) return BadRequest(result.Error.Message);
                }

                user.Photos.Remove(photo);

                if(await this.userRepository.SaveAllAsync()) return Ok();

                return BadRequest("Failed to delete this photo");
         }

        
    }
}