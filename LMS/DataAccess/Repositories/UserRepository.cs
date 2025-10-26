using DataAccess.Context;
using Domain.Entities.MainEntities;
using LMS.DataAccess.Contracts.Repositories;


namespace LMS.DataAccess.Repositories
{
    internal class UserRepository: BaseRepository<ApplicationUser,string>,IUserRepository
    {

        public UserRepository(LMSDbContext db) : base(db)
        {
        }

       
    }
}
