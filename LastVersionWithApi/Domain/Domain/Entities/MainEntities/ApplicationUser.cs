using Domain.Entities.RelationTables;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.MainEntities
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? UserImage { get; set; }
        public UserType ApplyAs { get; set; }
        public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;

        //Relations
        //course
            // instructor
        public ICollection<InstructorCourse> Courses { get; set; } = new List<InstructorCourse>();
            // student
        public ICollection<StudentEnrollIntoCourse> Enrollment { get; set; } = new List<StudentEnrollIntoCourse>();
            // admin
        public ICollection<Course> CreatedCourses { get; set; } = new List<Course>();

        //Assignemnt
            // instructor
        public ICollection<Assignment> CreatedAssignments { get; set; } = new List<Assignment>();
            // student
        public ICollection<StudentAssignment> UploadedAssignemnts { get; set; } = new List<StudentAssignment>();
        //category
            // admin
        public ICollection<Category> Categories { get; set; } = new List<Category>();

        //certificatetemplate

        public ICollection<CertificateTemplate> certificateTemplates { get; set; } = new List<CertificateTemplate>();

        // lecture
        // instructor
        public ICollection<Lecture> TeachedLectures { get; set; } = new List<Lecture>();
        public ICollection<Lecture> LecturesLastUploaded { get; set; } = new List<Lecture>();
        // student
        public ICollection<StudentLecture> LecturesAttended { get; set; } = new List<StudentLecture>();

        //quiz
        // instructor
        public ICollection<Quiz> CreatedQuizzes { get; set; } = new List<Quiz>();
        // students
        public ICollection<StudentQuiz> QuizzesAttended { get; set; } = new List<StudentQuiz>();
        //skills 
        //admin
        public ICollection<Skills> CreatedSkills { get; set; } = new List<Skills>();

        // question
        // student
        public ICollection<StudentAnswerQuestion> Answers { get; set; } = new List<StudentAnswerQuestion>();

    }
}
