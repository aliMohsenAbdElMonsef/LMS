using AutoMapper;
using Domain.Entities.MainEntities;
using Domain.Enums;
using LMS.BusinessLogic.DTOs.Course;
using LMS.DataAcess.Contracts;
using LMS.Entity.Entities.MainEntities;
using Microsoft.AspNetCore.Hosting;

namespace LMS.BusinessLogic.Services.Helpers
{
    public static class CourseStatusHelper
    {
        //private readonly IUnitOfWork _unitOfWork;
        //private readonly IWebHostEnvironment _webHostEnvironment;
        //private readonly IMapper _mapper;

        public static Status DetermineCourseStatus(DateTime startDate, DateTime endDate, DateTime currentDate)
        {
            if (currentDate < startDate)
                return Status.Draft;

            if (currentDate >= startDate && currentDate <= endDate)
                return Status.Ongoing;

            if (currentDate > endDate)
                return Status.Completed;

            return Status.Draft;
        }

        public static Status DetermineCourseStatus(DateTime startDate, DateTime endDate)
        {
            return DetermineCourseStatus(startDate, endDate, DateTime.UtcNow);
        }
        public static string GetStatusDescription(Status status)
        {
            return status switch
            {
                Status.Draft => "Draft - Not yet published",
                Status.Published => "Published - Ready to start",
                Status.Ongoing => "Ongoing - Currently running",
                Status.Completed => "Completed - Finished",
                Status.Archived => "Archived - Stored",
                _ => "Unknown"
            };
        }


        public static string GetDeliveryModeDescription(DeliveryMode mode)
        {
            return mode switch
            {
                DeliveryMode.Online => "Online",
                DeliveryMode.Onsite => "OnSite",
                DeliveryMode.Hybrid => "Hybrid(Online+Onsite)"
            };
        }
    }
}

    
