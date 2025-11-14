using AutoMapper;
using LMS.BusinessLogic.DTOs.Assignment;
using LMS.BusinessLogic.DTOs.Responses;
using LMS.Entity.Enums;
using LMS.MVC.Models.ViewModels.Assignment;
using LMS.MVC.Services.Contracts.Services;
using Newtonsoft.Json;
using System.Text;

namespace LMS.MVC.Services.Services
{
    internal class AssignmentService : BaseMVCServices, IAssignmentService
    {
        private readonly IMapper _mapper;
        protected readonly HttpClient _client;
        protected readonly IHttpContextAccessor _contextAccessor;
        private string _baseApiUrl = "api/enrollment";
        private readonly ITokenService _tokenService;

        public AssignmentService(HttpClient client, IHttpContextAccessor accessor, ITokenService tokenService, IMapper mapper)
            : base(client, accessor)
        {
            _mapper = mapper;
            _tokenService = tokenService;
            _client = client;
            _contextAccessor = accessor;

        }

        public async Task<IEnumerable<ReadAssignmentResult>> GetAssignmentsByCourse(string courseId)
        {
            var serviceResponse = await GetAsync<ServiceResponseDTO<IEnumerable<ReadAssignmentDTO>>>($"api/assignment/course/{courseId}");
            var assignments = serviceResponse?.Data ?? Enumerable.Empty<ReadAssignmentDTO>();
            return _mapper.Map<IEnumerable<ReadAssignmentResult>>(assignments);
        }
        public async Task<StudentAssignmentResult> GetStudentAssignmentById(string studentAssignmentId)
        {
            var response = await GetAsync<ServiceResponseDTO<StudentAssignmentDTO>>($"api/assignment/submission/{studentAssignmentId}");
            return _mapper.Map<StudentAssignmentResult>(response?.Data ?? new StudentAssignmentDTO());
        }

        public async Task<AssignmentDetailsResult> GetAssignmentById(string id)
        {
            var response = await GetAsync<ServiceResponseDTO<AssignmentDetailsDTO>>($"api/assignment/details/{id}");
            var assignment = response?.Data ?? new AssignmentDetailsDTO();
            return _mapper.Map<AssignmentDetailsResult>(assignment);
        }

        public async Task<StudentAssignmentResult> GetStudentAssignment(string assignmentId, string studentId)
        {
            try
            {
                var response = await GetAsync<ServiceResponseDTO<StudentAssignmentDTO>>($"api/assignment/student/{assignmentId}/{studentId}");

                // Check if the API call was successful and has data
                if (response?.Success == true && response.Data != null)
                {
                    return _mapper.Map<StudentAssignmentResult>(response.Data);
                }

                // If API returned success but no data, or submission not found
                return null;
            }
            catch (Exception ex) when (ex.Message.Contains("not found") || ex.Message.Contains("Submission not found"))
            {
                // Handle "not found" as a normal case
                return null;
            }
            catch (Exception ex)
            {
                // Log other errors but still return null to allow submission
                Console.WriteLine($"Error getting student assignment: {ex.Message}");
                return null;
            }
        }
        public async Task<IEnumerable<StudentAssignmentResult>> GetAssignmentSubmissions(string assignmentId)
        {
            var response = await GetAsync<ServiceResponseDTO<IEnumerable<StudentAssignmentDTO>>>($"api/assignment/submissions/{assignmentId}");
            var submissions = response?.Data ?? Enumerable.Empty<StudentAssignmentDTO>();
            return _mapper.Map<IEnumerable<StudentAssignmentResult>>(submissions);
        }

        public async Task<ReadAssignmentResult> GetEditModel(string id)
        {
            try
            {
                // Use the correct API endpoint that exists
                var response = await GetAsync<ServiceResponseDTO<AssignmentDetailsDTO>>($"api/assignment/details/{id}");

                if (response?.Success == true && response.Data != null)
                {
                    // Map from AssignmentDetailsDTO to ReadAssignmentResult
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
                Console.WriteLine("Error retrieving edit model: " + ex.Message);
                return null;
            }
        }
        public ReadAssignmentResult GetCreateModel() => new();

        public async Task<ReadAssignmentResult> CreateAssignment(ReadAssignmentResult model)
        {
            if (string.IsNullOrEmpty(model.InstructorId))
                model.InstructorId = _tokenService.GetUserId();

            var dto = _mapper.Map<CreateAssignmentDTO>(model);
            var response = await PostAsync<ServiceResponseDTO<ReadAssignmentDTO>>(
                "api/assignment/create",
                JsonContent.Create(dto)
            );
            return _mapper.Map<ReadAssignmentResult>(response.Data);
        }

        public async Task<ReadAssignmentResult> EditAssignment(ReadAssignmentResult model)
        {
            var updateDTO = _mapper.Map<UpdateAssignmentDTO>(model);
            var response = await PutAsync<ServiceResponseDTO<ReadAssignmentDTO>>(
                "api/assignment/update",
                updateDTO
            );
            return _mapper.Map<ReadAssignmentResult>(response.Data);
        }
        //public async Task<bool> DeleteAssignment(string id)
        //{
        //    try
        //    {
        //        var response = await DeleteAsync<ServiceResponseDTO<bool>>($"api/assignment/{id}");
        //        return response?.Success == true;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("Error deleting assignment in service: " + ex.Message);
        //        return false;
        //    }
        //}

        public async Task<StudentAssignmentResult> SubmitAssignment(StudentAssignmentResult model)
        {
            var dto = _mapper.Map<SubmitAssignmentDTO>(model);
            var response = await PostAsync<ServiceResponseDTO<StudentAssignmentDTO>>(
                "api/assignment/submit",
                JsonContent.Create(dto)
            );
            return _mapper.Map<StudentAssignmentResult>(response.Data);
        }

        public async Task<StudentAssignmentResult> GradeAssignment(StudentAssignmentResult model)
        {
            var dto = _mapper.Map<GradeAssignmentDTO>(model);
            var response = await PutAsync<ServiceResponseDTO<StudentAssignmentDTO>>(
                "api/assignment/grade",
                dto
            );
            return _mapper.Map<StudentAssignmentResult>(response.Data);
        }

        public async Task<T> PutAsync<T>(string url, object content)
        {
            var json = JsonConvert.SerializeObject(content);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PutAsync(url, httpContent);
            response.EnsureSuccessStatusCode();
            var responseJson = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(responseJson) || responseJson == "null")
                throw new Exception($"API returned null for PUT {url}");
            return JsonConvert.DeserializeObject<T>(responseJson)!;
        }
        // NEW: Implement GetSubmissionsByStatus
        public async Task<IEnumerable<StudentAssignmentResult>> GetSubmissionsByStatus(string assignmentId, string status)
        {
            try
            {
                // Convert string status to enum
                if (!Enum.TryParse<AssignmentStatus>(status, out var statusEnum))
                {
                    // If parsing fails, return empty list
                    return Enumerable.Empty<StudentAssignmentResult>();
                }

                var response = await GetAsync<ServiceResponseDTO<IEnumerable<StudentAssignmentDTO>>>(
                    $"api/assignment/submissions/status/{assignmentId}/{(int)statusEnum}");

                var submissions = response?.Data ?? Enumerable.Empty<StudentAssignmentDTO>();
                return _mapper.Map<IEnumerable<StudentAssignmentResult>>(submissions);
            }
            catch (Exception ex)
            {
                // Log error and return empty list
                Console.WriteLine($"Error getting submissions by status: {ex.Message}");
                return Enumerable.Empty<StudentAssignmentResult>();
            }
        }

    }
}