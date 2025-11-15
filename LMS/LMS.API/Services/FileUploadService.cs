using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Services
{
   

    public class FileUploadService : IFileUploadService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly long _maxFileSize = 10 * 1024 * 1024; // 10MB
        private readonly string[] _allowedExtensions = { ".pdf", ".doc", ".docx", ".txt", ".zip", ".rar", ".jpg", ".png" };

        public FileUploadService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> UploadAssignmentFileAsync(IFormFile file, string assignmentId)
        {
            ValidateFile(file);

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "assignments");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{assignmentId}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return $"/uploads/assignments/{uniqueFileName}";
        }

        public async Task<string> UploadSubmissionFileAsync(IFormFile file, string assignmentId, string studentId)
        {
            ValidateFile(file);

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "submissions");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{assignmentId}_{studentId}_{DateTime.UtcNow:yyyyMMddHHmmss}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return $"/uploads/submissions/{uniqueFileName}";
        }

        public async Task<FileStreamResult?> DownloadFileAsync(string filePath)
        {
            var fullPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));

            if (!File.Exists(fullPath))
                return null;

            var memory = new MemoryStream();
            using (var stream = new FileStream(fullPath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            var contentType = GetContentType(fullPath);
            var fileName = Path.GetFileName(fullPath);

            return new FileStreamResult(memory, contentType)
            {
                FileDownloadName = fileName
            };
        }

        public async Task<bool> DeleteFileAsync(string filePath)
        {
            try
            {
                var fullPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private void ValidateFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty");

            if (file.Length > _maxFileSize)
                throw new ArgumentException($"File size exceeds {_maxFileSize / 1024 / 1024}MB limit");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
                throw new ArgumentException($"File type {extension} is not allowed");
        }

        private string GetContentType(string path)
        {
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return ext switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".txt" => "text/plain",
                ".zip" => "application/zip",
                ".rar" => "application/x-rar-compressed",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => "application/octet-stream"
            };
        }
    }
}