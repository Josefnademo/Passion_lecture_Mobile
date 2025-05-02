using System.Text;

namespace PLMobile;

public partial class TagsPage : ContentPage
{
	public TagsPage()
	{
		InitializeComponent();
	}

    private async void CreateTag(object sender, EventArgs e)
    {
        //read what user wrote, create tag with this , stock in db
    }

    //Use GoBackAsync function from Shell to get on previous page
    private async void GoBack(object sender, EventArgs e)
    {
        await AppShell.GoBackAsync();
    }
}