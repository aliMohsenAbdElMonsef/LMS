using Domain.Entities.MainEntities;
using LMS.BusinessLogic.DTOs.CertificateTemplate;
using LMS.BusinessLogic.Services;
using LMS.DataAccess.Contracts;
using LMS.DataAccess.Contracts.Repositories;
using Moq;
using Xunit;

namespace LMS.Tests.Services
{
    public class CertificateTemplateServicesTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ICertificateTemplateRepository> _certificateTemplateRepoMock;
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<ICourseRepository> _courseRepoMock;
        private readonly CertificateTemplateServices _certificateTemplateService;

        public CertificateTemplateServicesTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _certificateTemplateRepoMock = new Mock<ICertificateTemplateRepository>();
            _userRepoMock = new Mock<IUserRepository>();
            _courseRepoMock = new Mock<ICourseRepository>();

            _unitOfWorkMock.Setup(u => u.CertificateTemplates).Returns(_certificateTemplateRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.Courses).Returns(_courseRepoMock.Object);

            _certificateTemplateService = new CertificateTemplateServices(_unitOfWorkMock.Object);
        }

        [Fact]
        public async Task CreateAsync_ShouldCreateCertificateTemplate_WhenDataIsValid()
        {

            var dto = new CreateCertificateTemplateDTO
            {
                Title = "Course Completion Certificate",
                Description = "Certificate for completing the course",
                AdminId = "admin1",
                CourseId = "course1"
            };

            CertificateTemplate capturedTemplate = null;
            _certificateTemplateRepoMock.Setup(r => r.CreateAsync(It.IsAny<CertificateTemplate>()))
                .Callback<CertificateTemplate>(t => {
                    capturedTemplate = t;

                    t.Admin = new ApplicationUser { FirstName = "Test", LastName = "Admin" };
                    t.Course = new Course { Name = "Test Course" };
                })
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);


            var result = await _certificateTemplateService.CreateAsync(dto);


            Assert.True(result.Success);
            Assert.Equal("Entity created successfully.", result.Message);
            Assert.NotNull(capturedTemplate);
            Assert.Equal(dto.Title, capturedTemplate.Title);
            Assert.Equal(dto.Description, capturedTemplate.Description);
            Assert.Equal(dto.AdminId, capturedTemplate.AdminId);
            Assert.Equal(dto.CourseId, capturedTemplate.CourseId);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnCertificateTemplate_WhenExists()
        {

            var templateId = "template1";
            var template = new CertificateTemplate
            {
                Id = templateId,
                Title = "Test Certificate",
                Description = "Test Description",
                MessageBody = "Congratulations!",
                AdminId = "admin1",
                CourseId = "course1",
                Admin = new ApplicationUser { FirstName = "John", LastName = "Doe" },
                Course = new Course { Name = "Test Course" },
                CreatedAt = DateTime.UtcNow
            };

            _certificateTemplateRepoMock.Setup(r => r.FindByIdAsync(templateId)).ReturnsAsync(template);


            var result = await _certificateTemplateService.GetByIdAsync(templateId);


            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(template.Id, result.Data.Id);
            Assert.Equal(template.Title, result.Data.Title);
            Assert.Equal("John Doe", result.Data.AdminName);
            Assert.Equal("Test Course", result.Data.CourseName);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnFailure_WhenNotFound()
        {

            var templateId = "nonexistent";
            _certificateTemplateRepoMock.Setup(r => r.FindByIdAsync(templateId)).ReturnsAsync((CertificateTemplate)null);


            var result = await _certificateTemplateService.GetByIdAsync(templateId);


            Assert.False(result.Success);
            Assert.Equal("Entity Not Found.", result.Message);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateCertificateTemplate_WhenExists()
        {

            var updateDto = new UpdateCertificateTemplateDTO
            {
                Id = "template1",
                Title = "Updated Title",
                Description = "Updated Description"
            };

            var existingTemplate = new CertificateTemplate
            {
                Id = "template1",
                Title = "Old Title",
                Description = "Old Description",
                AdminId = "admin1",
                CourseId = "course1",
                Admin = new ApplicationUser { FirstName = "Test", LastName = "Admin" },
                Course = new Course { Name = "Test Course" }
            };

            _certificateTemplateRepoMock.Setup(r => r.FindByIdAsync(updateDto.Id)).ReturnsAsync(existingTemplate);
            _certificateTemplateRepoMock.Setup(r => r.UpdateAsync(It.IsAny<CertificateTemplate>())).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);


            var result = await _certificateTemplateService.UpdateAsync(updateDto);


            Assert.True(result.Success);
            Assert.Equal("Entity updated successfully.", result.Message);
            Assert.Equal(updateDto.Title, existingTemplate.Title);
            Assert.Equal(updateDto.Description, existingTemplate.Description);
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteCertificateTemplate_WhenExists()
        {

            var templateId = "template1";
            var template = new CertificateTemplate
            {
                Id = templateId,
                Title = "Test Certificate",
                AdminId = "admin1",
                CourseId = "course1"
            };

            _certificateTemplateRepoMock.Setup(r => r.FindByIdAsync(templateId)).ReturnsAsync(template);
            _certificateTemplateRepoMock.Setup(r => r.DeleteWithIDAsync(templateId)).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);


            var result = await _certificateTemplateService.DeleteAsync(templateId);


            Assert.True(result.Success);
            Assert.Equal("Entity Deleted Succesfully.", result.Message);
            _certificateTemplateRepoMock.Verify(r => r.DeleteWithIDAsync(templateId), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnFailure_WhenNotFound()
        {

            var templateId = "nonexistent";
            _certificateTemplateRepoMock.Setup(r => r.FindByIdAsync(templateId)).ReturnsAsync((CertificateTemplate)null);


            var result = await _certificateTemplateService.DeleteAsync(templateId);


            Assert.False(result.Success);
            Assert.Equal("Entity Not Found.", result.Message);
        }
    }
}
