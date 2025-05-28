using PLMobile.ViewModels;

namespace PLMobile;

    public partial class MainPage : ContentPage
    {
    public MainPage(MainPageViewModel viewModel)
        {
            InitializeComponent();
        BindingContext = viewModel;
        }
}
