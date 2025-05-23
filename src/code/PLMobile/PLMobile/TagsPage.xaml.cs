using System.Collections.ObjectModel;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using PLMobile.Services;
using PLMobile.ViewModels;
using PLMobile.Models;

namespace PLMobile;

public partial class TagsPage : ContentPage
{
    public TagsPage(ApiService apiService, TagsPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}

