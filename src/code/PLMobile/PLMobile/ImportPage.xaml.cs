namespace PLMobile;

public partial class ImportPage : ContentPage
{
	public ImportPage()
	{
		InitializeComponent();
	}
    private async void ImportBook(object sender, EventArgs e)
    {
        //choose file, stock in db
    }

    //Use GoBackAsync function from Shell to get on previous page
    private async void GoBack(object sender, EventArgs e)
    {
        await AppShell.GoBackAsync();
    }
}