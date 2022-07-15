using System;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Net;
using System.IO.Compression;
using System.Diagnostics;

namespace SpiderSolitaireUpdater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private readonly string[] files = new string[] { "Spider Solitaire.deps.json", "Spider Solitaire.dll", "Spider Solitaire.exe", "Spider Solitaire.pdb", "Spider Solitaire.runtimeconfig.json" };
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Log("");
            StatusText.Text = "Checking version number";
            Log("Checking version");

            if (!File.Exists(@"args.txt")) Exit(new FileNotFoundException());
            string[] input = File.ReadAllLines(@"args.txt");
            if (input.Length != 1) Exit(new FileFormatException());
            File.Delete(@"args.txt");

            Log($"Downloadig version {input[0]}\nDownload started");
            MessageBoxResult result = MessageBox.Show("Procced with download?","",MessageBoxButton.YesNo,MessageBoxImage.Question);
            if (result == MessageBoxResult.No) Exit(null);
            Download(input[0]);
        }

        private static void Exit(Exception? ex)
        {
            if (ex != null) { MessageBox.Show("There has been an error. in case some file have been deleted, a backup has been created in /backup folder\n\n" + ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error); Environment.Exit(1); }
            Environment.Exit(0);
        }

        private void Download(string version)
        {
            StatusText.Text = "Downloading";
            using (WebClient wc = new WebClient())
            {
                wc.DownloadProgressChanged += DownloadProgressChanged;
                wc.DownloadFileAsync(
                    new Uri($"https://github.com/CrusaderSVK287/Spider-Solitaire/releases/download/v{version}/Spider-Solitaire-v{version}.zip"),
                    "update.zip"
                );
            }
        }
        // Event to track the progress
        void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Log($"Downloaded {e.BytesReceived} bytes out of {e.TotalBytesToReceive} total bytes");
            ProgressBar.Value = e.ProgressPercentage;
            if (e.BytesReceived != e.TotalBytesToReceive) return;
            Log("Download Complete");
            Update();
        }

        private void Log(string text)
        {
            LogText.Text += text + "\n";
            ScrollLog.ScrollToBottom();
        }

        private void Update()
        {
            Log("Extracting files");
            try
            {
                DeleteBackup();
                StatusText.Text = "Creating backup";
                foreach (string file in files)
                {
                    if (File.Exists(file)) File.Move(file, @"backup/" + file);
                }
                StatusText.Text = "Extracting";
                ZipFile.ExtractToDirectory("update.zip", Directory.GetCurrentDirectory());
                File.Delete(@"update.zip");
                Log("Extraction complete");
                StatusText.Text = "Done";
            }
            catch (Exception ex)
            {
                Exit(ex);
            }
            MessageBoxResult result = MessageBox.Show("Update complete. Would you like to start the game now?","Done",MessageBoxButton.YesNo,MessageBoxImage.Question);
            if (result == MessageBoxResult.No) Exit(null);
            Close();
            Process.Start("Spider Solitaire.exe");
        }

        private void DeleteBackup()
        {
            if (!Directory.Exists(@"backup"))
            {
                Directory.CreateDirectory(@"backup");
                return;
            }

            foreach (string file in files)
            {
                if (File.Exists(@"backup/" + file)) File.Delete(@"backup/" + file);
            }
        }
    }
}
