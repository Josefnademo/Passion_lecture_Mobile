namespace Passion_lecture_Mobile;

public partial class LibraryPage : ContentPage
{
	public LibraryPage()
	{
		InitializeComponent();
	}

    //Use GoBackAsync function from Shell to get on previous page
    private async void GoBack(object sender, EventArgs e)
    {
        await AppShell.GoBackAsync();
    }
}