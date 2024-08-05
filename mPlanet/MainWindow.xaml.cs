using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
using System.Windows.Shapes;
using MegawareDLL;
using Newtonsoft.Json;

namespace mPlanet
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private MegawareMHand mHandScanner;
        private ObservableCollection<TagInfo> scannedTags;

        public MainWindow()
        {
            InitializeComponent();
            mHandScanner = new MegawareMHand();
            scannedTags = new ObservableCollection<TagInfo>();
            scannedTagsListView.ItemsSource = scannedTags;
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
            if (mHandScanner.connect(comPort))      
            {
                MessageBox.Show($"Connection to {comPort} successful.");
            }
            else
            {
                MessageBox.Show($"Connection to {comPort} unsuccessful.");
            }
        }

        private void btnFetchScan_Click(object sender, RoutedEventArgs e)
        {
            mHandScanner.setProfile(30, 400, 99); //Placeholder values
            mHandScanner.setCurrentProfileH6();

            UpdateScannedTags(); 
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            var data = new
            {
                ScanDate = DateTime.Now,
                Tags = scannedTags
            };

            string jsonData = JsonConvert.SerializeObject(data, Formatting.Indented);

            string fileName = $"RFID_Scan_{DateTime.Now:yyyyMMddHHmmss}.json";
            File.WriteAllText(fileName, jsonData);

            MessageBox.Show($"Data exported successfully to {fileName}");
        }
    }
}
