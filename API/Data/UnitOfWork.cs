using System.Threading.Tasks;
using API.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DataContext context;
        private readonly IMapper mapper;

        public UnitOfWork(DataContext context,
         IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }
        public IUserRepository userRepository => new UserRepository(this.context,this.mapper);

        public ILikesRepository likesRepository => new LikesRepository(this.context,this.mapper);

        public IMessageRepository messageRepository => new MessageRepository(this.context,this.mapper);

        public async Task<bool> Complete()
        {
               return await this.context.SaveChangesAsync() > 0;
        }

        public bool HasChanges()
        {
            return this.context.ChangeTracker.HasChanges();
        }
    }
}