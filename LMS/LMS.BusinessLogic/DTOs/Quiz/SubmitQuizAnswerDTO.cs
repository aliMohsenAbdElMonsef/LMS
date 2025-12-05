using Domain.Enums;

namespace LMS.BusinessLogic.DTOs.Quiz
{
    public class SubmitQuizAnswerDTO
    {
        public string QuestionId { get; set; }

        public Options? SelectedAnswer { get; set; }
        public string? ShortAnswerText { get; set; }
    }
}
