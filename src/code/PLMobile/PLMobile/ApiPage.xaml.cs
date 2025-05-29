using PLMobile.ViewModels;

namespace PLMobile;

public partial class ApiPage : ContentPage
{
    public ApiPage(ApiPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
} 