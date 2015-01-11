using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;

namespace OthelloGame
{
    /// <summary>
    /// Class for user modifiable settings.
    /// </summary>
    public class Settings : INotifyPropertyChanged
    {
        private bool _useSavedControllers = false;
        /// <summary>
        /// Should the saved controller data be used to automatically start a new game upon startup?
        /// </summary>
        public bool UseSavedControllers 
        {
            get { return _useSavedControllers; }
            set
            {
                if (_useSavedControllers != value)
                {
                    _useSavedControllers = value;
                    OnPropertyChanged();
                }
            }
        }

        private GameRender.TileInfoLevels _tileInfoLevel = GameRender.TileInfoLevels.Full;
        /// <summary>
        /// How much information should be drawn on tiles.
        /// </summary>
        public GameRender.TileInfoLevels TileInfoLevel
        {
            get { return _tileInfoLevel; }
            set
            {
                if(_tileInfoLevel != value)
                {
                    _tileInfoLevel = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _drawControllerData = true;
        /// <summary>
        /// Should the controller be asked to draw tile data; also controls if the controller draws debug info.
        /// </summary>
        public bool DrawControllerData
        {
            get { return _drawControllerData; }
            set
            {
                if(_drawControllerData != value)
                {
                    _drawControllerData = value;
                    OnPropertyChanged();
                }
            }
        }

        #region Controller and Depth selectors for Players 1 and 2

        // This is so ugly, but it works and I don't have the time to mess with trying to get WPF databindings to work.

        private bool _player1IsCruel = true;
        public bool Player1IsCruel
        {
            get { return _player1IsCruel; }
            set
            {
                if(_player1IsCruel != value)
                {
                    _player1IsCruel = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _player1IsSmart = false;
        public bool Player1IsSmart
        {
            get { return _player1IsSmart; }
            set
            {
                if (_player1IsSmart != value)
                {
                    _player1IsSmart = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _player1IsConfused = false;
        public bool Player1IsConfused
        {
            get { return _player1IsConfused; }
            set
            {
                if (_player1IsConfused != value)
                {
                    _player1IsConfused = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _player1Depth = 6;
        public int Player1Depth
        {
            get { return _player1Depth; }
            set
            {
                if (_player1Depth != value)
                {
                    _player1Depth = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _player1IsAI = true;
        public bool Player1IsAI
        {
            get { return _player1IsAI; }
            set
            {
                if (_player1IsAI != value)
                {
                    _player1IsAI = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _player1IsHuman = false;
        public bool Player1IsHuman
        {
            get { return _player1IsHuman; }
            set
            {
                if (_player1IsHuman != value)
                {
                    _player1IsHuman = value;
                    OnPropertyChanged();
                }
            }
        }


        private bool _player2IsCruel = true;
        public bool Player2IsCruel
        {
            get { return _player2IsCruel; }
            set
            {
                if (_player2IsCruel != value)
                {
                    _player2IsCruel = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _player2IsSmart = false;
        public bool Player2IsSmart
        {
            get { return _player2IsSmart; }
            set
            {
                if (_player2IsSmart != value)
                {
                    _player2IsSmart = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _player2IsConfused = false;
        public bool Player2IsConfused
        {
            get { return _player2IsConfused; }
            set
            {
                if (_player2IsConfused != value)
                {
                    _player2IsConfused = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _player2Depth = 6;
        public int Player2Depth
        {
            get { return _player2Depth; }
            set
            {
                if (_player2Depth != value)
                {
                    _player2Depth = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _player2IsAI = false;
        public bool Player2IsAI
        {
            get { return _player2IsAI; }
            set
            {
                if (_player2IsAI != value)
                {
                    _player2IsAI = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _player2IsHuman = true;
        public bool Player2IsHuman
        {
            get { return _player2IsHuman; }
            set
            {
                if (_player2IsHuman != value)
                {
                    _player2IsHuman = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        /// <summary>
        /// Loads the settings from a file.
        /// </summary>
        /// <returns>The loaded settings, or the default settings if there was an error.</returns>
        public static Settings Load()
        {
            var path = Globals.SETTINGS_PATH;

            if(File.Exists(path) == false)
                return new Settings();

            try
            {
                var xs = new XmlSerializer(typeof(Settings));
                using (var sr = new StreamReader(path))
                {
                    return (Settings)xs.Deserialize(sr);
                }
            }
            // This could still happen due to race conditions and what not.
            catch (System.IO.FileNotFoundException ex)
            {
                return new Settings();
            }
            catch (System.IO.IOException ex)
            {
                hasErrored = true;
                MessageBox.Show("Generic File Error while attempting to load Settings:\n\n" + ex.Message + "\n\nUsing default settings.");
                return new Settings();
            }
            catch (Exception ex)
            {
                hasErrored = true;
                MessageBox.Show("Generic Error while attempting to load Settings:\n\n" + ex.Message + "\n\nUsing default settings.");
                return new Settings();
            }
        }

        /// <summary>
        /// Used to track if an error occurred while trying to load the settings.
        /// </summary>
        private static bool hasErrored;

        /// <summary>
        /// Saves the current settings (Globals.Settings) to a file.
        /// </summary>
        public void Save()
        {
            if(hasErrored && MessageBox.Show("An error occurred when attempting to load the Settings file which caused the default settings to be used.\nWould you still like to save the current settings?", "Save Confirmation", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning) != MessageBoxResult.Yes) {
                return;
            }

            hasErrored = false;

            var path = Globals.SETTINGS_PATH;
            var xs = new XmlSerializer(typeof(Settings));
            try
            {
                using (var sw = new StreamWriter(path))
                {
                    xs.Serialize(sw, this);
                }
            }
            catch (System.IO.IOException ex)
            {
                MessageBox.Show("Generic File Error while attempting to save Settings:\n\n" + ex.Message + "\n\nSettings were not saved.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Generic Error while attempting to save Settings:\n\n" + ex.Message + "\n\nSettings were not saved.");
            }
        }


        private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName]string name = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
