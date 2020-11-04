using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using API.Entities;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext context;

        public UserRepository(DataContext context)
        {
            this.context = context;
        }
        public async Task<AppUser> GetUserByIdAsync(int id)
        {
           return   await this.context.Users.FindAsync(id);
        }

        public async Task<AppUser> GetUserByUsernameAsync(string username)
        {

             return   await this.context.Users.SingleOrDefaultAsync(x=> x.Username == username);

        }

        public async Task<IEnumerable<AppUser>> GetUsersAsync()
        {
            return   await this.context.Users.ToListAsync();
        }

        public async Task<bool> SaveAllAsync()
        {
            return await this.context.SaveChangesAsync() > 0;
        }

        public void Update(AppUser user)
        {
            this.context.Entry(user).State = EntityState.Modified;
        }
    }
}