using PLMobile.ViewModels;


namespace PLMobile
{
    public partial class LibraryPage : ContentPage
    {
        public LibraryPage(LibraryPageViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}