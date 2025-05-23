using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PLMobile.Services;

namespace PLMobile.ViewModels
{
    public partial class ImportPageViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

        [ObservableProperty]
        private string _selectedFilePath;

        public ImportPageViewModel(ApiService apiService)
        {
            _apiService = apiService;
            Title = "Importer un livre";
        }

        [RelayCommand]
        private async Task ImportBook()
        {
            if (IsBusy) return;

            try
            {
                var result = await FilePicker.PickAsync(new PickOptions
                {
                    FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        { DevicePlatform.iOS, new[] { "com.apple.ibooks.epub" } },
                        { DevicePlatform.Android, new[] { "application/epub+zip" } },
                        { DevicePlatform.WinUI, new[] { ".epub" } },
                        { DevicePlatform.macOS, new[] { "epub" } }
                    })
                });

                if (result == null) return;

                IsBusy = true;

                var fileStream = await result.OpenReadAsync();
                var bytes = new byte[fileStream.Length];
                await fileStream.ReadAsync(bytes, 0, (int)fileStream.Length);

                await _apiService.UploadBookAsync(
                    Path.GetFileNameWithoutExtension(result.FileName),
                    bytes
                );

                await Shell.Current.DisplayAlert("Succès", "Le livre a été importé avec succès", "OK");
                await GoBack();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erreur", "Impossible d'importer le livre: " + ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task GoBack()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
} 