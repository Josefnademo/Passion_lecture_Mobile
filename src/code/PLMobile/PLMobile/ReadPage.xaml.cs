namespace PLMobile;

public partial class ReadPage : ContentPage
{
	public ReadPage()
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