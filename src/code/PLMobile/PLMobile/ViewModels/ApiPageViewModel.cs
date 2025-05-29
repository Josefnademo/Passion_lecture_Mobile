using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PLMobile.Models;
using PLMobile.Services;
using System.IO.Compression;

namespace PLMobile.ViewModels
{
    public partial class ApiPageViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

        [ObservableProperty]
        private ImageSource _coverImage;

        [ObservableProperty]
        private string _bookTitle;

        [ObservableProperty]
        private bool _useXml;

        public ApiPageViewModel(ApiService apiService)
        {
            _apiService = apiService;
            Title = "API Test";
        }

        [RelayCommand]
        private async Task LoadBook()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;

                // Get the first book from the list
                var books = await _apiService.GetBooksAsync();
                var firstBook = books.FirstOrDefault();
                if (firstBook == null)
                {
                    await Shell.Current.DisplayAlert("Error", "No books found", "OK");
                    return;
                }

                var bookContent = await _apiService.GetBookEpubAsync(firstBook.Id);

                using (var stream = new MemoryStream(bookContent))
                using (var archive = new ZipArchive(stream))
                {
                    // Load cover
                    var coverEntry = archive.GetEntry("OEBPS/Images/cover.png");
                    if (coverEntry != null)
                    {
                        using var coverStream = coverEntry.Open();
                        CoverImage = ImageSource.FromStream(() => coverEntry.Open());
                    }

                    // Load metadata
                    var contentEntry = archive.GetEntry("OEBPS/content.opf");
                    if (contentEntry != null)
                    {
                        using var contentStream = contentEntry.Open();
                        using var reader = new StreamReader(contentStream);
                        var contentString = await reader.ReadToEndAsync();

                        if (UseXml)
                        {
                            var xmlDoc = new System.Xml.XmlDocument();
                            xmlDoc.LoadXml(contentString);
                            var nsManager = new System.Xml.XmlNamespaceManager(xmlDoc.NameTable);
                            nsManager.AddNamespace("dc", "http://purl.org/dc/elements/1.1/");
                            var titleNode = xmlDoc.SelectSingleNode("//dc:title", nsManager);
                            BookTitle = titleNode?.InnerText ?? "Title not found";
                        }
                        else
                        {
                            int start = contentString.IndexOf("<dc:title>") + 10;
                            int end = contentString.IndexOf("</dc:title>");
                            BookTitle = (start != -1 && end != -1) 
                                ? contentString.Substring(start, end - start) 
                                : "Title not found";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", 
                    $"Could not load book: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
} 