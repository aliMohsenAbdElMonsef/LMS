using DataAccess.Context;
using Domain.Entities.MainEntities;
using LMS.DataAcess.Contracts.Repositories;


namespace LMS.DataAcess.Repositories
{
    internal class UserRepository: BaseRepository<ApplicationUser,string>,IUserRepository
    {

        public UserRepository(LMSDbContext db) : base(db)
        {
        }

       
    }
}
