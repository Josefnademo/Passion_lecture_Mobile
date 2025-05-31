using PLMobile.ViewModels;
using System.Web;

namespace PLMobile;

public partial class ReadPage : ContentPage
{
    private readonly ReadPageViewModel _viewModel;

    public ReadPage(ReadPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadBook();

        // Restore scroll position
        await MainScrollView.ScrollToAsync(0, _viewModel.ScrollPosition, false);
    }

    private void OnScrollChanged(object sender, ScrolledEventArgs e)
    {
        // Maintaining position while scrolling
        if (MainScrollView.ContentSize.Height > 0)
        {
            _viewModel.ScrollPosition = e.ScrollY;
            _viewModel.SavePosition(e.ScrollY / MainScrollView.ContentSize.Height);
        }
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        // Handle Shell navigation parameters
        if (Shell.Current.CurrentState.Location.OriginalString.Contains("bookId="))
        {
            var bookId = HttpUtility.ParseQueryString(
                new Uri(Shell.Current.CurrentState.Location.OriginalString).Query)["bookId"];
            _viewModel.BookId = bookId;
        }
        else if (Shell.Current.CurrentState.Location.OriginalString.Contains("numericBookId="))
        {
            var numericId = HttpUtility.ParseQueryString(
                new Uri(Shell.Current.CurrentState.Location.OriginalString).Query)["numericBookId"];
            if (int.TryParse(numericId, out int id))
            {
                _viewModel.NumericBookId = id;
            }
        }
    }
}