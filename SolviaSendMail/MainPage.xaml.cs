using System.Net.Mail;
using System.Net;
using System.Xml.Linq;
using System.Xml;

namespace SolviaSendMail;

public partial class MainPage : ContentPage
{
    int count = 0;

    public MainPage()
    {
        InitializeComponent();
        btnUpdatePreview.Clicked += BtnUpdatePreview_Clicked;
        LoadMauiAsset();
    }

    async Task LoadMauiAsset()
    {
        using var stream = await FileSystem.OpenAppPackageFileAsync("MailTemplate.html");
        using var reader = new StreamReader(stream);

        txtBodyHtml.Text = reader.ReadToEnd();
    }

    void BtnUpdatePreview_Clicked(object sender, EventArgs e)
    {
        if (IsValidHtml(txtBodyHtml.Text))
        {
            var htmlSource = new HtmlWebViewSource
            {
                Html = txtBodyHtml.Text
            };
            htmlPreview.Source = htmlSource;
        }
        else
        {
            lblOutput.Text = "Invalid HTML!";

            var htmlSource = new HtmlWebViewSource
            {
                Html = txtBodyHtml.Text
            };
            htmlPreview.Source = htmlSource;
        }
    }
    bool IsValidHtml(string html)
    {
        try
        {
            // Attempt to load the text as XML
            XDocument.Parse(html);
            return true;
        }
        catch (XmlException)
        {
            // If parsing fails, the HTML is not well-formed
            return false;
        }
    }
    // Reset border colors
    void resetBorderColor()
    {
        // Reset border colors
        Color transparent = new Color(0, 0, 0, 0);  // This creates a transparent color
        frameSender.BorderColor = transparent;
        frameRecipient.BorderColor = transparent;
        frameSmtpServer.BorderColor = transparent;
        framePort.BorderColor = transparent;
        frameUsername.BorderColor = transparent;
        framePassword.BorderColor = transparent;
        frameSubject.BorderColor = transparent;
    }

    bool validateInput()
    {
        // Validate input
        bool hasError = false;
        if (string.IsNullOrEmpty(txtSender.Text)) { frameSender.BorderColor = Colors.Red; hasError = true; }
        if (string.IsNullOrEmpty(txtRecipient.Text)) { frameRecipient.BorderColor = Colors.Red; hasError = true; }
        if (string.IsNullOrEmpty(txtSmtpServer.Text)) { frameSmtpServer.BorderColor = Colors.Red; hasError = true; }
        if (string.IsNullOrEmpty(txtPort.Text)) { framePort.BorderColor = Colors.Red; hasError = true; }
        if (authSwitch.IsToggled && string.IsNullOrEmpty(txtUsername.Text)) { frameUsername.BorderColor = Colors.Red; hasError = true; }
        if (authSwitch.IsToggled && string.IsNullOrEmpty(txtPassword.Text)) { framePassword.BorderColor = Colors.Red; hasError = true; }
        if (string.IsNullOrEmpty(txtSubject.Text)) { frameSubject.BorderColor = Colors.Red; hasError = true; }
        if (string.IsNullOrEmpty(txtBodyHtml.Text)) { frameBody.BorderColor = Colors.Red; hasError = true; }
        return hasError;
    }

    void btnSend_Click(System.Object sender, System.EventArgs e)
    {
        resetBorderColor();
        // Validate input
        if (validateInput())
        {
            lblOutput.Text = "Please fill in all fields.";
            return;
        }

        if (!int.TryParse(txtPort.Text, out int port))
        {
            framePort.BorderColor = Colors.Red;
            lblOutput.Text = "Please enter a valid port number.";
            return;
        }

        // Create and send email
        MailMessage mail = new MailMessage(txtSender.Text, txtRecipient.Text);
        SmtpClient client = new SmtpClient(txtSmtpServer.Text, port);

        client.DeliveryMethod = SmtpDeliveryMethod.Network;
        client.UseDefaultCredentials = false;
        if (authSwitch.IsToggled)
        {
            client.Credentials = new NetworkCredential(txtUsername.Text, txtPassword.Text);
        }

        mail.Subject = txtSubject.Text;
        mail.Body = txtBodyHtml.Text;
        mail.IsBodyHtml = htmlSwitch.IsToggled;

        try
        {
            client.Send(mail);
            lblOutput.Text = "Mail sent successfully";
        }
        catch (SmtpException ex)
        {
            lblOutput.Text = $"Error in sending email: {ex.Message}";
        }
        catch (Exception ex)
        {
            lblOutput.Text = $"General Error: {ex.Message}";
        }
    }
}

