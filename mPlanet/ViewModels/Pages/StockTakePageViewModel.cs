using System.Collections.ObjectModel;
using System.Windows.Input;
using mPlanet.Infrastructure;
using mPlanet.Services.Interfaces;
using mPlanet.Models;
using System.Linq;
using System;

namespace mPlanet.ViewModels.Pages
{
    public class StockTakePageViewModel : PageViewModelBase
    {
        private readonly IRfidScannerService _scannerService;
        private readonly IDataExportService _exportService;
        
        private string _comPort = "COM3";
        private bool _isConnected = false;
        private string _currentView = "Missing"; // Default to Missing view
        
        // Settings for taglist views
        private bool _showPhotos = false;
        private bool _isGridView = true; // true for grid view, false for list view

        // Collections
        private ObservableCollection<TagInfo> _expectedTags = new ObservableCollection<TagInfo>();
        private ObservableCollection<TagInfo> _scannedTags = new ObservableCollection<TagInfo>();
        private ObservableCollection<TagInfo> _missingTags = new ObservableCollection<TagInfo>();
        private ObservableCollection<TagInfo> _extraTags = new ObservableCollection<TagInfo>();
        private ObservableCollection<TagInfo> _foundTags = new ObservableCollection<TagInfo>();

        // Summary counts
        private int _expectedCount = 0;
        private int _foundCount = 0;
        private int _missingCount = 0;

        // Properties
        public string ComPort
        {
            get => _comPort;
            set => SetProperty(ref _comPort, value);
        }

        public bool IsConnected
        {
            get => _isConnected;
            set => SetProperty(ref _isConnected, value);
        }

        public string CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        public ObservableCollection<TagInfo> ExpectedTags
        {
            get => _expectedTags;
            set => SetProperty(ref _expectedTags, value);
        }

        public ObservableCollection<TagInfo> ScannedTags
        {
            get => _scannedTags;
            set => SetProperty(ref _scannedTags, value);
        }

        public ObservableCollection<TagInfo> MissingTags
        {
            get => _missingTags;
            set => SetProperty(ref _missingTags, value);
        }

        public ObservableCollection<TagInfo> ExtraTags
        {
            get => _extraTags;
            set => SetProperty(ref _extraTags, value);
        }

