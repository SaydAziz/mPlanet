using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using mPlanet.Infrastructure;
using mPlanet.Models;
using mPlanet.Services;
using mPlanet.Services.Interfaces;
using mPlanet.Configuration;
using System.IO;


namespace mPlanet.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IRfidScannerService _scannerService;
        private readonly IDataExportService _dataExportService;

        private string _comPort = "";
        private string _statusMessage = "Ready";
        private bool _isConnected = false;
        private bool _isGrabbingScan = false;
        private bool _isExporting = false;
        private bool _canConnect = true;

        public ICommand ConnectCommand { get; }
        public ICommand GrabScanCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand AddTestDataCommand { get; }
        public ICommand DisconnectCommand { get; }
        

        public string ComPort
        {
            get => _comPort;
            set
            {
                if (SetProperty(ref _comPort, value))
                {
                    ((RelayCommand)ConnectCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                if (SetProperty(ref _isConnected, value))
                {
                    OnConnectionStateChanged();
                }
            }
        }

        public bool IsGrabbingScan
        {
            get => _isGrabbingScan;
            set
            {
                if (SetProperty(ref _isGrabbingScan, value))
                {
                    ((RelayCommand)GrabScanCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsExporting
        {
            get => _isExporting;
            set
            {
                if (SetProperty(ref _isExporting, value))
                {
                    ((RelayCommand)ExportCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public bool CanConnect
        {
            get => _canConnect;
            set
            {
                if (SetProperty(ref _canConnect, value))
                {
                    // ADDED: Update connect command when CanConnect changes
                    ((RelayCommand)ConnectCommand).RaiseCanExecuteChanged();
                }
            }
        }


        public ObservableCollection<TagInfo> ScannedTags { get; }

        public MainViewModel()
        {
            _scannerService = new ScannerMHandService();
            _dataExportService = new ExportJsonService();

            ScannedTags = new ObservableCollection<TagInfo>();

            _scannerService.StatusChanged += OnScannerStatusChanged;

            ConnectCommand = new RelayCommand(
                execute: async _ => await ExecuteConnectAsync(),
                canExecute: _ => CanExecuteConnect()
            );

            GrabScanCommand = new RelayCommand(
                execute: async _ => await ExecuteGrabScanAsync(),
                canExecute: _ => CanExecuteGrabScan()
            );

            ExportCommand = new RelayCommand(
                execute: async _ => await ExecuteExportAsync(),
                canExecute: _ => CanExecuteExport()
            );

            AddTestDataCommand = new RelayCommand(
                execute: _ => ExecuteAddTestData(),
                canExecute: _ => true
            );

            DisconnectCommand = new RelayCommand(
                execute: async _ => await ExecuteDisconnectAsync(),
                canExecute: _ => IsConnected
            );
        }

        private async Task ExecuteConnectAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ComPort))
                {
                    StatusMessage = "Please enter a COM port.";
                    return;
                }

                CanConnect = false;
                StatusMessage = "Connecting...";

                bool connected = await _scannerService.ConnectAsync(ComPort);
                IsConnected = connected;

                StatusMessage = connected ? $"Connected to COM port {ComPort}" : $"Failed to connect to COM port {ComPort}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Connection error: {ex.Message}";
                IsConnected = false;
            }
            finally
            {
                CanConnect = true;
            }
        }

        private bool CanExecuteConnect()
        {
            return !IsConnected && !string.IsNullOrWhiteSpace(ComPort) && CanConnect;
        }

        private async Task ExecuteGrabScanAsync()
        {
            try
            {
                IsGrabbingScan = true;
                StatusMessage = "Grabbing scanned tags...";

                ScannedTags.Clear();

                var newTags = await _scannerService.UpdateScannedTagsAsync();

                foreach (var tag in newTags)
                {
                    if (!ScannedTags.Any(existingTag => existingTag.Equals(tag)))
                    {
                        ScannedTags.Add(tag);
                    }
                }

                StatusMessage = $"Tags grabbed from scan. Found {newTags.Count()} tags.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Scan error: {ex.Message}";
            }
            finally
            {
                IsGrabbingScan = false;
            }
        }

        private bool CanExecuteGrabScan()
        {
            return IsConnected && !IsGrabbingScan;
        }

        private async Task ExecuteExportAsync()
        {
            try
            {
                if (!ScannedTags.Any())
                {
                    StatusMessage = "No tags to export.";
                    return;
                }

                IsExporting = true;
                StatusMessage = "Exporting data...";

                string fileName = $"RFID_Scan_{DateTime.Now:ddMMyyyyHHmmss}.json";
                string filePath = Path.Combine(AppSettings.DefaultExportPath, fileName);

                bool success = await _dataExportService.ExportAsync(ScannedTags, filePath, "json");

                StatusMessage = success ? $"Exported to: {filePath}" : "Export failed.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Export error: {ex.Message}";
            }
            finally
            {
                IsExporting = false;
            }
        }

        private bool CanExecuteExport()
        {
            return ScannedTags.Any() && !IsExporting;
        }


        private void ExecuteAddTestData()
        {
            string[] fakeData = new string[]
            {
                "3000-847293615208374951627384,4782",
                "3003-291847365019283746582019,8456",
                "3010-639182745830192847361029,2193",
                "2000-485729163084729385061927,9374",
                "7000-927384650192847362851047,5628",
                "9000-374829506182749305847261,3945"
            };

            ScannedTags.Clear();

            foreach (string tagData in fakeData)
            {
                string[] tagParts = tagData.Split('-', ',');
                if (tagParts.Length >= 3)
                {
                    var tag = new TagInfo(tagParts[0], tagParts[1], tagParts[2]);
                    ScannedTags.Add(tag);
                }
            }

            StatusMessage = $"Added {fakeData.Length} test tags.";
        }

        private async Task ExecuteDisconnectAsync()
        {
            try
            {
                StatusMessage = "Disconnecting...";
                await _scannerService.DisconnectAsync();
                IsConnected = false;
                StatusMessage = "Disconnected.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Disconnect error: {ex.Message}";
            }
        }

        private void OnScannerStatusChanged(object sender, string message)
        {
            StatusMessage = message;
        }

        private void OnConnectionStateChanged()
        {
            ((RelayCommand)GrabScanCommand).RaiseCanExecuteChanged();
            ((RelayCommand)DisconnectCommand).RaiseCanExecuteChanged();
            ((RelayCommand)ConnectCommand).RaiseCanExecuteChanged();
        }

        public async Task CleanupAsync()
        {
            try
            {
                if (IsConnected)
                {
                    await _scannerService.DisconnectAsync();
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Cleanup error: {ex.Message}";
            }
        }
    }
}
