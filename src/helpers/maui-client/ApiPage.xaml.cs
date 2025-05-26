//Auteur : JMY
//Date   : 03.4.2024 
//Lieu   : ETML
//Descr. : squelette pour chargement de data à partir d’une api

using System.IO.Compression;
using System.Xml;
using static Android.Provider.MediaStore;

namespace HelloMaui1;

public partial class ApiPage : ContentPage
{
	HttpClient client = new();
	bool useXml = false;

	public ApiPage()
	{
		InitializeComponent();
	}

    private async void Button_Clicked(object sender, EventArgs e)
    {
		try
		{
			//Call API
            var response = await client.GetAsync(endpoint.Text);
			if (response.IsSuccessStatusCode)
			{
				var content = response.Content;

				//Open epub ZIP
				ZipArchive archive = new ZipArchive(content.ReadAsStream());
				var coverEntry = archive.GetEntry("OEBPS/Images/cover.png");
				var coverStream = coverEntry.Open();

				//Attach cover to UI
				cover.Source = ImageSource.FromStream(() => coverStream);

				//Load CONTENT (meta data)
				var bookTitle = "not found";
				var contentString = new StreamReader(archive.GetEntry("OEBPS/content.opf").Open()).ReadToEnd();

				
				if (useXml)
				{
                    #region XML version
                    //load meta-data from xml
                    var xmlDoc = new XmlDocument();
					xmlDoc.LoadXml(contentString);

					// Retrieve the title element
					XmlNode titleNode = xmlDoc.SelectSingleNode("//dc:title", GetNamespaceManager(xmlDoc));

					bookTitle = titleNode != null ? titleNode.InnerText : "not found with xml";
					#endregion
				}
				else
				{
                    #region plain text version
                    int start = contentString.IndexOf("<dc:title>") + 10;
                    int end = contentString.IndexOf("</dc:title>");

                    bookTitle = (start != -1 && end != -1) ? contentString.Substring(start, end - start) : "Title node not found.";
                    #endregion
                }
				title.Text=bookTitle;

            }
            else
			{
				throw new Exception($"Bad status : {response.StatusCode}, {response.Headers},{response.Content}");
			}
        }
		catch(Exception ex) {
			await DisplayAlert(ex.Message, ex.StackTrace,"ok");
		}
		

    }

    private static XmlNamespaceManager GetNamespaceManager(XmlDocument xmlDoc)
    {
        XmlNamespaceManager nsManager = new XmlNamespaceManager(xmlDoc.NameTable);
        nsManager.AddNamespace("dc", "http://purl.org/dc/elements/1.1/");
        return nsManager;
    }

    private void Switch_Toggled(object sender, ToggledEventArgs e)
    {
		useXml = e.Value;
    }
}