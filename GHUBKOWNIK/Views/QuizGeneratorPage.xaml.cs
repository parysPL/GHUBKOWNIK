using CommunityToolkit.Maui.Storage;
using GHUBKOWNIK.Models;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace GHUBKOWNIK.Views;

public partial class QuizGeneratorPage : ContentPage
{
    public bool IsSingleChoice => QuestionTypePicker.SelectedIndex == 0;
    public bool IsMultipleChoice => QuestionTypePicker.SelectedIndex == 1;

    private ObservableCollection<string> _imagePaths = new();
    private ObservableCollection<AnswerOption> _answerOptions = new();
    private string _selectedFolderPath;

    public QuizGeneratorPage()
    {
        InitializeComponent();
        ImagesCollectionView.ItemsSource = _imagePaths;
        AnswerOptionsCollectionView.ItemsSource = _answerOptions;

        QuestionTypePicker.SelectedIndexChanged += (s, e) =>
        {
            AnswerOptionsContainer.IsVisible = QuestionTypePicker.SelectedIndex <= 1;
            TextAnswerContainer.IsVisible = QuestionTypePicker.SelectedIndex == 2;
            OnPropertyChanged(nameof(IsSingleChoice));
            OnPropertyChanged(nameof(IsMultipleChoice));
        };
    }

    private async void OnAddImageClicked(object sender, EventArgs e)
    {
        try
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Select an image",
                FileTypes = FilePickerFileType.Images
            });

            if (result != null)
            {
                _imagePaths.Add(result.FullPath);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private void OnAddAnswerOptionClicked(object sender, EventArgs e)
    {
        _answerOptions.Add(new AnswerOption());
    }

    private async void OnSelectAnswerImageClicked(object sender, EventArgs e)
    {
        var button = (Button)sender;
        var answerOption = (AnswerOption)button.CommandParameter;

        try
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Select answer image",
                FileTypes = FilePickerFileType.Images
            });

            if (result != null)
            {
                answerOption.ImagePath = result.FullPath;
                // Refresh the collection view to update the image display
                var tempList = new List<AnswerOption>(_answerOptions);
                _answerOptions.Clear();
                foreach (var item in tempList) _answerOptions.Add(item);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private void OnRemoveAnswerOptionClicked(object sender, EventArgs e)
    {
        var button = (Button)sender;
        var answerOption = (AnswerOption)button.CommandParameter;
        _answerOptions.Remove(answerOption);
    }

    private async void OnSelectFolderClicked(object sender, EventArgs e)
    {
        try
        {
            var result = await FolderPicker.Default.PickAsync();
            if (result != null)
            {
                _selectedFolderPath = result.Folder.Path;
                SelectedFolderLabel.Text = _selectedFolderPath;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnGenerateQuestionClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(QuestionTextEditor.Text))
        {
            await DisplayAlert("Error", "Please enter question text", "OK");
            return;
        }

        if (string.IsNullOrEmpty(_selectedFolderPath))
        {
            await DisplayAlert("Error", "Please select a folder to save the question", "OK");
            return;
        }

        var question = new Question
        {
            Text = QuestionTextEditor.Text,
            ImagePaths = _imagePaths.ToList(),
            Type = QuestionTypePicker.SelectedIndex switch
            {
                0 => QuestionType.SingleChoice,
                1 => QuestionType.MultipleChoice,
                2 => QuestionType.TextInput,
                _ => QuestionType.SingleChoice
            },
            AnswerOptions = _answerOptions.ToList(),
            CorrectTextAnswer = CorrectTextAnswerEntry.Text
        };

        // Validate answer options
        if (question.Type != QuestionType.TextInput)
        {
            if (!question.AnswerOptions.Any(o => o.IsCorrect))
            {
                await DisplayAlert("Error", "Please mark at least one correct answer", "OK");
                return;
            }
        }

        // Save question to JSON file
        var json = JsonSerializer.Serialize(question);
        var fileName = $"question_{DateTime.Now:yyyyMMddHHmmss}.json";
        var filePath = Path.Combine(_selectedFolderPath, fileName);

        try
        {
            await File.WriteAllTextAsync(filePath, json);
            await DisplayAlert("Success", "Question generated successfully", "OK");

            // Reset form
            QuestionTextEditor.Text = string.Empty;
            _imagePaths.Clear();
            _answerOptions.Clear();
            CorrectTextAnswerEntry.Text = string.Empty;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to save question: {ex.Message}", "OK");
        }
    }

    private async void OnBackToMenuClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}