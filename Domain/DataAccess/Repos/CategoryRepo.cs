using DataAccess.Context;
using Domain.Entities.MainEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repos
{
    internal class CategoryRepo : BaseRepo<Category>, ICategoryRepo
    {
        public CategoryRepo(ApplicationDbContext db) : base(db)
        {
        }
    }
}
