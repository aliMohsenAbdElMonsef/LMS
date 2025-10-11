using DataAccess.Context;
using Domain.Entities.MainEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repos
{
    internal class ApplicationUserRepo : BaseRepo<ApplicationUser>, IApplicationUserRepo
    {
        public ApplicationUserRepo(ApplicationDbContext db) : base(db)
        {
        }
    }
}
