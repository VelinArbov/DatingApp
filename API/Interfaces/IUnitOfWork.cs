using System.Threading.Tasks;

namespace API.Interfaces
{
    public interface IUnitOfWork
    {
        
         IUserRepository userRepository {get;}

         ILikesRepository likesRepository { get;}

         IMessageRepository messageRepository { get;}

         Task<bool> Complete();

         bool HasChanges();

    }
}