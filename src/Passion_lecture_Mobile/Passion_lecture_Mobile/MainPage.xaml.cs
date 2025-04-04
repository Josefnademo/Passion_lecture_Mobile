namespace Passion_lecture_Mobile
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void NavigateToTagsPage(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(TagsPage));
        }

        private async void NavigateToLibraryPage(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(LibraryPage));
        }

        private async void NavigateToImportPage(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(ImportPage));
        }



    }

}
