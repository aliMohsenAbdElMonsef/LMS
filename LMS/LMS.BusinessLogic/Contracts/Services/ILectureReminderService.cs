using System.Threading.Tasks;

namespace LMS.BusinessLogic.Contracts.Services
{
    public interface ILectureReminderService
    {
        Task SendLectureRemindersAsync();
    }
}
