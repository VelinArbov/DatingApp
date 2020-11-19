using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DataContext context;
        private readonly IMapper mapper;

        public MessageRepository(DataContext context, IMapper mapper)
        {
            
            this.context = context;
            this.mapper = mapper;
        }
        public void AddMessage(Message message)
        {
            this.context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            this.context.Messages.Remove(message);
        }

        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUsername, string recipientUsername)
        {
           var messages = await this.context.Messages
           .Include(u => u.Sender).ThenInclude(p => p.Photos)
           .Include(u => u.Recipient).ThenInclude(p => p.Photos)
           .Where(x=> x.Recipient.Username == currentUsername
                    && x.Sender.Username == recipientUsername
                    || x.Recipient.Username == recipientUsername
                    && x.Sender.Username == currentUsername
            )
            .OrderBy(m => m.MessageSent)
            .ToListAsync();


            var unreadMessages = messages.Where(m=> m.DateRead == null 
            && m.Recipient.Username == currentUsername).ToList();

            if(unreadMessages.Any())
            {
                foreach (var message in messages)
                {
                    message.DateRead = DateTime.Now;
                }

                await this.context.SaveChangesAsync();

            }

            return this.mapper.Map<IEnumerable<MessageDto>>(messages);
        }

        public async Task<Message> GetMessage(int id)
        {
            return await this.context.Messages.FindAsync(id);
        }

        public async Task<PageList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
        {
            var query = this.context.Messages
            .OrderByDescending(m => m.MessageSent)
            .AsQueryable();

            query = messageParams.Container switch
            {
                "Inbox" => query.Where(u => u.Recipient.Username == messageParams.Username),
                "Outbox" => query.Where(u => u.Sender.Username == messageParams.Username),
                _ => query.Where(u => u.Recipient.Username == messageParams.Username && u.DateRead == null)


            };

            var messages = query.ProjectTo<MessageDto>(this.mapper.ConfigurationProvider);

            return await PageList<MessageDto>.CreateAsync(messages,messageParams.PageNumber,messageParams.PageSize);


        }

        public async Task<bool> SaveAllAsync()
        {
            return await this.context.SaveChangesAsync() > 0;
        }
    }
}