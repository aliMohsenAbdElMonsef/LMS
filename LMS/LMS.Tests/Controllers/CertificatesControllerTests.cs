using LMS.API.Controllers;
using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.DTOs.Certificate;
using LMS.BusinessLogic.DTOs.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;

namespace LMS.Tests.Controllers
{
    public class CertificatesControllerTests
    {
        private readonly Mock<ICertificateGenerationService> _mockCertificateService;
        private readonly CertificatesController _controller;

        public CertificatesControllerTests()
        {
            _mockCertificateService = new Mock<ICertificateGenerationService>();
            _controller = new CertificatesController(_mockCertificateService.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "student-id"),
            }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }

        [Fact]
        public async Task GenerateCertificate_ReturnsOk_WhenSuccessful()
        {

            var dto = new GenerateCertificateDTO { CourseId = "course1" };
            var responseDto = new ServiceResponseDTO<ReadStudentCertificateDTO> { Success = true };

            _mockCertificateService.Setup(s => s.GenerateCertificateAsync(dto))
                .ReturnsAsync(responseDto);


            var result = await _controller.GenerateCertificate(dto);


            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.True(((ServiceResponseDTO<ReadStudentCertificateDTO>)okResult.Value).Success);
        }

        [Fact]
        public async Task GetMyCertificates_ReturnsOk()
        {

            var responseDto = new ServiceResponseDTO<IEnumerable<ReadStudentCertificateDTO>> { Success = true };

            _mockCertificateService.Setup(s => s.GetStudentCertificatesAsync("student-id"))
                .ReturnsAsync(responseDto);


            var result = await _controller.GetMyCertificates();


            var okResult = Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetStudentCertificates_ReturnsOk()
        {

            var studentId = "student-1";
            var responseDto = new ServiceResponseDTO<IEnumerable<ReadStudentCertificateDTO>> { Success = true };

            _mockCertificateService.Setup(s => s.GetStudentCertificatesAsync(studentId))
                .ReturnsAsync(responseDto);


            var result = await _controller.GetStudentCertificates(studentId);


            var okResult = Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetCertificateById_ReturnsOk_WhenFound()
        {

            var id = "cert-1";
            var responseDto = new ServiceResponseDTO<ReadStudentCertificateDTO> { Success = true };

            _mockCertificateService.Setup(s => s.GetCertificateByIdAsync(id))
                .ReturnsAsync(responseDto);


            var result = await _controller.GetCertificateById(id);


            var okResult = Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task DownloadCertificate_ReturnsFile_WhenSuccessful()
        {

            var id = "cert-1";
            var fileBytes = new byte[] { 1, 2, 3 };
            var responseDto = new ServiceResponseDTO<byte[]> { Success = true, Data = fileBytes };

            _mockCertificateService.Setup(s => s.DownloadCertificateAsync(id))
                .ReturnsAsync(responseDto);


            var result = await _controller.DownloadCertificate(id);


            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/pdf", fileResult.ContentType);
        }
    }
}
