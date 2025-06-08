namespace GHUBKOWNIK
{
    public partial class MainPage : ContentPage
    {

        public MainPage()
        {
            InitializeComponent();
        }

        private async void QuizNavigationButton_Clicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("///TestPage");
        }

        private async void GeneratorNavigationButton_Clicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("///GeneratorPage");
        }

        private async void SettingsNavigationButton_Clicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("///SettingsPage");
        }
    }
}
