using System;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    public class MessageHub : Hub
    {
        private readonly IMapper mapper;
        private readonly IUserRepository userRepository;
        private readonly IMessageRepository messageRepository;
        public MessageHub(IMessageRepository messageRepository, IMapper mapper,
        IUserRepository userRepository)
        {
            this.messageRepository = messageRepository;
            this.mapper = mapper;
            this.userRepository = userRepository;
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var otherUser = httpContext.Request.Query["user"].ToString();
            var groupName = GetGroupName(Context.User.GetUsername(),otherUser);
            await Groups.AddToGroupAsync(Context.ConnectionId,groupName);
            await AddToGroup(Context,groupName);

            var messages = await this.messageRepository
            .GetMessageThread(Context.User.GetUsername(),otherUser);

             await Clients.Group(groupName).SendAsync("ReceiveMessageThread",messages);

        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await RemoveFromMessageGroup(Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);

        }

        public async Task SendMessage(CreateMessageDto createMessageDto)
        {
              var username = Context.User.GetUsername();

            if(username == createMessageDto.RecipientUsername.ToLower()) throw new HubException ("You cannot send messages to yourself");

            
            var sender = await this.userRepository.GetUserByUsernameAsync(username);

            var recipient = await this.userRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

            if(recipient == null)  throw new HubException ("Not found user");

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessageDto.Content
            };

            var groupName = GetGroupName(sender.UserName, recipient.UserName);

            var group = await this.messageRepository.GetMessageGroup(groupName);

            if(group.Connections.Any(x=> x.Username == recipient.UserName))
            {
                message.DateRead = DateTime.UtcNow;
            }

            this.messageRepository.AddMessage(message);

            if(await this.messageRepository.SaveAllAsync())
             {
                 await Clients.Group(groupName).SendAsync("NewMessage", this.mapper.Map<MessageDto>(message));
             }

           
        }

        private async Task<bool> AddToGroup(HubCallerContext context, string groupName)
        {
            var group = await this.messageRepository.GetMessageGroup(groupName);
            var connetion = new Connection(Context.ConnectionId, Context.User.GetUsername());

            if(group == null){
                group = new Group(groupName);
                this.messageRepository.AddGroup(group);
            }

            group.Connections.Add(connetion);

            return await this.messageRepository.SaveAllAsync();

        }


        private async Task RemoveFromMessageGroup(string connectionId)
        {
            var connection = await this.messageRepository.GetConnection(connectionId);

            this.messageRepository.RemoveConnection(connection);

            await this.messageRepository.SaveAllAsync();
        }


        private string GetGroupName (string caller, string other)
        {
            var stringCompare = string.CompareOrdinal(caller,other) < 0;
            return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
        }

    }
}