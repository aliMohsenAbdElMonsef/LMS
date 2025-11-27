using AutoMapper;
using LMS.BusinessLogic.DTOs.Assignment;
using LMS.BusinessLogic.DTOs.Responses;
using LMS.Entity.Enums;
using LMS.MVC.Models.ViewModels.Assignment;
using LMS.MVC.Services.Contracts.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace LMS.MVC.Services.Services
{
    internal class AssignmentService : BaseMVCServices, IAssignmentService
    {
        private readonly IMapper _mapper;
        private readonly HttpClient _client;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ITokenService _tokenService;

        public AssignmentService(
            HttpClient client,
            IHttpContextAccessor accessor,
            ITokenService tokenService,
            IMapper mapper)
            : base(client, accessor)
        {
            _mapper = mapper;
            _tokenService = tokenService;
            _client = client;
            _contextAccessor = accessor;
        }

        // ============== FILE UPLOAD METHODS ==============

        /// <summary>
        /// Creates assignment with file by sending multipart form data to API
        /// </summary>
        public async Task<ReadAssignmentResult> CreateAssignmentWithFile(
            ReadAssignmentResult model,
            IFormFile file)
        {
            Console.WriteLine("=== MVC SERVICE: CreateAssignmentWithFile ===");
            Console.WriteLine($"Model Title: {model.Title}");
            Console.WriteLine($"File Name: {file.FileName}");
            Console.WriteLine($"File Size: {file.Length} bytes");
            Console.WriteLine($"API Base URL: {_client.BaseAddress}");

            using var content = new MultipartFormDataContent();

            content.Add(new StringContent(model.Title ?? ""), "Title");
            content.Add(new StringContent(model.Description ?? ""), "Description");
            content.Add(new StringContent(model.CourseId ?? ""), "CourseId");
            content.Add(new StringContent(model.DueDate.ToString("o")), "DueDate");
            content.Add(new StringContent(_tokenService.GetUserId() ?? ""), "InstructorId");

            var fileContent = new StreamContent(file.OpenReadStream());
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            content.Add(fileContent, "assignmentFile", file.FileName);

            Console.WriteLine("✅ Sending POST to api/assignment/create-with-file");

            var response = await _client.PostAsync("api/assignment/create-with-file", content);

            Console.WriteLine($"Response Status: {response.StatusCode}");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ API Error: {errorContent}");
                throw new Exception($"API Error: {response.StatusCode} - {errorContent}");
            }

            var json = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response JSON: {json.Substring(0, Math.Min(200, json.Length))}...");

            var serviceResponse = JsonConvert.DeserializeObject<ServiceResponseDTO<ReadAssignmentDTO>>(json);

            if (serviceResponse?.Success != true || serviceResponse.Data == null)
            {
                Console.WriteLine($"❌ Service response failed: {serviceResponse?.Message}");
                throw new Exception(serviceResponse?.Message ?? "Failed to create assignment");
            }

            Console.WriteLine($"✅ Assignment created successfully: {serviceResponse.Data.Id}");

            return _mapper.Map<ReadAssignmentResult>(serviceResponse.Data);
        }        /// <summary>
                 /// Submits assignment with file by sending multipart form data to API
                 /// </summary>
        public async Task<StudentAssignmentResult> SubmitAssignmentWithFile(
            string assignmentId,
            string studentId,
            IFormFile file)
        {
            using var content = new MultipartFormDataContent();

            // Add form fields
            content.Add(new StringContent(assignmentId), "assignmentId");

            // Add file
            var fileContent = new StreamContent(file.OpenReadStream());
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            content.Add(fileContent, "submissionFile", file.FileName);

            // Send to API
            var response = await _client.PostAsync("api/assignment/submit-with-file", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"API Error: {response.StatusCode} - {errorContent}");
            }

            var json = await response.Content.ReadAsStringAsync();
            var serviceResponse = JsonConvert.DeserializeObject<ServiceResponseDTO<StudentAssignmentDTO>>(json);

            if (serviceResponse?.Success != true || serviceResponse.Data == null)
            {
                throw new Exception(serviceResponse?.Message ?? "Failed to submit assignment");
            }

            return _mapper.Map<StudentAssignmentResult>(serviceResponse.Data);
        }

        /// <summary>
        /// Downloads file from API
        /// </summary>
        public async Task<FileContentResult?> DownloadFileFromApi(string filePath)
        {
            try
            {
                // Encode the file path for URL
                var encodedPath = Uri.EscapeDataString(filePath);

                // Request file from API
                var response = await _client.GetAsync($"api/assignment/download/{encodedPath}");

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"❌ Download failed: {response.StatusCode}");
                    return null;
                }

                // Read file bytes
                var bytes = await response.Content.ReadAsByteArrayAsync();

                // Get content type
                var contentType = response.Content.Headers.ContentType?.ToString()
                    ?? "application/octet-stream";

                // Get filename from path
                var fileName = Path.GetFileName(filePath);

                return new FileContentResult(bytes, contentType)
                {
                    FileDownloadName = fileName
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error downloading file from API: {ex.Message}");
                return null;
            }
        }

        // ============== GET METHODS ==============

        public async Task<IEnumerable<ReadAssignmentResult>> GetAssignmentsByCourse(string courseId)
        {
            var serviceResponse = await GetAsync<ServiceResponseDTO<IEnumerable<ReadAssignmentDTO>>>(
                $"api/assignment/course/{courseId}");
            var assignments = serviceResponse?.Data ?? Enumerable.Empty<ReadAssignmentDTO>();
            return _mapper.Map<IEnumerable<ReadAssignmentResult>>(assignments);
        }

        public async Task<AssignmentDetailsResult> GetAssignmentById(string id)
        {
            var response = await GetAsync<ServiceResponseDTO<AssignmentDetailsDTO>>(
                $"api/assignment/details/{id}");
            var assignment = response?.Data ?? new AssignmentDetailsDTO();
            return _mapper.Map<AssignmentDetailsResult>(assignment);
        }

        public async Task<StudentAssignmentResult> GetStudentAssignmentById(string studentAssignmentId)
        {
            var response = await GetAsync<ServiceResponseDTO<StudentAssignmentDTO>>(
                $"api/assignment/submission/{studentAssignmentId}");
            return _mapper.Map<StudentAssignmentResult>(response?.Data ?? new StudentAssignmentDTO());
        }

        public async Task<StudentAssignmentResult?> GetStudentAssignment(
            string assignmentId,
            string studentId)
        {
            try
            {
                var response = await GetAsync<ServiceResponseDTO<StudentAssignmentDTO>>(
                    $"api/assignment/student/{assignmentId}/{studentId}");

                if (response?.Success == true && response.Data != null)
                {
                    return _mapper.Map<StudentAssignmentResult>(response.Data);
                }

                return null;
            }
            catch (Exception ex) when (ex.Message.Contains("not found"))
            {
                // Not found is a normal case - student hasn't submitted yet
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error getting student assignment: {ex.Message}");
                return null;
            }
        }

        public async Task<IEnumerable<StudentAssignmentResult>> GetAssignmentSubmissions(string assignmentId)
        {
            var response = await GetAsync<ServiceResponseDTO<IEnumerable<StudentAssignmentDTO>>>(
                $"api/assignment/submissions/{assignmentId}");
            var submissions = response?.Data ?? Enumerable.Empty<StudentAssignmentDTO>();
            return _mapper.Map<IEnumerable<StudentAssignmentResult>>(submissions);
        }

        public async Task<IEnumerable<StudentAssignmentResult>> GetSubmissionsByStatus(
            string assignmentId,
            string status)
        {
            try
            {
                // Convert string status to enum
                if (!Enum.TryParse<AssignmentStatus>(status, out var statusEnum))
                {
                    return Enumerable.Empty<StudentAssignmentResult>();
                }

                var response = await GetAsync<ServiceResponseDTO<IEnumerable<StudentAssignmentDTO>>>(
                    $"api/assignment/submissions/status/{assignmentId}/{(int)statusEnum}");

                var submissions = response?.Data ?? Enumerable.Empty<StudentAssignmentDTO>();
                return _mapper.Map<IEnumerable<StudentAssignmentResult>>(submissions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error getting submissions by status: {ex.Message}");
                return Enumerable.Empty<StudentAssignmentResult>();
            }
        }

        public async Task<ReadAssignmentResult?> GetEditModel(string id)
        {
            try
            {
                var response = await GetAsync<ServiceResponseDTO<AssignmentDetailsDTO>>(
                    $"api/assignment/details/{id}");

                if (response?.Success == true && response.Data != null)
                {
                    return new ReadAssignmentResult
                    {
                        Id = response.Data.Id,
                        Title = response.Data.Title,
                        Description = response.Data.Description,
                        FilePath = response.Data.FilePath,
                        DueDate = response.Data.DueDate,
                        CourseId = response.Data.CourseId,
                        CourseName = response.Data.CourseName,
                        InstructorId = response.Data.InstructorId,
                        InstructorName = response.Data.InstructorName,
                        UploadDate = response.Data.UploadDate,
                        SubmissionsCount = response.Data.SubmissionsCount
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error retrieving edit model: {ex.Message}");
                return null;
            }
        }

        public ReadAssignmentResult GetCreateModel() => new ReadAssignmentResult();

        // ============== CRUD METHODS ==============

        public async Task<ReadAssignmentResult> CreateAssignment(ReadAssignmentResult model)
        {
            if (string.IsNullOrEmpty(model.InstructorId))
                model.InstructorId = _tokenService.GetUserId();

            var dto = _mapper.Map<CreateAssignmentDTO>(model);
            var response = await PostAsync<ServiceResponseDTO<ReadAssignmentDTO>>(
                "api/assignment/create",
                JsonContent.Create(dto)
            );

            if (response?.Data == null)
            {
                throw new Exception("Failed to create assignment");
            }

            return _mapper.Map<ReadAssignmentResult>(response.Data);
        }

        public async Task<ReadAssignmentResult> EditAssignment(ReadAssignmentResult model)
        {
            var updateDTO = _mapper.Map<UpdateAssignmentDTO>(model);
            var response = await PutAsync<ServiceResponseDTO<ReadAssignmentDTO>>(
                "api/assignment/update",
                updateDTO
            );

            if (response?.Data == null)
            {
                throw new Exception("Failed to update assignment");
            }

            return _mapper.Map<ReadAssignmentResult>(response.Data);
        }

        public async Task<bool> DeleteAssignment(string id)
        {
            try
            {
                var response = await DeleteAsync<ServiceResponseDTO<bool>>(
                    $"api/assignment/delete/{id}");
                return response?.Success == true && response.Data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error deleting assignment: {ex.Message}");
                return false;
            }
        }

        // ============== SUBMISSION & GRADING ==============

        public async Task<StudentAssignmentResult> SubmitAssignment(StudentAssignmentResult model)
        {
            var dto = _mapper.Map<SubmitAssignmentDTO>(model);
            var response = await PostAsync<ServiceResponseDTO<StudentAssignmentDTO>>(
                "api/assignment/submit",
                JsonContent.Create(dto)
            );

            if (response?.Data == null)
            {
                throw new Exception("Failed to submit assignment");
            }

            return _mapper.Map<StudentAssignmentResult>(response.Data);
        }

        public async Task<StudentAssignmentResult> GradeAssignment(StudentAssignmentResult model)
        {
            var dto = _mapper.Map<GradeAssignmentDTO>(model);
            var response = await PutAsync<ServiceResponseDTO<StudentAssignmentDTO>>(
                "api/assignment/grade",
                dto
            );

            if (response?.Data == null)
            {
                throw new Exception("Failed to grade assignment");
            }

            return _mapper.Map<StudentAssignmentResult>(response.Data);
        }

        // ============== HELPER METHODS ==============

        public async Task<T> PutAsync<T>(string url, object content)
        {
            var json = JsonConvert.SerializeObject(content);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PutAsync(url, httpContent);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"API Error: {response.StatusCode} - {errorContent}");
            }

            var responseJson = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(responseJson) || responseJson == "null")
            {
                throw new Exception($"API returned null for PUT {url}");
            }

            return JsonConvert.DeserializeObject<T>(responseJson)!;
        }

        // ============== DEPRECATED METHODS (For Compatibility) ==============

        [Obsolete("Use DownloadFileFromApi instead")]
        public async Task<FileResult> DownloadAssignmentFileAsync(string filePath, string fileName)
        {
            return await DownloadFileFromApi(filePath)
                ?? throw new Exception("Failed to download file");
        }

        [Obsolete("Use DownloadFileFromApi instead")]
        public async Task<FileResult> DownloadSubmissionFileAsync(string filePath, string fileName)
        {
            return await DownloadFileFromApi(filePath)
                ?? throw new Exception("Failed to download file");
        }
        public async Task<IEnumerable<StudentAssignmentResult>> GetStudentSubmissions(string studentId)
        {
            try
            {
                var response = await GetAsync<ServiceResponseDTO<IEnumerable<StudentAssignmentDTO>>>(
                    $"api/assignment/student/{studentId}/submissions");

                var submissions = response?.Data ?? Enumerable.Empty<StudentAssignmentDTO>();
                return _mapper.Map<IEnumerable<StudentAssignmentResult>>(submissions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error getting student submissions: {ex.Message}");
                return Enumerable.Empty<StudentAssignmentResult>();
            }
        }
        public async Task<List<StudentAssignmentItemResult>> GetStudentAllAssignments(string studentId)
        {
            try
            {
                var response = await GetAsync<ServiceResponseDTO<IEnumerable<StudentAllAssignmentsDTO>>>(
                    $"api/assignment/student/{studentId}/all-assignments");

                var assignments = response?.Data ?? Enumerable.Empty<StudentAllAssignmentsDTO>();

                return assignments.Select(a => new StudentAssignmentItemResult
                {
                    AssignmentId = a.AssignmentId,
                    AssignmentTitle = a.AssignmentTitle,
                    CourseName = a.CourseName,
                    DueDate = a.DueDate,
                    IsSubmitted = a.IsSubmitted,
                    Status = a.Status,
                    StatusDisplay = a.StatusDisplay,
                    Grade = a.Grade,
                    FilePath = a.FilePath,
                    SubmissionId = a.SubmissionId,
                    SubmittedAt = a.SubmittedAt
                }).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error getting student assignments: {ex.Message}");
                return new List<StudentAssignmentItemResult>();
            }
        }
        public async Task<ServiceResponseDTO<IEnumerable<ReadAssignmentResult>>> GetAssignmentsByInstructorAsync(string instructorId)
        {
            try
            {
                var response = await GetAsync<ServiceResponseDTO<IEnumerable<ReadAssignmentDTO>>>(
                    $"api/assignment/instructor/{instructorId}");

                if (response?.Success == true && response.Data != null)
                {
                    var mappedData = _mapper.Map<IEnumerable<ReadAssignmentResult>>(response.Data);
                    return new ServiceResponseDTO<IEnumerable<ReadAssignmentResult>>
                    {
                        Success = true,
                        Data = mappedData,
                        Message = response.Message
                    };
                }

                return new ServiceResponseDTO<IEnumerable<ReadAssignmentResult>>
                {
                    Success = false,
                    Message = response?.Message ?? "Failed to retrieve assignments."
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error getting instructor assignments: {ex.Message}");
                return new ServiceResponseDTO<IEnumerable<ReadAssignmentResult>>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }
    }
}