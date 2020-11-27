using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class MessagesController : BaseApiController
    {

        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;

        public MessagesController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;

            this.mapper = mapper;
        }


        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
        {
            var username = User.GetUsername();

            if (username == createMessageDto.RecipientUsername.ToLower()) return BadRequest("You cannot send messages to yourself");


            var sender = await this.unitOfWork.userRepository.GetUserByUsernameAsync(username);

            var recipient = await this.unitOfWork.userRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

            if (recipient == null) return NotFound();

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessageDto.Content
            };

            this.unitOfWork.messageRepository.AddMessage(message);

            if (await this.unitOfWork.Complete()) return Ok(this.mapper.Map<MessageDto>(message));

            return BadRequest("Failed to send message");


        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForUser([FromQuery] MessageParams messageParams)
        {
            messageParams.Username = User.GetUsername();
            var messages = await this.unitOfWork.messageRepository.GetMessagesForUser(messageParams);

            Response.AddPaginationHeader(messages.CurrentPage, messages.PageSize,
             messages.TotalCount, messages.TotalPages);

            return messages;

        }


      


        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            var username = User.GetUsername();
            var message = await this.unitOfWork.messageRepository.GetMessage(id);
            if (message.Sender.UserName != username && message.Recipient.UserName != username) return Unauthorized("You cannot delete this message");
            if (message.Sender.UserName == username) message.SenderDeleted = true;
            if (message.RecipientUsername == username) message.RecipientDeleted = true;
            if (message.SenderDeleted && message.RecipientDeleted) this.unitOfWork.messageRepository.DeleteMessage(message);

            if (await this.unitOfWork.Complete()) return Ok();
            return BadRequest("Problem deleting the message");



        }

    }
}