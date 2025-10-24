using DataAccess.Context;
using LMS.DataAcess.Contracts;
using LMS.DataAcess.Contracts.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.DataAcess.Repositories
{
    internal class UnitOfWork : IUnitOfWork
    {
        private readonly LMSDbContext _db;
        private readonly Lazy<IAssignmentRepository> _assignments;
        private readonly Lazy<ICategoryRepository> _categories;
        private readonly Lazy<ICertificateTemplateRepository> _certificateTemplates;
        private readonly Lazy<ICourseRepository> _courses;
        private readonly Lazy<ILectureRepository> _lectures;
        private readonly Lazy<IQuestionRepository> _questions;
        private readonly Lazy<IQuizRepository> _quizzes;
        private readonly Lazy<ISkillRepository> _skills;
        private readonly Lazy<IUserRepository> _users;
        private readonly Lazy<IInstructorEnrolltoCourseRepository> _instructorEnrollments;
        private readonly Lazy<IStudentEnrollIntoCourseRepository> _studentEnrollments;

        public UnitOfWork(LMSDbContext db)
        {
            _db = db;
            _assignments = new Lazy<IAssignmentRepository>(() => new AssignmentRepository(_db));
            _categories = new Lazy<ICategoryRepository>(() => new CategoryRepository(_db));
            _certificateTemplates = new Lazy<ICertificateTemplateRepository>(() => new CertificateTemplateRepository(_db));
            _courses = new Lazy<ICourseRepository>(() => new CourseRepository(_db));
            _lectures = new Lazy<ILectureRepository>(() => new LectureRepository(_db));
            _questions = new Lazy<IQuestionRepository>(() => new QuestionRepository(_db));
            _quizzes = new Lazy<IQuizRepository>(() => new QuizRepository(_db));
            _skills = new Lazy<ISkillRepository>(() => new SkillRepository(_db));
            _users = new Lazy<IUserRepository>(() => new UserRepository(_db));
            _instructorEnrollments = new Lazy<IInstructorEnrolltoCourseRepository>(() => new InstructorEnrolltoCourseRepository(_db));
            _studentEnrollments = new Lazy<IStudentEnrollIntoCourseRepository>(() => new StudentEnrollIntoCourseRepository(_db));
        }

        public IAssignmentRepository Assignments => _assignments.Value;
        public ICategoryRepository Categories => _categories.Value;
        public ICertificateTemplateRepository CertificateTemplates => _certificateTemplates.Value;
        public ICourseRepository Coures => _courses.Value; 
        public ILectureRepository Lectures => _lectures.Value;
        public IQuestionRepository Questions => _questions.Value;
        public IQuizRepository Quizzes => _quizzes.Value;
        public ISkillRepository Skills => _skills.Value;
        public IUserRepository Users => _users.Value;
        public IInstructorEnrolltoCourseRepository InstructorEnrollments => _instructorEnrollments.Value;
        public IStudentEnrollIntoCourseRepository StudentEnrollments => _studentEnrollments.Value;


        public int SaveChanges()
        {
            return _db.SaveChanges();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _db.SaveChangesAsync();
        }

    }
}