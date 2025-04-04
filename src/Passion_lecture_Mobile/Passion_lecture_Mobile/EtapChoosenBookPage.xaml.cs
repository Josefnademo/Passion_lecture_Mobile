namespace Passion_lecture_Mobile;

public partial class EtapChoosenBookPage : ContentPage
{
	public EtapChoosenBookPage()
	{
		InitializeComponent();
	}
    //method for buttons 1(which will redirect to "ReadBook"), 2(wich will redirect to "EditPage")



    //Redirect to "ReadBook"
    private async void ReadThisBook(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(ReadBookPage));
    }


    //Redirect to "EditPage"
    private async void ModifyThisBook(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(EditPage));
    }


    //Use GoBackAsync function from Shell to get on previous page
    private async void GoBack(object sender, EventArgs e)
    {
        await AppShell.GoBackAsync();
    }
}