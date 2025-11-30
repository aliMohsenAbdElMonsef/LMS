using Domain.Enums;

namespace LMS.BusinessLogic.DTOs.Quiz
{
    public class SubmitQuizAnswerDTO
    {
        public string QuestionId { get; set; }
        // Nullable to allow unanswered questions (null instead of defaulting to OptionA)
        public Options? SelectedAnswer { get; set; }
        public string? ShortAnswerText { get; set; }
    }
}
