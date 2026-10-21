using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;


namespace proiectmip3
{
    public partial class Form1 : Form
    {
        private List<string> blockedKeywords = new List<string>
    {
    "facebook",
    "tiktok",
    "casino"
    };
        public Form1()
        {
            InitializeComponent();
        }
        private void NavigateToUrl()
        {
            string url = txtUrl.Text.Trim();
            if (url == "") return;

            // ha csak "google.com"-ot írnak, hozzátesszük a https://-t
            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                url = "https://" + url;

            webBrowser1.Navigate(url);
        }

        private void btnGo_Click(object sender, EventArgs e)
        {
            NavigateToUrl();
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            if (webBrowser1.CanGoBack)
                webBrowser1.GoBack();
        }

        private void btnForward_Click(object sender, EventArgs e)
        {
            if (webBrowser1.CanGoForward)
                webBrowser1.GoForward();
        }

        private void btnHome_Click(object sender, EventArgs e)
        {
            webBrowser1.GoHome();
        }

        private void txtUrl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;   // ne legyen "ding" hang
                NavigateToUrl();
            }
        }

        private void webBrowser1_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            string url = e.Url.ToString();

            bool blocked = IsBlockedAsync(url).Result;   // megvárjuk a Task eredményét

            if (blocked)
            {
                e.Cancel = true;
                MessageBox.Show("Ez az oldal le van tiltva!\n" + url);
            }
        }

        
        private bool IsBlockedLinq(string url)
        {
            url = url.ToLower();
            return blockedKeywords.Any(k => url.Contains(k.ToLower()));
        }

        //linq:
        private Task<bool> IsBlockedAsync(string url)
        {
            List<string> copy = blockedKeywords.ToList();   // másolat, hogy közben ne változzon
            string lowerUrl = url.ToLower();

            return Task.Run(() => copy.Any(k => lowerUrl.Contains(k.ToLower())));
        }

        //cer 5.

        private void Form1_Load(object sender, EventArgs e)
        {
            Trace.Listeners.Add(new TextWriterTraceListener("log.txt"));
            Trace.AutoFlush = true;

            Log("Az alkalmazás elindult");
            RefreshCombo();
            webBrowser1.GoHome();
        }
        private void Log(string message)
        {
            string line = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " | " + message;
            Task.Run(() => Trace.WriteLine(line));   // fără await
        }

        private async void btnAddKeyword_Click(object sender, EventArgs e)
        {
            await AddKeyword();
        }
        private async void txtKeyword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                await AddKeyword();
            }
        }
        private async Task AddKeyword()
        {
            string keyword = txtKeyword.Text.Trim().ToLower();
            if (keyword == "")
            {
                MessageBox.Show("Introduceți un cuvânt cheie!");
                return;
            }

            // linq: megnezzuk benne van e a listaban
            List<string> copy = blockedKeywords.ToList();
            bool exists = await Task.Run(() =>
                copy.Any(k => k.Equals(keyword, StringComparison.OrdinalIgnoreCase)));

            if (exists)
            {
                MessageBox.Show("Cuvântul \"" + keyword + "\" există deja în listă!");
                return;
            }

            blockedKeywords.Add(keyword);
            RefreshCombo();
            cmbKeywords.SelectedItem = keyword;
            txtKeyword.Clear();
            Log("Cuvânt cheie adăugat: " + keyword);
        }

        // one handler object for the whole form
        private SQLiteHandler db = new SQLiteHandler();

        private void RefreshCombo()
        {
            cmbKeywords.Items.Clear();
            foreach (string k in blockedKeywords.OrderBy(k => k))   // linq: abc sorrendbe helyezes
                cmbKeywords.Items.Add(k);

            if (cmbKeywords.Items.Count > 0)
                cmbKeywords.SelectedIndex = 0;
        }

        private void viewKeywordsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!db.IsConnected)
            {
                MessageBox.Show("Mai intai conecteaza-te la baza de date!");
                return;
            }

            List<string> keywords = db.GetAllKeywords();

            if (keywords.Count == 0)
            {
                MessageBox.Show("Nu exista cuvinte cheie in baza de date.");
                return;
            }

            // every keyword goes on a new line
            MessageBox.Show(string.Join("\n", keywords));
        }

        private void connectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                db.ConnectToDb();
                Log("Conectat la baza de date");
                MessageBox.Show("Conectat la baza de date!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare: " + ex.Message);
            }

        }

        private void disconnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            db.DisconnectFromDb();
            Log("Deconectat de la baza de date");
            MessageBox.Show("Deconectat!");
        }

        private void addKeywordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // cant add anything if were not connected
            if (!db.IsConnected)
            {
                MessageBox.Show("Mai intai conecteaza-te la baza de date!");
                return;
            }

            KeywordInput kwdForm = new KeywordInput();

            // only save when the user pressed ok
            if (kwdForm.ShowDialog() == DialogResult.OK)
            {
                string kwd = kwdForm.getInputText().Trim().ToLower();

                if (kwd == "")
                {
                    MessageBox.Show("Introduceti un cuvant cheie!");
                    return;
                }

                db.InsertKeyword(kwd);
                Log("Cuvant cheie salvat in DB: " + kwd);
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Web browser blokkolt kulcsszavakkal\nLabor 4 - MIP");
        }
    }

}
