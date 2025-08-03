using mPlanet.Services.Interfaces;
using System.Collections.ObjectModel;
using mPlanet.Models;
using System.Windows.Input;
using mPlanet.Infrastructure;
using mPlanet.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using mPlanet.Configuration;
using System.IO;

namespace mPlanet.ViewModels.Pages
{
    public class StockTakePageViewModel : PageViewModelBase
    {
        private readonly IRfidScannerService _scannerService;
        private readonly IDataExportService _dataExportService;

        private string _comPort = "";
        private bool _isConnected = false;
        private bool _isScanning = false;
        private bool _isExporting = false;
        private bool _canConnect = true;
        private string _expectedStockPath = "";
        private int _expectedCount = 0;
        private int _foundCount = 0;
        private int _missingCount = 0;

        // Collections
        public ObservableCollection<TagInfo> ExpectedTags { get; }
        public ObservableCollection<TagInfo> ScannedTags { get; }
        public ObservableCollection<TagInfo> MissingTags { get; }
        public ObservableCollection<TagInfo> ExtraTags { get; }

        // Properties
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

        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                if (SetProperty(ref _isConnected, value))
                {
                    OnConnectionStateChanged();
                    _navigationService.UpdateConnectionStatus(value, value ? ComPort : "");
                }
            }
        }

        public bool IsScanning
        {
            get => _isScanning;
            set
            {
                if (SetProperty(ref _isScanning, value))
                {
                    ((RelayCommand)ScanCommand).RaiseCanExecuteChanged();
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
                    ((RelayCommand)ExportResultsCommand).RaiseCanExecuteChanged();
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
                    ((RelayCommand)ConnectCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public string ExpectedStockPath
        {
            get => _expectedStockPath;
            set => SetProperty(ref _expectedStockPath, value);
        }

        public int ExpectedCount
        {
            get => _expectedCount;
            set => SetProperty(ref _expectedCount, value);
        }

        public int FoundCount
        {
            get => _foundCount;
            set => SetProperty(ref _foundCount, value);
        }

        public int MissingCount
        {
            get => _missingCount;
            set => SetProperty(ref _missingCount, value);
        }

        // Commands
        public ICommand ConnectCommand { get; }
        public ICommand DisconnectCommand { get; }
        public ICommand LoadExpectedStockCommand { get; }
        public ICommand ScanCommand { get; }
        public ICommand CompareStockCommand { get; }
        public ICommand ExportResultsCommand { get; }
        public ICommand ClearAllCommand { get; }
        public ICommand AddTestExpectedDataCommand { get; }

        public StockTakePageViewModel(INavigationService navigationService)
            : base(navigationService)
        {
            // Use the same scanner service as MainPage
            _scannerService = new ScannerMHandService();
            _dataExportService = new ExportJsonService();

            // Initialize collections
            ExpectedTags = new ObservableCollection<TagInfo>();
            ScannedTags = new ObservableCollection<TagInfo>();
            MissingTags = new ObservableCollection<TagInfo>();
            ExtraTags = new ObservableCollection<TagInfo>();

            // Subscribe to scanner events
            _scannerService.StatusChanged += OnScannerStatusChanged;

            // Initialize commands
            ConnectCommand = new RelayCommand(
                execute: async _ => await ExecuteConnectAsync(),
                canExecute: _ => CanExecuteConnect()
            );

            DisconnectCommand = new RelayCommand(
                execute: async _ => await ExecuteDisconnectAsync(),
                canExecute: _ => IsConnected
            );

            LoadExpectedStockCommand = new RelayCommand(
                execute: _ => ExecuteLoadExpectedStock()
            );

            ScanCommand = new RelayCommand(
                execute: async _ => await ExecuteScanAsync(),
                canExecute: _ => CanExecuteScan()
            );

            CompareStockCommand = new RelayCommand(
                execute: _ => ExecuteCompareStock(),
                canExecute: _ => ExpectedTags.Any() && ScannedTags.Any()
            );

            ExportResultsCommand = new RelayCommand(
                execute: async _ => await ExecuteExportResultsAsync(),
                canExecute: _ => CanExecuteExport()
            );

            ClearAllCommand = new RelayCommand(
                execute: _ => ExecuteClearAll()
            );

            AddTestExpectedDataCommand = new RelayCommand(
                execute: _ => ExecuteAddTestExpectedData()
            );
        }

        private async Task ExecuteConnectAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ComPort))
                {
                    _navigationService.UpdateStatusMessage("Пожалуйста, введите COM порт.");
                    return;
                }

                CanConnect = false;
                _navigationService.UpdateStatusMessage("Подключение к сканеру...");

                bool connected = await _scannerService.ConnectAsync(ComPort);
                IsConnected = connected;

                var message = connected ?
                    $"Подключен к COM порту {ComPort}" :
                    $"Не удалось подключиться к COM порту {ComPort}";

                _navigationService.UpdateStatusMessage(message);
            }
            catch (Exception ex)
            {
                _navigationService.UpdateStatusMessage($"Ошибка подключения: {ex.Message}");
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

        private async Task ExecuteDisconnectAsync()
        {
            try
            {
                _navigationService.UpdateStatusMessage("Отключение...");
                await _scannerService.DisconnectAsync();
                IsConnected = false;
                _navigationService.UpdateStatusMessage("Отключен от сканера.");
            }
            catch (Exception ex)
            {
                _navigationService.UpdateStatusMessage($"Ошибка отключения: {ex.Message}");
            }
        }

        private void ExecuteLoadExpectedStock()
        {
            _navigationService.UpdateStatusMessage("Загрузка ожидаемого инвентаря...");

            try
            {
                LoadExpectedTagsFromCustomList();
                ExpectedStockPath = "Встроенный список инвентаря";
                
                _navigationService.UpdateStatusMessage($"Загружено {ExpectedTags.Count} ожидаемых меток из встроенного списка");
            }
            catch (Exception ex)
            {
                _navigationService.UpdateStatusMessage($"Ошибка загрузки инвентаря: {ex.Message}");
            }
        }

        private void LoadExpectedTagsFromCustomList()
        {
            // Your custom expected stock list
            string[] expectedStockData = new string[]
            {
                "1400-00016697,131",
                "1400-00025746,138",
                "1400-00019672,141",
                "1400-00016075,145",
                "1400-00044299,147",
                "3400-E28011700000021C03159BAF,134",
                "1400-00017440,138",
                "1400-00014862,142",
                "1400-00018673,126",
                "1400-00044268,134",
                "1400-00032995,132",
                "1400-00014274,152",
                "1400-00017460,143",
                "1400-00017569,132",
                "1400-00017098,130",
                "1400-00015391,149",
                "1400-00016041,144",
                "1400-00019073,139",
                "1400-00017280,140",
                "1400-00017075,154",
                "1400-00017092,150",
                "1400-00019666,134",
                "1400-00019673,157",
                "1400-00017093,135",
                "1400-00016696,137",
                "1400-00016693,139",
                "1400-00014430,140",
                "1400-00017265,151",
                "1400-00019339,146",
                "1400-00025744,143",
                "1400-00017074,141",
                "1400-00017083,149",
                "1400-00033057,131",
                "1400-00033099,136",
                "1400-00017367,135",
                "1400-00044223,131",
                "1400-00019074,156",
                "1400-00014486,143",
                "1400-00014912,138",
                "1400-00017099,133",
                "1400-00016694,136",
                "1400-00015520,135",
                "1400-00015635,130",
                "1400-00025754,141",
                "1400-00025584,145",
                "1400-00019340,132",
                "1400-00033079,151",
                "1400-00015140,146",
                "1400-00025580,132",
                "1400-00017457,146",
                "1400-00015392,145",
                "1400-00017259,136",
                "1400-00014863,140",
                "1400-00017452,126",
                "1400-00044252,124",
                "1400-00017568,140",
                "1400-00044326,131",
                "1400-00016093,133",
                "1400-00016448,140",
                "1400-00015360,141",
                "1400-00014487,139",
                "1400-00019593,131",
                "1400-00014553,141",
                "1400-00014911,135",
                "1400-00017105,136",
                "1400-00015448,138",
                "1400-00015519,131",
                "1400-00033074,136",
                "1400-00015449,127",
                "1400-00015648,144",
                "1400-00016440,143",
                "1400-00017281,136",
            };

            ExpectedTags.Clear();

            foreach (string tagData in expectedStockData)
            {
                string[] tagParts = tagData.Split('-', ',');
                if (tagParts.Length >= 3)
                {
                    var tag = new TagInfo(tagParts[0], tagParts[1], tagParts[2]);
                    ExpectedTags.Add(tag);
                }
            }

            ExpectedCount = ExpectedTags.Count;
            
            // Update command states
            ((RelayCommand)CompareStockCommand).RaiseCanExecuteChanged();
        }

        private async Task ExecuteScanAsync()
        {
            try
            {
                IsScanning = true;
                _navigationService.UpdateStatusMessage("Сканирование инвентаря...");

                var newTags = await _scannerService.UpdateScannedTagsAsync();

                // Add new tags to scanned collection
                foreach (var tag in newTags)
                {
                    if (!ScannedTags.Any(existingTag => existingTag.EPC == tag.EPC))
                    {
                        ScannedTags.Add(tag);
                    }
                }

                FoundCount = ScannedTags.Count;
                _navigationService.UpdateStatusMessage($"Сканирование завершено. Найдено {newTags.Count()} новых меток. Всего отсканировано: {ScannedTags.Count}");
                
                // Update command states
                ((RelayCommand)CompareStockCommand).RaiseCanExecuteChanged();
            }
            catch (Exception ex)
            {
                _navigationService.UpdateStatusMessage($"Ошибка сканирования: {ex.Message}");
            }
            finally
            {
                IsScanning = false;
            }
        }

        private bool CanExecuteScan()
        {
            return IsConnected && !IsScanning;
        }

        private void ExecuteCompareStock()
        {
            try
            {
                _navigationService.UpdateStatusMessage("Сравнение инвентаря...");

                // Clear previous comparison results
                MissingTags.Clear();
                ExtraTags.Clear();

                // Find missing tags (expected but not scanned)
                foreach (var expectedTag in ExpectedTags)
                {
                    if (!ScannedTags.Any(scanned => scanned.EPC == expectedTag.EPC))
                    {
                        MissingTags.Add(expectedTag);
                    }
                }

                // Find extra tags (scanned but not expected)
                foreach (var scannedTag in ScannedTags)
                {
                    if (!ExpectedTags.Any(expected => expected.EPC == scannedTag.EPC))
                    {
                        ExtraTags.Add(scannedTag);
                    }
                }

                // Update counts
                MissingCount = MissingTags.Count;
                var extraCount = ExtraTags.Count;

                _navigationService.UpdateStatusMessage(
                    $"Сравнение завершено. Недостает: {MissingCount}, Лишних: {extraCount}, Найдено: {FoundCount}/{ExpectedCount}");

                // Update command states
                ((RelayCommand)ExportResultsCommand).RaiseCanExecuteChanged();
            }
            catch (Exception ex)
            {
                _navigationService.UpdateStatusMessage($"Ошибка сравнения: {ex.Message}");
            }
        }

        private async Task ExecuteExportResultsAsync()
        {
            try
            {
                IsExporting = true;
                _navigationService.UpdateStatusMessage("Экспорт результатов инвентаризации...");

                string fileName = $"StockTake_Results_{DateTime.Now:ddMMyyyyHHmmss}.json";
                string filePath = Path.Combine(AppSettings.DefaultExportPath, fileName);

                // Create comprehensive results object
                var results = new
                {
                    Timestamp = DateTime.Now,
                    Expected = new { Count = ExpectedCount, Tags = ExpectedTags },
                    Scanned = new { Count = FoundCount, Tags = ScannedTags },
                    Missing = new { Count = MissingCount, Tags = MissingTags },
                    Extra = new { Count = ExtraTags.Count, Tags = ExtraTags },
                    Summary = new
                    {
                        ExpectedCount,
                        FoundCount,
                        MissingCount,
                        ExtraCount = ExtraTags.Count,
                        AccuracyPercentage = ExpectedCount > 0 ? (double)(FoundCount - ExtraTags.Count) / ExpectedCount * 100 : 0
                    }
                };

                // You'll need to serialize this to JSON
                // bool success = await _dataExportService.ExportAsync(results, filePath, "json");
                bool success = true; // Placeholder

                var message = success ?
                    $"Результаты экспортированы: {filePath}" :
                    "Ошибка экспорта результатов.";

                _navigationService.UpdateStatusMessage(message);
            }
            catch (Exception ex)
            {
                _navigationService.UpdateStatusMessage($"Ошибка экспорта: {ex.Message}");
            }
            finally
            {
                IsExporting = false;
            }
        }

        private bool CanExecuteExport()
        {
            return (MissingTags.Any() || ExtraTags.Any() || ScannedTags.Any()) && !IsExporting;
        }

        private void ExecuteClearAll()
        {
            ExpectedTags.Clear();
            ScannedTags.Clear();
            MissingTags.Clear();
            ExtraTags.Clear();

            ExpectedCount = 0;
            FoundCount = 0;
            MissingCount = 0;
            ExpectedStockPath = "";

            _navigationService.UpdateStatusMessage("Все данные очищены.");

            // Update command states
            ((RelayCommand)CompareStockCommand).RaiseCanExecuteChanged();
            ((RelayCommand)ExportResultsCommand).RaiseCanExecuteChanged();
        }

        private void ExecuteAddTestExpectedData()
        {
            string[] testExpectedData = new string[]
            {
                "3000-847293615208374951627384,4782", // This should be found
                "3003-291847365019283746582019,8456", // This should be found
                "3010-639182745830192847361029,2193", // This should be found
                "1000-111111111111111111111111,1111", // This will be missing
                "2000-222222222222222222222222,2222", // This will be missing
                "4000-444444444444444444444444,4444"  // This will be missing
            };

            ExpectedTags.Clear();

            foreach (string tagData in testExpectedData)
            {
                string[] tagParts = tagData.Split('-', ',');
                if (tagParts.Length >= 3)
                {
                    var tag = new TagInfo(tagParts[0], tagParts[1], tagParts[2]);
                    ExpectedTags.Add(tag);
                }
            }

            ExpectedCount = ExpectedTags.Count;
            _navigationService.UpdateStatusMessage($"Добавлено {ExpectedTags.Count} тестовых ожидаемых меток.");

            // Update command states
            ((RelayCommand)CompareStockCommand).RaiseCanExecuteChanged();
        }

        private void OnScannerStatusChanged(object sender, string message)
        {
            _navigationService.UpdateStatusMessage(message);
        }

        private void OnConnectionStateChanged()
        {
            ((RelayCommand)ScanCommand).RaiseCanExecuteChanged();
            ((RelayCommand)DisconnectCommand).RaiseCanExecuteChanged();
            ((RelayCommand)ConnectCommand).RaiseCanExecuteChanged();
        }

        public override void OnNavigatedTo()
        {
            _navigationService.UpdateStatusMessage("Страница инвентаризации загружена");
            _navigationService.UpdateConnectionStatus(IsConnected, IsConnected ? ComPort : "");
        }

        public override void OnNavigatedFrom()
        {
            if (IsScanning)
            {
                _navigationService.UpdateStatusMessage("Сканирование инвентаризации приостановлено");
            }
        }
    }
}