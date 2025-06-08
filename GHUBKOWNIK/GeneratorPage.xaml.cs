namespace GHUBKOWNIK
{
    public partial class GeneratorPage : ContentPage
    {
        public GeneratorPage()
        {
            InitializeComponent();
        }

        private async void BackButton_Clicked(object sender, EventArgs e)
        {

            await Navigation.PushAsync(new MainPage());
        
        }
    }
}