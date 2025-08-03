using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Threading.Tasks;
using ThingMagic;

namespace mPlanet.Configuration
{
    public static class AppSettings
    {
        public const int DefaultApiPort = 8080;
        public const string DefaultExportPath = ".\\exports";
        public const bool AutoOpenExportFolder = false;
        public const bool StartWithWindows = false;
        public const bool MinimizeToTray = false;
        public const string DefaultLanguage = "Русский";

        public static class Scanner
        {
            public const int DefaultPower = 30;
            public const int DefaultFrequency = 400;
            public const int DefaultSensitivity = 99;
        }
    }
}
