using System.Collections.Generic;

namespace GHUBKOWNIK.Models
{
    public enum QuestionType
    {
        SingleChoice,
        MultipleChoice,
        TextInput
    }

    public class AnswerOption
    {
        public string Text { get; set; }
        public string ImagePath { get; set; }
        public bool IsCorrect { get; set; }
        public bool IsSelected { get; set; } // Add this
    }

    public class Question
    {
        public string Text { get; set; }
        public List<string> ImagePaths { get; set; } = new List<string>();
        public QuestionType Type { get; set; }
        public List<AnswerOption> AnswerOptions { get; set; } = new List<AnswerOption>();
        public string CorrectTextAnswer { get; set; } // For text input questions
        public int RepeatCount { get; set; } = 1;
    }
}