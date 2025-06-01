using System.Collections.ObjectModel;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using PLMobile.Services;
using PLMobile.ViewModels;
using PLMobile.Models;

namespace PLMobile;

public partial class BookTagsPage : ContentPage
{
    public BookTagsPage(BookTagsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        Loaded += async (_, _) => await viewModel.LoadTags();
    }
}
