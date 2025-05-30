using System;
using System.Collections.Generic;

namespace GHUBKOWNIK.Models;

public class QuizResult
{
    public DateTime StartTime { get; set; } = DateTime.Now;
    public DateTime EndTime { get; set; }
    public TimeSpan Duration => EndTime - StartTime;

    public int TotalQuestions { get; set; }
    public int CorrectAnswers { get; set; }
    public int IncorrectAnswers { get; set; }
    public int FirstTimeCorrect { get; set; }
    public Dictionary<string, int> QuestionRepeatCounts { get; set; } = new();

    public double ScorePercentage => TotalQuestions > 0
        ? Math.Round((double)CorrectAnswers / TotalQuestions * 100, 2)
        : 0;

    public double FirstTimeAccuracy => TotalQuestions > 0
        ? Math.Round((double)FirstTimeCorrect / TotalQuestions * 100, 2)
        : 0;
}