        public ObservableCollection<TagInfo> FoundTags
        {
            get => _foundTags;
            set => SetProperty(ref _foundTags, value);
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

        public bool ShowPhotos
        {
            get => _showPhotos;
            set => SetProperty(ref _showPhotos, value);
        }

        public bool IsGridView
        {
            get => _isGridView;
            set => SetProperty(ref _isGridView, value);
        }

        // Commands
        public ICommand ConnectCommand { get; }
        public ICommand DisconnectCommand { get; }
        public ICommand LoadExpectedStockCommand { get; }
        public ICommand AddTestExpectedDataCommand { get; }
        public ICommand ScanCommand { get; }
        public ICommand CompareStockCommand { get; }
        public ICommand ExportResultsCommand { get; }
        public ICommand ClearAllCommand { get; }
        
        // New clickable status commands
        public ICommand ShowExpectedCommand { get; }
        public ICommand ShowFoundCommand { get; }
        public ICommand ShowMissingCommand { get; }
        public ICommand ShowExtraCommand { get; }

        public StockTakePageViewModel(INavigationService navigationService) : base(navigationService)
        {
            // Initialize commands
            ConnectCommand = new RelayCommand(_ => ConnectToScanner(), _ => !IsConnected);
            DisconnectCommand = new RelayCommand(_ => DisconnectFromScanner(), _ => IsConnected);
            LoadExpectedStockCommand = new RelayCommand(_ => LoadExpectedStock());
            AddTestExpectedDataCommand = new RelayCommand(_ => AddTestExpectedData());
            ScanCommand = new RelayCommand(_ => PerformScan(), _ => IsConnected);
            CompareStockCommand = new RelayCommand(_ => CompareStock());
            ExportResultsCommand = new RelayCommand(_ => ExportResults());
            ClearAllCommand = new RelayCommand(_ => ClearAll());
            
            // Initialize the new status button commands
            ShowExpectedCommand = new RelayCommand(_ => ShowExpectedTags());
            ShowFoundCommand = new RelayCommand(_ => ShowFoundTags());
            ShowMissingCommand = new RelayCommand(_ => ShowMissingTags());
            ShowExtraCommand = new RelayCommand(_ => ShowExtraTags());

            // Initialize services (you'll need to inject these)
            // _scannerService = scannerService;
            // _exportService = exportService;
        }

        private async void ConnectToScanner()
        {
            try
            {
                if (_scannerService != null)
                {
                    IsConnected = await _scannerService.ConnectAsync(ComPort);
                    _navigationService.UpdateConnectionStatus(IsConnected, ComPort);
                    _navigationService.UpdateStatusMessage(IsConnected ? 
                        $"Подключен к {ComPort}" : 
                        $"Не удалось подключиться к {ComPort}");
                }
            }
            catch (Exception ex)
            {
                _navigationService.UpdateStatusMessage($"Ошибка подключения: {ex.Message}");
            }
        }

        private async void DisconnectFromScanner()
        {
            try
            {
                if (_scannerService != null)
                {
                    await _scannerService.DisconnectAsync();
                    IsConnected = false;
                    _navigationService.UpdateConnectionStatus(false);
                    _navigationService.UpdateStatusMessage("Отключен от сканера");
                }
            }
            catch (Exception ex)
            {
                _navigationService.UpdateStatusMessage($"Ошибка отключения: {ex.Message}");
            }
        }

        private void LoadExpectedStock()
        {
            _navigationService.UpdateStatusMessage("Загрузка ожидаемого инвентаря...");
            // Implement file loading logic here
        }

        private void AddTestExpectedData()
        {
            ExpectedTags.Clear();
            for (int i = 1; i <= 10; i++)
            {
                ExpectedTags.Add(new TagInfo($"PC{i:D4}", $"E200001{i:D8}", "0"));
            }
            ExpectedCount = ExpectedTags.Count;
            _navigationService.UpdateStatusMessage($"Добавлено {ExpectedCount} тестовых ожидаемых меток");
        }

        private async void PerformScan()
        {
            try
            {
                if (_scannerService != null)
                {
                    _navigationService.UpdateStatusMessage("Сканирование...");
                    var tags = await _scannerService.UpdateScannedTagsAsync();
                    
                    ScannedTags.Clear();
                    foreach (var tag in tags)
                    {
                        ScannedTags.Add(tag);
                    }
                    
                    _navigationService.UpdateStatusMessage($"Сканирование завершено: найдено {ScannedTags.Count} меток");
                }
            }
            catch (Exception ex)
            {
                _navigationService.UpdateStatusMessage($"Ошибка сканирования: {ex.Message}");
            }
        }

        private void CompareStock()
        {
            MissingTags.Clear();
            ExtraTags.Clear();
            FoundTags.Clear();

            // Find missing tags (expected but not scanned)
            foreach (var expectedTag in ExpectedTags)
            {
                if (!ScannedTags.Any(s => s.EPC.Equals(expectedTag.EPC, StringComparison.OrdinalIgnoreCase)))
                {
                    MissingTags.Add(expectedTag);
                }
            }

            // Find extra tags (scanned but not expected)
            foreach (var scannedTag in ScannedTags)
            {
                if (!ExpectedTags.Any(e => e.EPC.Equals(scannedTag.EPC, StringComparison.OrdinalIgnoreCase)))
                {
                    ExtraTags.Add(scannedTag);
                }
                else
                {
                    // This is a found tag (scanned and expected)
                    FoundTags.Add(scannedTag);
                }
            }

            // Update counts
            FoundCount = FoundTags.Count;
            MissingCount = MissingTags.Count;

            _navigationService.UpdateStatusMessage($"Сравнение завершено: найдено {FoundCount}, недостает {MissingCount}, лишних {ExtraTags.Count}");
        }

        private async void ExportResults()
        {
            try
            {
                if (_exportService != null)
                {
                    var allTags = ScannedTags.Concat(MissingTags).Concat(ExtraTags);
                    await _exportService.ExportAsync(allTags, $"StockTake_Results_{DateTime.Now:yyyyMMddHHmmss}.json");
                    _navigationService.UpdateStatusMessage("Результаты экспортированы");
                }
            }
            catch (Exception ex)
            {
                _navigationService.UpdateStatusMessage($"Ошибка экспорта: {ex.Message}");
            }
        }

        private void ClearAll()
        {
            ExpectedTags.Clear();
            ScannedTags.Clear();
            MissingTags.Clear();
            ExtraTags.Clear();
            FoundTags.Clear();
            
            ExpectedCount = 0;
            FoundCount = 0;
            MissingCount = 0;
            
            CurrentView = "Missing"; // Reset to Missing view (default)
            
            _navigationService.UpdateStatusMessage("Все данные очищены");
        }

        // Status button command implementations - now toggle views
        private void ShowExpectedTags()
        {
            if (CurrentView == "Expected")
            {
                CurrentView = "Missing"; // Toggle off - return to Missing view (default)
                _navigationService.UpdateStatusMessage("Показаны недостающие метки");
            }
            else
            {
                CurrentView = "Expected";
                _navigationService.UpdateStatusMessage($"Показаны ожидаемые метки: {ExpectedCount} шт.");
            }
        }

        private void ShowFoundTags()
        {
            if (CurrentView == "Found")
            {
                CurrentView = "Missing"; // Toggle off - return to Missing view (default)
                _navigationService.UpdateStatusMessage("Показаны недостающие метки");
            }
            else
            {
                CurrentView = "Found";
                _navigationService.UpdateStatusMessage($"Показаны найденные метки: {FoundCount} шт.");
            }
        }

        private void ShowMissingTags()
        {
            if (CurrentView == "Missing")
            {
                CurrentView = "Missing"; // Stay on Missing view (don't toggle off)
                _navigationService.UpdateStatusMessage($"Показаны недостающие метки: {MissingCount} шт.");
            }
            else
            {
                CurrentView = "Missing";
                _navigationService.UpdateStatusMessage($"Показаны недостающие метки: {MissingCount} шт.");
            }
        }

        private void ShowExtraTags()
        {
            if (CurrentView == "Extra")
            {
                CurrentView = "Missing"; // Toggle off - return to Missing view (default)
                _navigationService.UpdateStatusMessage("Показаны недостающие метки");
            }
            else
            {
                CurrentView = "Extra";
                _navigationService.UpdateStatusMessage($"Показаны лишние метки: {ExtraTags.Count} шт.");
            }
        }

        public override void OnNavigatedTo()
        {
            base.OnNavigatedTo();
            _navigationService.UpdateStatusMessage("Страница инвентаризации загружена");
        }

        public override void OnNavigatedFrom()
        {
            base.OnNavigatedFrom();
        }
    }
}