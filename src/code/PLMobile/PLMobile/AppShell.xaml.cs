namespace PLMobile
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            RegisterRoutes();
        }

        private void RegisterRoutes()
        {
            Routing.RegisterRoute(nameof(LibraryPage), typeof(LibraryPage));
            Routing.RegisterRoute(nameof(ImportPage), typeof(ImportPage));
            Routing.RegisterRoute(nameof(TagsPage), typeof(TagsPage));
            Routing.RegisterRoute(nameof(ReadPage), typeof(ReadPage));
        }

        // Static method for back navigation (can be called from any pages)
        public static async Task GoBackAsync()
        {
            await Shell.Current.GoToAsync(".."); //navigation vers page précédente
        }
    }
}
