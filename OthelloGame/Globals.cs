using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OthelloGame
{
    /// <summary>
    /// Class to hold global variables.
    /// </summary>
    public static class Globals
    {
        // Unused
        public static int MAX { get { return 999000000; } }
        public static int MIN { get { return -999000000; } }
        public static int ZERO { get { return 0; } }

        public static int TIE { get { return 77000000; } }
        public static int WIN { get { return 88000000; } }
        public static int LOSS { get { return -88000000; } }

        public static int RNG_SEED { get { return 12345; } }

        public static string APP_PATH { get { return System.AppDomain.CurrentDomain.BaseDirectory; } }
        public static string SETTINGS_PATH { get { return APP_PATH + @"\Settings.xml"; } }

        private static Settings _settings;
        public static Settings Settings
        {
            get
            {
                if (_settings == null)
                {
                    _settings = OthelloGame.Settings.Load();
                }

                return _settings;
            }
        }
    }
}
