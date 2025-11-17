using LMS.MVC.Models.ViewModels.Assignment;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LMS.MVC.Services.Contracts.Services
{
    /// <summary>
    /// Service for managing assignments in MVC layer
    /// This service communicates with the API layer
    /// </summary>
    public interface IAssignmentService
    {
        // ============== FILE UPLOAD METHODS ==============

        /// <summary>
        /// Creates assignment with file by sending to API
        /// </summary>
        Task<ReadAssignmentResult> CreateAssignmentWithFile(ReadAssignmentResult model, IFormFile file);

        /// <summary>
        /// Submits assignment with file by sending to API
        /// </summary>
        Task<StudentAssignmentResult> SubmitAssignmentWithFile(string assignmentId, string studentId, IFormFile file);

        /// <summary>
        /// Downloads file from API
        /// </summary>
        Task<FileContentResult?> DownloadFileFromApi(string filePath);

        // ============== GET METHODS ==============

        /// <summary>
        /// Gets all assignments for a specific course
        /// </summary>
        Task<IEnumerable<ReadAssignmentResult>> GetAssignmentsByCourse(string courseId);

        /// <summary>
        /// Gets assignment details by ID
        /// </summary>
        Task<AssignmentDetailsResult> GetAssignmentById(string id);

        /// <summary>
        /// Gets student assignment by student assignment ID
        /// </summary>
        Task<StudentAssignmentResult> GetStudentAssignmentById(string studentAssignmentId);

        /// <summary>
        /// Gets student's submission for a specific assignment
        /// Returns null if student hasn't submitted yet
        /// </summary>
        Task<StudentAssignmentResult?> GetStudentAssignment(string assignmentId, string studentId);

        /// <summary>
        /// Gets all submissions for an assignment
        /// </summary>
        Task<IEnumerable<StudentAssignmentResult>> GetAssignmentSubmissions(string assignmentId);

        /// <summary>
        /// Gets submissions filtered by status
        /// </summary>
        Task<IEnumerable<StudentAssignmentResult>> GetSubmissionsByStatus(string assignmentId, string status);

        /// <summary>
        /// Gets assignment data for editing
        /// </summary>
        Task<ReadAssignmentResult?> GetEditModel(string id);

        /// <summary>
        /// Gets empty model for creating new assignment
        /// </summary>
        ReadAssignmentResult GetCreateModel();

        // ============== CRUD METHODS ==============

        /// <summary>
        /// Creates assignment (without file)
        /// </summary>
        Task<ReadAssignmentResult> CreateAssignment(ReadAssignmentResult model);

        /// <summary>
        /// Updates assignment
        /// </summary>
        Task<ReadAssignmentResult> EditAssignment(ReadAssignmentResult model);

        /// <summary>
        /// Deletes assignment and all associated files
        /// </summary>
        Task<bool> DeleteAssignment(string id);

        // ============== SUBMISSION & GRADING ==============

        /// <summary>
        /// Submits assignment (without file)
        /// </summary>
        Task<StudentAssignmentResult> SubmitAssignment(StudentAssignmentResult model);

        /// <summary>
        /// Grades a student assignment
        /// </summary>
        Task<StudentAssignmentResult> GradeAssignment(StudentAssignmentResult model);

        // ============== DEPRECATED METHODS ==============
        // These are kept for backward compatibility but should not be used

        /// <summary>
        /// [DEPRECATED] Use DownloadFileFromApi instead
        /// </summary>
        [Obsolete("Use DownloadFileFromApi instead")]
        Task<FileResult> DownloadAssignmentFileAsync(string filePath, string fileName);

        /// <summary>
        /// [DEPRECATED] Use DownloadFileFromApi instead
        /// </summary>
        [Obsolete("Use DownloadFileFromApi instead")]
        Task<FileResult> DownloadSubmissionFileAsync(string filePath, string fileName);
        Task<IEnumerable<StudentAssignmentResult>> GetStudentSubmissions(string studentId);
        /// <summary>
        /// Gets all assignments for a student (submitted and not submitted)
        /// </summary>
        Task<List<StudentAssignmentItemResult>> GetStudentAllAssignments(string studentId);
    }
}