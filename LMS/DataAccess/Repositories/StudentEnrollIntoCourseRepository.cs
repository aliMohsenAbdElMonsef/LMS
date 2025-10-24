using DataAccess.Context;
using Domain.Entities.RelationTables;
using LMS.DataAcess.Contracts;
using LMS.DataAcess.Contracts.Repositories;
using LMS.Entity.Entities.RelationTables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.DataAcess.Repositories
{
    internal class StudentEnrollIntoCourseRepository : BaseRepository<StudentEnrollIntoCourse, string>,
        IStudentEnrollIntoCourseRepository
    {
        public StudentEnrollIntoCourseRepository(LMSDbContext context) : base(context)
        {


        }

      
    }
}
