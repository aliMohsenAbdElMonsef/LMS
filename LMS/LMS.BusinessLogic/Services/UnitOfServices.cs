using LMS.BusinessLogic.Contracts;
using LMS.BusinessLogic.Contracts.Services;
using LMS.DataAcess.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.Services
{
    internal class UnitOfServices : IUnitOfServices
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly Lazy<IAssignmentServices> _assignmentServices;
        private readonly Lazy<ICategoryServices> _categoryServices;
        private readonly Lazy<ICertificateTemplateServices> _certificateTemplateServices;
        private readonly Lazy<ICourseServices> _courseServices;
        private readonly Lazy<ILectureServices> _lectureServices;
        private readonly Lazy<IQuestionServices> _questionServices;
        private readonly Lazy<IQuizServices> _quizServices;
        private readonly Lazy<ISkillServices> _skillServices;
        private readonly Lazy<IUserServices> _userServices;
        public UnitOfServices(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _assignmentServices = new Lazy<IAssignmentServices>(() => new AssignmentServices(_unitOfWork));
            _categoryServices = new Lazy<ICategoryServices>(() => new CategoryServices(_unitOfWork));
            _courseServices = new Lazy<ICourseServices>(() => new CourseServices(_unitOfWork));
        }

        public IAssignmentServices Assignments => _assignmentServices.Value;

        public ICategoryServices Categories => _categoryServices.Value;

        public ICertificateTemplateServices CertificateTemplates => _certificateTemplateServices.Value;

        public ICourseServices Courses => _courseServices.Value;

        public ILectureServices Lectures => _lectureServices.Value;

        public IQuestionServices Questions => _questionServices.Value;

        public IQuizServices Quizzes => _quizServices.Value;

        public ISkillServices Skills => _skillServices.Value;

        public IUserServices Users => _userServices.Value;
    }
}
