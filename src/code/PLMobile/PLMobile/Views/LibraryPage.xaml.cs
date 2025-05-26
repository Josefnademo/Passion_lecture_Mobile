using PLMobile.ViewModels;

namespace PLMobile.Views
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