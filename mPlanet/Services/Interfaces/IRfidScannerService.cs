using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using mPlanet.Models;

namespace mPlanet.Services.Interfaces
{
    public interface IRfidScannerService
    {
        Task<bool> ConnectAsync(string comPort);

        Task<IEnumerable<TagInfo>> UpdateScannedTagsAsync();

        Task DisconnectAsync();

        bool IsConnected { get; }

        event EventHandler<string> StatusChanged;
    }
}
