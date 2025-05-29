using System.IO.Compression;
using System.Xml;
using PLMobile.Services;
using System.Diagnostics;

namespace PLMobile;

public partial class ApiPage : ContentPage
{
    HttpClient client = new();
    bool useXml = false;

    public ApiPage()
    {
        InitializeComponent();
        // Set the default API endpoint in the endpoint text field
        endpoint.Text = ApiConfiguration.GetApiUrl();
        Debug.WriteLine($"Initial API URL: {endpoint.Text}");
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        try
        {
            // Use the ApiConfiguration to get the proper endpoint
            var apiUrl = endpoint.Text ?? ApiConfiguration.GetApiUrl();
            Debug.WriteLine($"Attempting to connect to: {apiUrl}");
            await DisplayAlert("Debug", $"Trying to connect to: {apiUrl}", "OK");

            var response = await client.GetAsync(apiUrl);
            Debug.WriteLine($"Response status: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                Debug.WriteLine("Successfully connected to API");
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
                title.Text = bookTitle;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"API Error: Status {response.StatusCode}, Content: {errorContent}");
                throw new Exception($"Bad status: {response.StatusCode}\nHeaders: {response.Headers}\nContent: {errorContent}");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception: {ex.Message}\nStack trace: {ex.StackTrace}");
            await DisplayAlert("Error", $"Error: {ex.Message}\n\nDetails: {ex.StackTrace}", "OK");
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