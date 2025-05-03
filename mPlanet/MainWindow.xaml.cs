using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Net;
using MegawareDLL;
using Newtonsoft.Json;
using System.Text;

namespace mPlanet
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private MegawareMHand mHandScanner;
        private ObservableCollection<TagInfo> scannedTags;

        private HttpListener apiListener;
        private const string url = "http://localhost:8080/tags/";

        public MainWindow()
        {
            InitializeComponent();
            scannedTagsListView.SizeChanged += ListView_SizeChanged;

            mHandScanner = new MegawareMHand();
            scannedTags = new ObservableCollection<TagInfo>();
            scannedTagsListView.ItemsSource = scannedTags;
            StartServer();
        }

        private void UpdateScannedTags()
        {
            mHandScanner.retrieveEPC();
            string[] scannedEPCs = mHandScanner.getFullInfo();

            foreach (string TagInfo in scannedEPCs)
            {
                string[] tagParts = TagInfo.Split('-', ',');
                if (tagParts.Length >= 3)
                {
                    TagInfo tag = new TagInfo(
                       pc: tagParts[0],
                       epc: tagParts[1],
                       rssi: tagParts[2]
                    );

                    if (!scannedTags.Contains(tag))
                    {
                        scannedTags.Add(tag);
                    }
                }
            }

            scannedTagsListView.ItemsSource = null;
            scannedTagsListView.ItemsSource = scannedTags;
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            string comPort = txtConnectionPort.Text;
            if (comPort != "")
            {
                if (mHandScanner.connect(comPort))
                {
                    MessageBox.Show($"Подключение к COM-порт {comPort} успешно.");
                }
                else
                {
                    MessageBox.Show($"Подключение к COM-порт {comPort} не удалось.");
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, введите COM-порт.");
            }
        }

        private void btnFetchScan_Click(object sender, RoutedEventArgs e)
        {
            scannedTags.Clear();

            mHandScanner.setProfile(30, 400, 99); //Placeholder values
            mHandScanner.setCurrentProfileH6();

            UpdateScannedTags(); 
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            var data = new
            {
                ScanDate = $"{DateTime.Now:ddMMyyyy}",
                Tags = scannedTags
            };

            string jsonData = JsonConvert.SerializeObject(data, Formatting.Indented);

            string fileName = $"RFID_Scan_{DateTime.Now:yyyyMMddHHmmss}.json";
            File.WriteAllText(fileName, jsonData);

            MessageBox.Show($"Данные успешно экспортированы в {fileName}");
        }

        private async void StartServer()
        {
            apiListener = new HttpListener();
            apiListener.Prefixes.Add(url);
            apiListener.Start();
            Console.WriteLine("Listening...");

            while (true)
            {
                HttpListenerContext context = await apiListener.GetContextAsync();
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;

                if (request.HttpMethod == "GET" && request.Url.AbsolutePath == "/tags")
                {
                    var data = new
                    {
                        ScanDate = $"{DateTime.Now:dd.MM.yyyy}",
                        Tags = scannedTags
                    };

                    string jsonData = JsonConvert.SerializeObject(data, Formatting.Indented);

                    byte[] buffer = Encoding.UTF8.GetBytes(jsonData);
                    response.ContentLength64 = buffer.Length;
                    response.ContentType = "application/json";
                    await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);

                }

                response.Close();
            }
        }

        private void btnTestData_Click(object sender, RoutedEventArgs e)
        {
            string[] fakeData = new string[6];
            fakeData[0] = "3000-123456789012345678901234,6196";
            fakeData[1] = "3003-123456789012345678901234,2196";
            fakeData[2] = "3010-123456789012345678901234,1196";
            fakeData[3] = "2000-123456789012345678901234,6198";
            fakeData[4] = "7000-123456789012345678901234,7197";
            fakeData[5] = "9000-123456789012345678901234,6196";

            string[] scannedEPCs = fakeData;

            foreach (string TagInfo in scannedEPCs)
            {
                string[] tagParts = TagInfo.Split('-', ',');
                if (tagParts.Length >= 3)
                {
                    TagInfo tag = new TagInfo(
                       pc: tagParts[0],
                       epc: tagParts[1],
                       rssi: tagParts[2]
                    );

                    if (!scannedTags.Contains(tag))
                    {
                        scannedTags.Add(tag);
                    }
                }
            }

            scannedTagsListView.ItemsSource = null;
            scannedTagsListView.ItemsSource = scannedTags;
        }

        private void ListView_SizeChanged(Object sender, SizeChangedEventArgs e)
        {
            // Define minimum widths for the columns
            double minEpcColumnWidth = 200;
            double minRssiColumnWidth = 120;

            // Calculate the remaining width after subtracting the widths of other elements
            double remainingWidth = scannedTagsListView.ActualWidth - SystemParameters.VerticalScrollBarWidth;

            // Ensure there's at least some space to allocate
            if (remainingWidth > (minEpcColumnWidth + minRssiColumnWidth))
            {
                // Distribute the remaining width equally between the columns
                double newWidth = remainingWidth / 2;

                epcColumn.Width = newWidth > minEpcColumnWidth ? newWidth : minEpcColumnWidth;
                rssiColumn.Width = newWidth > minRssiColumnWidth ? newWidth : minRssiColumnWidth;
            }
            else
            {
                // If not enough space, use the minimum widths
                epcColumn.Width = minEpcColumnWidth;
                rssiColumn.Width = minRssiColumnWidth;
            }
        }
    }
}
