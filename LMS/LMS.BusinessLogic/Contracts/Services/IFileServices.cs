using LMS.BusinessLogic.DTOs.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.Contracts.Services
{
    public interface IFileService
    {
        Task<FileResult?> GetCourseThumbnailAsync(string fileName);
        Task<FileResult?> GetDefaultThumbnailAsync();
        Task<FileOperationResponseDTO> DeleteCourseThumbnailAsync(string fileName);
        Task<FileUploadResponseDTO> SaveCourseThumbnailAsync(IFormFile file);
    }

}
