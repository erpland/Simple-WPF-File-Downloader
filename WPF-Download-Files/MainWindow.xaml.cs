using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.ComponentModel;
using System.Net;
using Microsoft.Win32;

namespace WPF_Download_Files
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BackgroundWorker backgroundWorker;
        public MainWindow()
        {
            InitializeComponent();
            btnExit.Click += (s, e) => Application.Current.Shutdown();
            btnDownload.Click += OnDownloadButtonClick;
            btnClear.Click += OnClearButtonClick;
            btnCancelDownload.Click += OnCancelDownloadButtonClick;
            backgroundWorker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            backgroundWorker.DoWork += BackgroundWorker_DoWork;
            backgroundWorker.ProgressChanged += BackgroundWorker_ProgressChanged;
            backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;
        }
        private void OnCancelDownloadButtonClick(object sender, RoutedEventArgs e)
        {
            if (backgroundWorker.IsBusy)
            {
                backgroundWorker.CancelAsync();
            }
        }
        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show($"Error: {e.Error.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (e.Cancelled)
            {
                Log("Download canceled.");
                MessageBox.Show("Download Has Been Canceled By User.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                prograssStack.Visibility = Visibility.Hidden;
            }
            else
            {
                Log("Download completed successfully.");
                MessageBox.Show("Download completed successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            btnDownload.IsEnabled = true;
        }

        private void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            prograssBar.Value = e.ProgressPercentage + 1;
        }

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var arguments = e.Argument as Tuple<string, string>;
            string url = arguments.Item1;
            string downloadPath = arguments.Item2;
            Uri uri = new Uri(url);

            using (WebClient webClient = new WebClient())
            {
                webClient.DownloadProgressChanged += (s, progressArgs) =>
                {
                    backgroundWorker.ReportProgress(progressArgs.ProgressPercentage);
                };

                webClient.DownloadFileAsync(uri, downloadPath);

                while (webClient.IsBusy)
                {
                    if (backgroundWorker.CancellationPending)
                    {
                        webClient.CancelAsync();
                        e.Cancel = true;
                        break;
                    }
                    System.Threading.Thread.Sleep(100);
                }
            }
        }
        private void Log(string message)
        {
            string timestamp = DateTime.Now.ToString("HH:mm");
            listLog.Items.Insert(0,$"{timestamp} - {message}");
        }
        private void OnDownloadButtonClick(object sender, RoutedEventArgs e)
        {
            btnDownload.IsEnabled = false;

            string url = txtUrl.Text;
            if (!string.IsNullOrWhiteSpace(url))
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    FileName = url.Split('/').Last(),
                    Filter = "All files (*.*)|*.*"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    string downloadPath = saveFileDialog.FileName;
                    prograssStack.Visibility = Visibility.Visible;
                    Log($"Download {saveFileDialog.SafeFileName} has started");
                    backgroundWorker.RunWorkerAsync(new Tuple<string, string>(url, downloadPath));
                }
                else
                {
                    btnDownload.IsEnabled = true;
                }
            }
            else
            {
                MessageBox.Show("Please enter a valid URL.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                btnDownload.IsEnabled = true;
            }
        }
        private void OnClearButtonClick(object sender, RoutedEventArgs e)
        {
            txtUrl.Clear();
        }
    }
}
