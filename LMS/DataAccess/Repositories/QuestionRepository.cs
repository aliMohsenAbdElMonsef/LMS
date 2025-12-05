using DataAccess.Context;
using Domain.Entities.MainEntities;
using LMS.DataAccess.Contracts.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.DataAccess.Repositories
{
    internal class QuestionRepository: BaseRepository<Question, string>, IQuestionRepository
    {
        public QuestionRepository(LMSDbContext db) : base(db)
        {
        }
    }
}
