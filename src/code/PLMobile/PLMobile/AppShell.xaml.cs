using PLMobile;

namespace PLMobile;

    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

        // Register routes for navigation
            Routing.RegisterRoute(nameof(LibraryPage), typeof(LibraryPage));
            Routing.RegisterRoute(nameof(ImportPage), typeof(ImportPage));
        Routing.RegisterRoute(nameof(TagsPage), typeof(TagsPage));
        }

        // Static method for back navigation (can be called from any pages)
        public static async Task GoBackAsync()
        {
            await Shell.Current.GoToAsync(".."); //navigation vers page précédente
    }
}
