using MegawareDLL;
using mPlanet.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using mPlanet.Models;
using System.Linq;
using mPlanet.Configuration;

namespace mPlanet.Services
{
    public class ScannerMHandService : IRfidScannerService
    {
        private MegawareMHand _scanner;
        private bool _isConnected;
        private string _currentComPort = "";

        public event EventHandler<string> StatusChanged;

        public bool IsConnected => _isConnected;
        public string CurrentComPort => _currentComPort;

        public ScannerMHandService()
        {
            _scanner = new MegawareMHand();
        }

        public async Task<bool> ConnectAsync(string comPort)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(comPort))
                {
                    OnStatusChanged("COM port cannot be empty");
                    return false;
                }

                var result = await Task.Run(() => _scanner.connect(comPort));

                _isConnected = result;
                _currentComPort = result ? comPort : "";

                if (_isConnected)
                {
                    await Task.Run(() =>
                    {
                        _scanner.setProfile(AppSettings.Scanner.DefaultPower,
                            AppSettings.Scanner.DefaultFrequency,
                            AppSettings.Scanner.DefaultSensitivity
                            );
                        _scanner.setCurrentProfileH6();
                    });
                }

                var message = result ?
                    $"Successfully connected to COM port {comPort}" :
                    $"Failed to connect to COM port {comPort}";

                OnStatusChanged(message);

                return result;
            }
            catch (Exception ex)
            {
                _isConnected = false;
                _currentComPort = "";
                OnStatusChanged($"Connection error: {ex.Message}");
                return false;
            }
        }

        public async Task<IEnumerable<TagInfo>> UpdateScannedTagsAsync()
        {
            if (!_isConnected)
            {
                OnStatusChanged("Scanner not connected");
                return Enumerable.Empty<TagInfo>();
            }

            try
            {
                var scannedTags = await Task.Run(() =>
                {
                    _scanner.retrieveEPC();
                    return _scanner.getFullInfo();
                });

                var tagList = ParseScannerData(scannedTags);

                OnStatusChanged($"Scan completed: {tagList.Count()} tags found");
                return tagList;
            }
            catch (Exception ex)
            {
                OnStatusChanged($"Error fetching scanned tags: {ex.Message}");
                return Enumerable.Empty<TagInfo>();
            }
        }

        public async Task DisconnectAsync()
        {
            try
            {
                await Task.Run(() =>
                {
                    _scanner?.disconnect();
                    _scanner = null;
                    _scanner = new MegawareMHand();
                });

                _isConnected = false;
                _currentComPort = "";
                OnStatusChanged("Disconnected from scanner");
            }
            catch (Exception ex)
            {
                OnStatusChanged($"Disconnect error: {ex.Message}");
                _isConnected = false;
                _currentComPort = "";
            }
        }

        private IEnumerable<TagInfo> ParseScannerData(string[] rawData)
        {
            var tags = new List<TagInfo>();

            if (rawData == null) return tags;

            foreach (string tagData in rawData)
            {
                if (string.IsNullOrWhiteSpace(tagData)) continue;

                try
                {
                    string[] tagParts= tagData.Split('-', ',');

                    if (tagParts.Length >= 3)
                    {
                        var tag = new TagInfo(
                            pc: tagParts[0].Trim(),
                            epc: tagParts[1].Trim(),
                            rssi: tagParts[2].Trim()
                        );

                        tags.Add(tag);
                    }
                }
                catch (Exception ex)
                {
                    OnStatusChanged($"Error parsing tag data '{tagData}': {ex.Message}");
                }
            }
            return tags;
        }

        private void OnStatusChanged(string message)
        {
            StatusChanged?.Invoke(this, message);
        }

        public void Dispose()
        {
            DisconnectAsync().GetAwaiter().GetResult();
        }
    }
}