using System.Dynamic;
using System.Threading.Tasks;

namespace mPlanet.Services.Interfaces
{
    public interface IApiService
    {
        Task StartAsync(int port = 8080);

        Task StopAsync();

        bool isRunning { get; }
    }
}
