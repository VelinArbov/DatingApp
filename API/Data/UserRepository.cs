using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using API.Entities;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;
using API.DTOs;
using AutoMapper.QueryableExtensions;
using AutoMapper;
using API.Helpers;
using System;

namespace API.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext context;
        private readonly IMapper mapper;

        public UserRepository(DataContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        public async Task<MemberDto> GetMemberAsync(string username)
        {
            return await this.context.Users
            .Where(x=> x.Username == username)
            .ProjectTo<MemberDto>(this.mapper.ConfigurationProvider)
            .SingleOrDefaultAsync();
        }

        public async Task<PageList<MemberDto>> GetMembersAsync(UserParams userParams)
        {
           var query =  this.context.Users
            .AsQueryable();

            query = query.Where( x=> x.Username != userParams.CurrentUsername);
            query = query.Where(x=> x.Gender == userParams.Gender);

            var minDob = DateTime.Today.AddYears(-userParams.MaxAge - 1);
            var maxDob = DateTime.Today.AddYears(-userParams.MinAge);


            query = query.Where(x=> x.DateOfBirth >= minDob && x.DateOfBirth <= maxDob);

            query = userParams.OrderBy switch
            {
                "created" => query.OrderByDescending(u => u.Created),
                _ => query.OrderByDescending(x=> x.LastActive)
            };



            return await PageList<MemberDto>.CreateAsync(query.ProjectTo<MemberDto>(this.mapper.ConfigurationProvider)
            .AsNoTracking(),
            userParams.PageNumber,
            userParams.PageSize);
        }

        public async Task<AppUser> GetUserByIdAsync(int id)
        {
           return   await this.context.Users.FindAsync(id);
        }

        public async Task<AppUser> GetUserByUsernameAsync(string username)
        {

             return   await this.context.Users
            .Include(p=> p.Photos)
            .SingleOrDefaultAsync(x=> x.Username == username);

        }

        public async Task<IEnumerable<AppUser>> GetUsersAsync()
        {
            return   await this.context.Users
            .Include(p=> p.Photos)
            .ToListAsync();
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