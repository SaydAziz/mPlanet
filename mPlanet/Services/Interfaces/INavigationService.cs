using System;

namespace mPlanet.Services.Interfaces
{
    public interface INavigationService
    {
        void NavigateTo(string pageName);
        void NavigateTo(Type pageType);
        bool CanGoBack { get; }
        bool CanGoForward { get; }
        void GoBack();
        void GoForward();
        
        // Navigation events
        event EventHandler<string> NavigationChanged;
        
        // Status message broadcasting
        event EventHandler<string> StatusMessageChanged;
        void UpdateStatusMessage(string message);
        
        // Connection status broadcasting
        event EventHandler<(bool isConnected, string comPort)> ConnectionStatusChanged;
        void UpdateConnectionStatus(bool isConnected, string comPort = "");
    }
}