using PLMobile.ViewModels;
using System.Diagnostics;
using System.Net;

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

        // Waiting for content rendering
        if (MainScrollView.Content != null)
        {
            MainScrollView.Content.SizeChanged += OnContentSizeChanged;
        }
    }

    private void OnContentSizeChanged(object sender, EventArgs e)
    {
        MainScrollView.Content.SizeChanged -= OnContentSizeChanged;

        Device.BeginInvokeOnMainThread(async () =>
        {
            await Task.Delay(50); // Additional delay
            await RestoreScrollPosition();
        });
    }

    private async Task RestoreScrollPosition()
    {
        if (MainScrollView.Content != null)
        {
            var scrollTo = MainScrollView.Content.Height * _viewModel.ScrollPosition;
            await MainScrollView.ScrollToAsync(0, scrollTo, false);
        }
    }

    private void OnScrollChanged(object sender, ScrolledEventArgs e)
    {
        if (MainScrollView.Content != null)
        {
            _viewModel.SaveReadingPosition(e.ScrollY, MainScrollView.Content.Height);
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (MainScrollView.Content != null)
        {
            MainScrollView.Content.SizeChanged -= OnContentSizeChanged;
        }
    }
}