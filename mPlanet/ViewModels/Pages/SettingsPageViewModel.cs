using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mPlanet.Infrastructure;
using mPlanet.Services.Interfaces;
using mPlanet.Configuration;
using System.Windows.Input;

namespace mPlanet.ViewModels.Pages
{
    public class SettingsPageViewModel : PageViewModelBase
    {
        private string _scannerModel = "HW6";
        private int _comPort = 6;
        private int _scannerPower = AppSettings.Scanner.DefaultPower;
        private int _scannerFrequency = AppSettings.Scanner.DefaultFrequency;
        private int _scannerSensitivity = AppSettings.Scanner.DefaultSensitivity;
        private string _exportPath = AppSettings.DefaultExportPath;
        private bool _autoOpenExportFolder = false;
        private bool _startWithWindows = false;
        private bool _minimizeToTray = false;
        private string _selectedLanguage = "Русский";
        private bool _scannerStatus = false;
        private string _eventLog = "";

        public string ScannerModel
        {
            get => _scannerModel;
            set => SetProperty(ref _scannerModel, value);
        }

        public int ComPort
        {
            get => _comPort;
            set => SetProperty(ref _comPort, value);
        }

        public int ScannerPower
        {
            get => _scannerPower;
            set => SetProperty(ref _scannerPower, value);
        }

        public int ScannerFrequency
        {
            get => _scannerFrequency;
            set => SetProperty(ref _scannerFrequency, value);
        }

        public int ScannerSensitivity
        {
            get => _scannerSensitivity;
            set => SetProperty(ref _scannerSensitivity, value);
        }

        public bool ScannerStatus
        {
            get => _scannerStatus;
            set
            {
                SetProperty(ref _scannerStatus, value);
                OnPropertyChanged(nameof(StatusIndicatorColor));
            }
        }

        public string StatusIndicatorColor => _scannerStatus ? "Green" : "Red";

        public string EventLog
        {
            get => _eventLog;
            set => SetProperty(ref _eventLog, value);
        }

        public string ExportPath
        {
            get => _exportPath;
            set => SetProperty(ref _exportPath, value);
        }

        public bool AutoOpenExportFolder
        {
            get => _autoOpenExportFolder;
            set => SetProperty(ref _autoOpenExportFolder, value);
        }

        public bool StartWithWindows
        {
            get => _startWithWindows;
            set => SetProperty(ref _startWithWindows, value);
        }

        public bool MinimizeToTray
        {
            get => _minimizeToTray;
            set => SetProperty(ref _minimizeToTray, value);
        }

        public string SelectedLanguage
        {
            get => _selectedLanguage;
            set => SetProperty(ref _selectedLanguage, value);
        }

        public List<string> AvailableLanguages { get; } = new List<string>
        {
            "Русский", "English"
        };

        public List<string> AvailableModels { get; } = new List<string>
        {
            "HW6", "HW5", "HW4"
        };

        public ICommand BrowseExportPathCommand { get; }
        public ICommand SaveSettingsCommand { get; }
        public ICommand CancelSettingsCommand { get; }
        public ICommand ResetToDefaultsCommand { get; }
        public ICommand TurnOnScannerCommand { get; }
        public ICommand TurnOffScannerCommand { get; }

        public SettingsPageViewModel(INavigationService navigationService) 
            : base(navigationService)
        {
            BrowseExportPathCommand = new RelayCommand(_ => ExecuteBrowseExportPath());
            SaveSettingsCommand = new RelayCommand(_ => ExecuteSaveSettings());
            CancelSettingsCommand = new RelayCommand(_ => ExecuteCancelSettings());
            ResetToDefaultsCommand = new RelayCommand(_ => ExecuteResetToDefaults());
            TurnOnScannerCommand = new RelayCommand(_ => ExecuteTurnOnScanner());
            TurnOffScannerCommand = new RelayCommand(_ => ExecuteTurnOffScanner());

            EventLog = "Добро пожаловать в настройки сканера\nСистема готова к работе\n";
        }

        private void ExecuteBrowseExportPath()
        {
            _navigationService.UpdateStatusMessage("Выбор папки для экспорта...");
            
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    ExportPath = dialog.SelectedPath;
                    _navigationService.UpdateStatusMessage($"Выбрана папка: {dialog.SelectedPath}");
                }
                else
                {
                    _navigationService.UpdateStatusMessage("Выбор папки отменен.");
                }
            }
        }

        private void ExecuteSaveSettings()
        {
            _navigationService.UpdateStatusMessage("Сохранение настроек...");
            
            //TODO: Implement settings saving
            // When you implement actual saving, update the status message accordingly
            
            _navigationService.UpdateStatusMessage("Настройки сохранены.");
            _navigationService.NavigateTo("MainPage");
        }

        private void ExecuteCancelSettings()
        {
            _navigationService.UpdateStatusMessage("Изменения отменены.");
            _navigationService.NavigateTo("MainPage");
        }

        private void ExecuteResetToDefaults()
        {
            _navigationService.UpdateStatusMessage("Сброс настроек к значениям по умолчанию...");
            
            ScannerModel = "HW6";
            ComPort = 6;
            ScannerPower = AppSettings.Scanner.DefaultPower;
            ScannerFrequency = AppSettings.Scanner.DefaultFrequency;
            ScannerSensitivity = AppSettings.Scanner.DefaultSensitivity;
            ExportPath = AppSettings.DefaultExportPath;
            AutoOpenExportFolder = AppSettings.AutoOpenExportFolder;
            StartWithWindows = AppSettings.StartWithWindows;
            MinimizeToTray = AppSettings.MinimizeToTray;
            SelectedLanguage = AppSettings.DefaultLanguage;
            ScannerStatus = false;
            
            AddToEventLog("Настройки сброшены к значениям по умолчанию");
            _navigationService.UpdateStatusMessage("Настройки сброшены к значениям по умолчанию.");
        }

        private void ExecuteTurnOnScanner()
        {
            ScannerStatus = true;
            AddToEventLog("Сканер включен");
            _navigationService.UpdateStatusMessage("Сканер включен");
        }

        private void ExecuteTurnOffScanner()
        {
            ScannerStatus = false;
            AddToEventLog("Сканер выключен");
            _navigationService.UpdateStatusMessage("Сканер выключен");
        }

        private void AddToEventLog(string message)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            EventLog += $"[{timestamp}] {message}\n";
        }

        public override void OnNavigatedTo()
        {
            _navigationService.UpdateStatusMessage("Страница настроек загружена");
        }

        public override void OnNavigatedFrom()
        {
            _navigationService.UpdateStatusMessage("Покинули страницу настроек");
        }
    }
}