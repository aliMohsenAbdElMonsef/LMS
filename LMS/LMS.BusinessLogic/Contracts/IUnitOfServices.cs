using LMS.BusinessLogic.Contracts.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.Contracts
{
    public interface IUnitOfServices
    {
        IAssignmentServices Assignments { get; }
        ICategoryServices Categories { get; }
        ICertificateTemplateServices CertificateTemplates { get; }
        ICourseServices Coures { get; }
        ILectureServices Lectures { get; }
        IQuestionServices Questions { get; }
        IQuizServices Quizzes { get; }
        ISkillServices Skills { get; }
        IUserServices Users { get; }
        IStudentEnrollIntoCourseServices StudentEnrollIntoCourse { get; }
    }
}
