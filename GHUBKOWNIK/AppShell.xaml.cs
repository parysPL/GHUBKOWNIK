using GHUBKOWNIK.Views;

namespace GHUBKOWNIK;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(QuizGeneratorPage), typeof(QuizGeneratorPage));
        Routing.RegisterRoute(nameof(QuizPage), typeof(QuizPage));
        Routing.RegisterRoute(nameof(ResultsPage), typeof(ResultsPage));
    }
}