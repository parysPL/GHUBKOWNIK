using GHUBKOWNIK.Views;

namespace GHUBKOWNIK.Views
{
    public partial class MainMenuPage : ContentPage
    {
        public MainMenuPage()
        {
            InitializeComponent();
        }

        private async void OnQuizGeneratorClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(QuizGeneratorPage));
        }

        private async void OnStartQuizClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(QuizPage));
        }
    }
}