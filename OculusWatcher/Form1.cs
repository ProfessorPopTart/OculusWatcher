using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OculusWatcher
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
            timer1_Tick(null, null);
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            bool soldOut = false;
            var block = webBrowser1.Document.GetElementById("priceblock-wrapper-wrapper");
            if (block == null)
                return;

            HtmlElementCollection spans = block.GetElementsByTagName("span");
            foreach (HtmlElement span in spans)
            {
                if (span.InnerText == "Loading")
                    return;

                if (span.InnerText == "Sold Out Online")
                {
                    webBrowser1.DocumentCompleted -= webBrowser1_DocumentCompleted;
                    soldOut = true;
                }
            }

            if (!soldOut)
            {
                webBrowser1.DocumentCompleted -= webBrowser1_DocumentCompleted;
                Notify("In Stock at Best Buy!");
            }
            else
                this.Text = "Oculus Watcher - Out of Stock as of " + DateTime.Now;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            webBrowser1.DocumentCompleted += webBrowser1_DocumentCompleted;
            webBrowser1.Navigate(tbxUrl.Text);
        }

        private bool Notify(string msg)
        {
            this.Text = "Oculus Watcher - " + msg;
            var message = new MailMessage();
            message.From = new MailAddress(tbxFrom.Text);

            string[] emails = tbxTo.Text.Split(',');
            foreach(string email in emails)
                message.To.Add(new MailAddress(email.Trim()));

            message.Subject = "Oculus";
            message.Body = msg + " " + tbxUrl.Text;
            message.BodyEncoding = Encoding.UTF8;

            var client = new SmtpClient();
            client.Host = "Smtp.Gmail.com";
            client.Port = 587;
            client.EnableSsl = true;
            client.Timeout = 10000;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(tbxFrom.Text, tbxPwd.Text);
            try
            {
                client.Send(message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to send message. Make sure you allow less secure apps for the gmail account and try again.");
                Process.Start("chrome.exe", "https://www.google.com/settings/security/lesssecureapps");
                return false;
            }
            return true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (Notify("Oculus Test Message"))
                MessageBox.Show("Email sent successfully!");
        }
    }
}
