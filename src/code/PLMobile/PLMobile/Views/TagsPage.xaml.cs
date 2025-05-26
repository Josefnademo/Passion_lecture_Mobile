using PLMobile.ViewModels;

namespace PLMobile.Views
{
    public partial class TagsPage : ContentPage
    {
        public TagsPage(TagsPageViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
} 