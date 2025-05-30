namespace GHUBKOWNIK.Models
{
    public class QuizSettings
    {
        public string QuestionsFolderPath { get; set; }
        public int InitialRepeatCount { get; set; } = 3;
        public int IncorrectAnswerRepeatIncrement { get; set; } = 1;
    }
}