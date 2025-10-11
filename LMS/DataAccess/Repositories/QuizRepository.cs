using DataAccess.Context;
using Domain.Entities.MainEntities;
using LMS.DataAcess.Contracts.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.DataAcess.Repositories
{
    internal class QuizRepository: BaseRepository<Quiz, string>, IQuizRepository
    {
        public QuizRepository(LMSDbContext db) : base(db)
        { 
        }
        
    }
}
