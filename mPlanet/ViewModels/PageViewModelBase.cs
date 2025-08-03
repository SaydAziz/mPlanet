using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mPlanet.Infrastructure;
using mPlanet.Services.Interfaces;

namespace mPlanet.ViewModels
{
    public abstract class PageViewModelBase : ViewModelBase
    {
        protected readonly INavigationService _navigationService;

        protected PageViewModelBase(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        public virtual void OnNavigatedTo() { }
        public virtual void OnNavigatedFrom() { }
        public virtual void OnLoaded() { }
    }
}
