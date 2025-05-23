using CommunityToolkit.Mvvm.ComponentModel;

namespace PLMobile.ViewModels
{
    public partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotBusy))]
        private bool _isBusy;

        [ObservableProperty]
        private string _title;

        public bool IsNotBusy => !IsBusy;

        public BaseViewModel()
        {
            Title = string.Empty;
        }
    }
} 