using CommunityToolkit.Maui.Storage;
using GHUBKOWNIK.Models;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace GHUBKOWNIK.Views;

public partial class QuizPage : ContentPage
{
    public bool IsSingleChoice { get; private set; }
    public bool IsMultipleChoice { get; private set; }

    private List<Question> _allQuestions = new();
    private List<Question> _remainingQuestions = new();
    private List<Question> _incorrectQuestions = new();
    private QuizSettings _settings = new() { InitialRepeatCount = 3, IncorrectAnswerRepeatIncrement = 1 };
    private QuizResult _result = new();
    private Dictionary<string, int> _questionRepeatCounts = new();
    private ObservableCollection<string> _repeatCountsDisplay = new();
    private ObservableCollection<AnswerOption> _currentAnswerOptions = new();
    private Question _currentQuestion;

    public QuizPage()
    {
        InitializeComponent();
        RepeatCountsCollectionView.ItemsSource = _repeatCountsDisplay;
        AnswerOptionsCollectionView.ItemsSource = _currentAnswerOptions;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadQuestions();
        PrepareQuestionQueue();
        ShowNextQuestion();
    }

    private async Task LoadQuestions()
    {
        try
        {
            var folderResult = await FolderPicker.Default.PickAsync();
            if (folderResult != null)
            {
                _settings.QuestionsFolderPath = folderResult.Folder.Path;
                var files = Directory.GetFiles(_settings.QuestionsFolderPath, "*.json");

                foreach (var file in files)
                {
                    var json = await File.ReadAllTextAsync(file);
                    var question = JsonSerializer.Deserialize<Question>(json);
                    _allQuestions.Add(question);
                    _questionRepeatCounts[question.Text] = _settings.InitialRepeatCount;
                }

                _result.TotalQuestions = _allQuestions.Count;
                UpdateRepeatCountsDisplay();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private void PrepareQuestionQueue()
    {
        _remainingQuestions.Clear();
        foreach (var question in _allQuestions)
        {
            for (int i = 0; i < _questionRepeatCounts[question.Text]; i++)
            {
                _remainingQuestions.Add(question);
            }
        }
        _remainingQuestions = _remainingQuestions.OrderBy(x => Guid.NewGuid()).ToList();
    }

    private void ShowNextQuestion()
    {
        if (_remainingQuestions.Count == 0 && _incorrectQuestions.Count == 0)
        {
            FinishQuiz();
            return;
        }

        // First try to show incorrectly answered questions
        if (_incorrectQuestions.Count > 0)
        {
            ShowQuestion(_incorrectQuestions[0]);
            _currentQuestion = _incorrectQuestions[0];
            _incorrectQuestions.RemoveAt(0);
        }
        else
        {
            ShowQuestion(_remainingQuestions[0]);
            _currentQuestion = _remainingQuestions[0];
            _remainingQuestions.RemoveAt(0);
        }
    }

    private void ShowQuestion(Question question)
    {
        QuestionTextLabel.Text = question.Text;

        // Clear previous images and answers
        ImagesStackLayout.Children.Clear();
        _currentAnswerOptions.Clear();

        // Add new images
        foreach (var imagePath in question.ImagePaths)
        {
            ImagesStackLayout.Children.Add(new Image
            {
                Source = imagePath,
                HeightRequest = 120,
                WidthRequest = 120
            });
        }

        // Set question type flags
        IsSingleChoice = question.Type == QuestionType.SingleChoice;
        IsMultipleChoice = question.Type == QuestionType.MultipleChoice;
        OnPropertyChanged(nameof(IsSingleChoice));
        OnPropertyChanged(nameof(IsMultipleChoice));

        // Configure answer options
        AnswerOptionsStackLayout.IsVisible = question.Type != QuestionType.TextInput;
        TextAnswerStackLayout.IsVisible = question.Type == QuestionType.TextInput;
        SubmitAnswerButton.IsVisible = question.Type != QuestionType.TextInput;

        if (question.Type != QuestionType.TextInput)
        {
            foreach (var option in question.AnswerOptions)
            {
                _currentAnswerOptions.Add(new AnswerOption
                {
                    Text = option.Text,
                    ImagePath = option.ImagePath,
                    IsCorrect = option.IsCorrect,
                    IsSelected = false
                });
            }
        }

        NextButton.IsVisible = false;
        FeedbackLabel.Text = string.Empty;
        TextAnswerEntry.Text = string.Empty;

        // Update progress
        QuizProgressBar.Progress = 1 - (double)(_remainingQuestions.Count + _incorrectQuestions.Count) /
            (_allQuestions.Count * _settings.InitialRepeatCount);
        ProgressLabel.Text = $"Questions remaining: {_remainingQuestions.Count + _incorrectQuestions.Count}";
    }

    private async void OnSubmitAnswerClicked(object sender, EventArgs e)
    {
        // First check if we have any questions left
        if (_remainingQuestions.Count == 0 && _incorrectQuestions.Count == 0)
        {
            await DisplayAlert("Error", "No questions available", "OK");
            return;
        }

        // Safely get current question
        Question currentQuestion = GetCurrentQuestion();
        if (currentQuestion == null)
        {
            await DisplayAlert("Error", "Could not determine current question", "OK");
            return;
        }

        bool isCorrect = false;

        if (IsSingleChoice)
        {
            var selectedOption = _currentAnswerOptions.FirstOrDefault(o => o.IsSelected);
            if (selectedOption == null)
            {
                await DisplayAlert("Error", "Please select an answer", "OK");
                return;
            }
            isCorrect = selectedOption.IsCorrect;
        }
        else if (IsMultipleChoice)
        {
            if (!_currentAnswerOptions.Any(o => o.IsSelected))
            {
                await DisplayAlert("Error", "Please select at least one answer", "OK");
                return;
            }
            isCorrect = _currentAnswerOptions.All(o => o.IsSelected == o.IsCorrect);
        }

        await ProcessAnswer(isCorrect, currentQuestion);
    }

    private Question GetCurrentQuestion()
    {
        try
        {
            if (_remainingQuestions.Count > 0)
            {
                return _remainingQuestions[0];
            }
            else if (_incorrectQuestions.Count > 0)
            {
                return _incorrectQuestions[0];
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    private async void OnTextAnswerSubmitted(object sender, EventArgs e)
    {
        // First check if we have any questions left
        if (_remainingQuestions.Count == 0 && _incorrectQuestions.Count == 0)
        {
            await DisplayAlert("Error", "No questions available", "OK");
            return;
        }

        // Safely get current question
        Question currentQuestion = GetCurrentQuestion();
        if (currentQuestion == null)
        {
            await DisplayAlert("Error", "Could not determine current question", "OK");
            return;
        }

        var userAnswer = TextAnswerEntry.Text;
        var isCorrect = string.Equals(userAnswer.Trim(), currentQuestion.CorrectTextAnswer?.Trim(),
            StringComparison.OrdinalIgnoreCase);

        await ProcessAnswer(isCorrect, currentQuestion);
    }

    private async Task ProcessAnswer(bool isCorrect, Question question)
    {
        // Track if this was the first attempt at this question
        bool isFirstAttempt = _questionRepeatCounts[question.Text] == _settings.InitialRepeatCount;

        if (isCorrect)
        {
            FeedbackLabel.Text = "Correct!";
            FeedbackLabel.TextColor = Colors.Green;
            _result.CorrectAnswers++;

            // Track first-time correct answers
            if (isFirstAttempt)
            {
                _result.FirstTimeCorrect++;
            }

            // Decrease repeat count for correct answers
            _questionRepeatCounts[question.Text]--;
            if (_questionRepeatCounts[question.Text] <= 0)
            {
                _questionRepeatCounts.Remove(question.Text);
            }
        }
        else
        {
            FeedbackLabel.Text = "Incorrect!";
            FeedbackLabel.TextColor = Colors.Red;
            _result.IncorrectAnswers++;

            // Increase repeat count for incorrect answers
            _questionRepeatCounts[question.Text] += _settings.IncorrectAnswerRepeatIncrement;

            // Add to incorrect questions queue to repeat later
            _incorrectQuestions.Add(question);
        }

        // Update UI
        UpdateRepeatCountsDisplay();
        NextButton.IsVisible = true;
        SubmitAnswerButton.IsVisible = false;

        // If this was the last question, set end time
        if (_remainingQuestions.Count == 0 && _incorrectQuestions.Count == 0)
        {
            _result.EndTime = DateTime.Now;
        }
    }

    private void UpdateRepeatCountsDisplay()
    {
        _repeatCountsDisplay.Clear();
        foreach (var kvp in _questionRepeatCounts)
        {
            _repeatCountsDisplay.Add($"{kvp.Key}: {kvp.Value} repeats");
        }
    }

    private void OnNextQuestionClicked(object sender, EventArgs e)
    {
        ShowNextQuestion();
    }

    private async void FinishQuiz()
{
    var result = new QuizResult
    {
        TotalQuestions = _allQuestions.Count,
        CorrectAnswers = _result.CorrectAnswers,
        IncorrectAnswers = _result.IncorrectAnswers,
        QuestionRepeatCounts = _questionRepeatCounts
    };

    // Serialize the result and pass as query parameter
    var resultJson = Newtonsoft.Json.JsonConvert.SerializeObject(result);
    await Shell.Current.GoToAsync($"{nameof(ResultsPage)}?QuizResult={Uri.EscapeDataString(resultJson)}");
}

    private async void OnBackToMenuClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}