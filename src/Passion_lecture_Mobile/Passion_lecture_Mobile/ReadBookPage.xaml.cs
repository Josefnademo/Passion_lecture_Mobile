namespace Passion_lecture_Mobile;

public partial class ReadBookPage : ContentPage
{
	public ReadBookPage()
	{
		InitializeComponent();
	}

    // Methode ReadCurrentBook - current choosen book id , show text wich is in epub


    //Use GoBackAsync function from Shell to get on previous page
    private async void GoBack(object sender, EventArgs e)
    {
        await AppShell.GoBackAsync();
    }
}