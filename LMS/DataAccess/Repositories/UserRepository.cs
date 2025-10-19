using DataAccess.Context;
using Domain.Entities.MainEntities;
using LMS.DataAcess.Contracts.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LMS.DataAcess.Repositories
{
    internal class UserRepository: BaseRepository<ApplicationUser,string>,IUserRepository
    {

        public UserRepository(LMSDbContext db) : base(db)
        {
        }

       
    }
}
