using GHUBKOWNIK.Models;
using System.Collections.Generic;

namespace GHUBKOWNIK.Views;

public partial class ResultsPage : ContentPage
{
    public QuizResult QuizResult { get; set; }

    public ResultsPage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        // Handle Shell navigation parameters
        if (Shell.Current?.CurrentState?.Location is not null)
        {
            var route = Shell.Current.CurrentState.Location.ToString();
            if (route.Contains("QuizResult="))
            {
                try
                {
                    var param = route.Split('=')[1];
                    if (!string.IsNullOrEmpty(param))
                    {
                        QuizResult = Newtonsoft.Json.JsonConvert.DeserializeObject<QuizResult>(param);
                        OnPropertyChanged(nameof(QuizResult));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing QuizResult: {ex}");
                }
            }
        }
    }

    private async void OnBackToMainMenuClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}