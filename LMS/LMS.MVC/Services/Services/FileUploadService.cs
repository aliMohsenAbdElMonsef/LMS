using LMS.MVC.Services.Contracts.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace LMS.MVC.Services.Services
{
    public class FileUploadService : IFileUploadService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public FileUploadService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<string> UploadAssignmentFileAsync(IFormFile file, string assignmentId)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("No file provided");

            var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".txt" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(fileExtension))
                throw new ArgumentException("Invalid file type. Only PDF, Word, and Text files are allowed.");

            if (file.Length > 10 * 1024 * 1024) // 10MB
                throw new ArgumentException("File size cannot exceed 10MB.");

            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "assignments", assignmentId);
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileNameWithoutExtension(file.FileName)}{fileExtension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return $"/uploads/assignments/{assignmentId}/{uniqueFileName}";
        }

        public async Task<string> UploadSubmissionFileAsync(IFormFile file, string assignmentId, string studentId)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("No file provided");

            var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".txt" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(fileExtension))
                throw new ArgumentException("Invalid file type. Only PDF, Word, and Text files are allowed.");

            if (file.Length > 10 * 1024 * 1024) // 10MB
                throw new ArgumentException("File size cannot exceed 10MB.");

            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "submissions", assignmentId, studentId);
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileNameWithoutExtension(file.FileName)}{fileExtension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return $"/uploads/submissions/{assignmentId}/{studentId}/{uniqueFileName}";
        }

        public async Task<bool> DeleteFileAsync(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath) || filePath.Contains("..") || Path.IsPathRooted(filePath))
                    return false;

                var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, filePath.TrimStart('/'));
                if (!System.IO.File.Exists(fullPath))
                    return false;

                System.IO.File.Delete(fullPath);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}