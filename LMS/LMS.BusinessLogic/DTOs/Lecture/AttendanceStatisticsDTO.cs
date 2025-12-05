using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.DTOs.Lecture
{
    public class AttendanceStatisticsDTO
    {
        public int TotalLectures { get; set; }
        public int AttendedLectures { get; set; }
        public double AttendancePercentage { get; set; }
    }
}
