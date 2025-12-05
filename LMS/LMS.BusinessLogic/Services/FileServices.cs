using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.DTOs.Responses;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LMS.BusinessLogic.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public FileService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<FileResult?> GetCourseThumbnailAsync(string fileName)
        {
            try
            {
                var filePath = GetCourseThumbnailPath(fileName);

                if (!System.IO.File.Exists(filePath))
                    return null;

                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                var contentType = GetContentType(filePath);

                return new FileContentResult(fileBytes, contentType);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<FileResult?> GetDefaultThumbnailAsync()
        {
            try
            {
                var defaultPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "default-course-thumbnail.jpg");
                if (!System.IO.File.Exists(defaultPath))
                    return null;

                var fileBytes = await System.IO.File.ReadAllBytesAsync(defaultPath);
                return new FileContentResult(fileBytes, "image/jpeg");
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<FileUploadResponseDTO> SaveCourseThumbnailAsync(IFormFile file)
        {
            var result = new FileUploadResponseDTO();

            try
            {
                if (file == null || file.Length == 0)
                {
                    result.Success = false;
                    result.Message = "No file provided";
                    return result;
                }
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    result.Success = false;
                    result.Message = "Invalid file type. Only image files are allowed.";
                    return result;
                }
                if (file.Length > 5 * 1024 * 1024)
                {
                    result.Success = false;
                    result.Message = "File size cannot exceed 5MB.";
                    return result;
                }

                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "course", "thumbnails");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileNameWithoutExtension(file.FileName)}{fileExtension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                result.Success = true;
                result.FileName = uniqueFileName;
                result.Message = "File uploaded successfully";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Error uploading file: {ex.Message}";
            }

            return result;
        }

        public async Task<FileOperationResponseDTO> DeleteCourseThumbnailAsync(string fileName)
        {
            var result = new FileOperationResponseDTO();

            try
            {
                if (string.IsNullOrEmpty(fileName) || fileName.Contains("..") || Path.IsPathRooted(fileName))
                {
                    result.Success = false;
                    result.Message = "Invalid file name";
                    return result;
                }

                var filePath = GetCourseThumbnailPath(fileName);

                if (!System.IO.File.Exists(filePath))
                {
                    result.Success = false;
                    result.Message = "File not found";
                    return result;
                }

                System.IO.File.Delete(filePath);
                result.Success = true;
                result.Message = "File deleted successfully";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Error deleting file: {ex.Message}";
            }

            return result;
        }

        private string GetCourseThumbnailPath(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName) || fileName.Contains("..") || Path.IsPathRooted(fileName))
                throw new ArgumentException("Invalid file name");

            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "course", "thumbnails");
            return Path.Combine(uploadsFolder, fileName);
        }

        private string GetContentType(string path)
        {
            var extension = Path.GetExtension(path).ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };
        }


        public async Task<FileUploadResponseDTO> SaveLectureMaterialAsync(IFormFile file)
        {
            var result = new FileUploadResponseDTO();

            try
            {
                if (file == null || file.Length == 0)
                {
                    result.Success = false;
                    result.Message = "No file provided";
                    return result;
                }
                var allowedExtensions = new[] { ".zip", ".rar", ".pdf", ".doc", ".docx", ".ppt", ".pptx" };
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    result.Success = false;
                    result.Message = "Invalid file type. Only zip, rar, pdf, doc, ppt files are allowed.";
                    return result;
                }
                if (file.Length > 50 * 1024 * 1024) // 50MB
                {
                    result.Success = false;
                    result.Message = "File size cannot exceed 50MB.";
                    return result;
                }

                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "lectures", "materials");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileNameWithoutExtension(file.FileName)}{fileExtension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                result.Success = true;
                result.FileName = uniqueFileName;
                result.Message = "File uploaded successfully";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Error uploading file: {ex.Message}";
            }

            return result;
        }

        public async Task<FileUploadResponseDTO> SaveLectureRecordingAsync(IFormFile file)
        {
            var result = new FileUploadResponseDTO();

            try
            {
                if (file == null || file.Length == 0)
                {
                    result.Success = false;
                    result.Message = "No file provided";
                    return result;
                }
                var allowedExtensions = new[] { ".mp4", ".mkv", ".avi", ".mov" };
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    result.Success = false;
                    result.Message = "Invalid file type. Only video files are allowed.";
                    return result;
                }
                if (file.Length > 500 * 1024 * 1024) // 500MB
                {
                    result.Success = false;
                    result.Message = "File size cannot exceed 500MB.";
                    return result;
                }

                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "lectures", "recordings");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileNameWithoutExtension(file.FileName)}{fileExtension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                result.Success = true;
                result.FileName = uniqueFileName;
                result.Message = "File uploaded successfully";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Error uploading file: {ex.Message}";
            }

            return result;
        }

        public async Task<FileOperationResponseDTO> DeleteLectureMaterialAsync(string fileName)
        {
            var result = new FileOperationResponseDTO();

            try
            {
                if (string.IsNullOrEmpty(fileName) || fileName.Contains("..") || Path.IsPathRooted(fileName))
                {
                    result.Success = false;
                    result.Message = "Invalid file name";
                    return result;
                }

                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "lectures", "materials");
                var filePath = Path.Combine(uploadsFolder, fileName);

                if (!System.IO.File.Exists(filePath))
                {
                    result.Success = false;
                    result.Message = "File not found";
                    return result;
                }

                System.IO.File.Delete(filePath);
                result.Success = true;
                result.Message = "File deleted successfully";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Error deleting file: {ex.Message}";
            }

            return result;
        }

        public async Task<FileOperationResponseDTO> DeleteLectureRecordingAsync(string fileName)
        {
            var result = new FileOperationResponseDTO();

            try
            {
                if (string.IsNullOrEmpty(fileName) || fileName.Contains("..") || Path.IsPathRooted(fileName))
                {
                    result.Success = false;
                    result.Message = "Invalid file name";
                    return result;
                }

                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "lectures", "recordings");
                var filePath = Path.Combine(uploadsFolder, fileName);

                if (!System.IO.File.Exists(filePath))
                {
                    result.Success = false;
                    result.Message = "File not found";
                    return result;
                }

                System.IO.File.Delete(filePath);
                result.Success = true;
                result.Message = "File deleted successfully";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Error deleting file: {ex.Message}";
            }

            return result;
        }
    }
}