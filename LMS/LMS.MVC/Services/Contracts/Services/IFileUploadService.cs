using Microsoft.AspNetCore.Http;

namespace LMS.MVC.Services.Contracts.Services
{
    public interface IFileUploadService
    {
        Task<string> UploadAssignmentFileAsync(IFormFile file, string assignmentId);
        Task<string> UploadSubmissionFileAsync(IFormFile file, string assignmentId, string studentId);
        Task<bool> DeleteFileAsync(string filePath);
    }
}