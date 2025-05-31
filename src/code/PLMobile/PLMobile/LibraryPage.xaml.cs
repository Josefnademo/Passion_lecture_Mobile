using PLMobile.ViewModels;


namespace PLMobile
{
    public partial class LibraryPage : ContentPage
    {
        private readonly LibraryPageViewModel _viewModel;

        public LibraryPage(LibraryPageViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }

       
    }
}