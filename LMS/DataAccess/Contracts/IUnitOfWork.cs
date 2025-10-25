using LMS.DataAcess.Contracts.Repositories;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.DataAcess.Contracts
{
    public interface IUnitOfWork
    {
        IAssignmentRepository Assignments { get; }

        ICategoryRepository Categories { get; }

        ICertificateTemplateRepository CertificateTemplates { get; }

        ICourseRepository Coures { get; }

        ILectureRepository Lectures { get; }
        ICourseDayScheduleRepository DaySchedules { get; }

        IQuestionRepository Questions { get; }

        IQuizRepository Quizzes { get; }

        ISkillRepository Skills { get; }

        IUserRepository Users { get; }
        IInstructorEnrolltoCourseRepository InstructorEnrollments { get; }

        IStudentEnrollIntoCourseRepository StudentEnrollments { get; }

        int SaveChanges();
        Task<IDbContextTransaction> BeginTransactionAsync();

        Task<int> SaveChangesAsync();
    }
}
