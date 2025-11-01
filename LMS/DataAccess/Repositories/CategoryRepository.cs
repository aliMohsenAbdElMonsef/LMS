using DataAccess.Context;
using Domain.Entities.MainEntities;
using LMS.DataAccess.Contracts.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.DataAccess.Repositories
{
    internal class CategoryRepository: BaseRepository<Category,string>, ICategoryRepository
    {
        public CategoryRepository(LMSDbContext context) : base(context)
        {
        }
        public override async Task<Category?> FindByIdAsync(string id)
        {
            if (id == null) return null;
            var entity = await _set.Include(c => c.Admin)
                                   .FirstOrDefaultAsync(c => c.Id == id);
            return entity;
        }
        public override async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _set.Include(c => c.Admin)
                             .Include(c => c.Courses) 
                             .ToListAsync();
        }

    }
}
