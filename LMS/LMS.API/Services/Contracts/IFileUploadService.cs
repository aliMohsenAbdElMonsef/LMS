using Microsoft.AspNetCore.Mvc;

public interface IFileUploadService
{
    Task<string> UploadAssignmentFileAsync(IFormFile file, string assignmentId);
    Task<string> UploadSubmissionFileAsync(IFormFile file, string assignmentId, string studentId);
    Task<FileStreamResult?> DownloadFileAsync(string filePath);
    Task<bool> DeleteFileAsync(string filePath);
}