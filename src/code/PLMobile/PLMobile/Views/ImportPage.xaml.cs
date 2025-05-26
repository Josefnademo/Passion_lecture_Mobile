using PLMobile.ViewModels;

namespace PLMobile.Views
{
    public partial class ImportPage : ContentPage
    {
        public ImportPage(ImportPageViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
} 