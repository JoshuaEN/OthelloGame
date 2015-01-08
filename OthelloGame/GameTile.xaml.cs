using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OthelloGame
{
    /// <summary>
    /// Interaction logic for GameTile.xaml
    /// </summary>
    public partial class GameTile : UserControl, System.ComponentModel.INotifyPropertyChanged
    {
        private string _headerText;
        public string HeaderText { get { return _headerText; } set { _headerText = value; NotifyPropertyChanged("HeaderText"); } }

        private string _footerText;
        public string FooterText { get { return _footerText; } set { _footerText = value; NotifyPropertyChanged("FooterText"); } }

        private int owner = -1;
        public int Owner
        {
            get
            {
                return owner;
            }
            set
            {
                owner = value;
                if (-1 == owner)
                {
                    OwnerColor = Brushes.Transparent;
                    DiskVisibility = System.Windows.Visibility.Hidden;
                }
                else if (0 == owner)
                {
                    OwnerColor = Brushes.Black;
                    DiskVisibility = System.Windows.Visibility.Visible;
                }
                else if (1 == owner)
                {
                    OwnerColor = Brushes.White;
                    DiskVisibility = System.Windows.Visibility.Visible;
                }
                else
                    throw new ArgumentException("Invalid Owner");

                NotifyPropertyChanged("Owner");
            }
        }

        private Brush _backgroundColor;
        public Brush BackgroundColor { get { return _backgroundColor; } private set { _backgroundColor = value; NotifyPropertyChanged("BackgroundColor"); } }

        private HighlightModes highlightMode = HighlightModes.None;
        public HighlightModes HighlightMode
        {
            get { return highlightMode; }
            set
            {
                highlightMode = value;
                //Background = new SolidColorBrush(Colors.Transparent);

                switch (highlightMode)
                {
                    case HighlightModes.None:
                        Background = Brushes.Transparent;
                        break;
                    case HighlightModes.ValidMove:
                        Background = new SolidColorBrush(Color.FromRgb(193, 226, 193));
                        break;
                    case HighlightModes.BestMove:
                        Background = Brushes.LightGoldenrodYellow;
                        break;
                    case HighlightModes.ChosenMove:
                        Background = Brushes.Goldenrod;
                        break;
                    case HighlightModes.LastMove:
                        {
                            var brush = new RadialGradientBrush(Colors.DodgerBlue, Colors.Transparent);
                            brush.RadiusX = .8;
                            brush.RadiusY = .8;
                            Background = brush;
                        }

                        break;
                    case HighlightModes.LastMoveEffect:
                        {
                            var brush = new RadialGradientBrush(Colors.Red, Colors.Transparent);
                            brush.RadiusX = .8;
                            brush.RadiusY = .8;
                            Background = brush;
                        }

                        break;
                    default:
                        throw new ArgumentException();
                }

                NotifyPropertyChanged("HighlightMode");
            }
        }

        private Brush _ownerColor;
        public Brush OwnerColor { get { return _ownerColor; } private set { _ownerColor = value; NotifyPropertyChanged("OwnerColor"); } }

        private Visibility _diskVisibility;
        public Visibility DiskVisibility { get { return _diskVisibility; } private set { _diskVisibility = value; NotifyPropertyChanged("DiskVisibility"); } }

        public enum HighlightModes { None, ValidMove, BestMove, ChosenMove, LastMove, LastMoveEffect }

        public GameTile()
        {
            InitializeComponent();
            DataContext = this;
        }




        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propName));
        }

    }// http://blogs.msdn.com/b/mikehillberg/archive/2006/09/26/cannotanimateimmutableobjectinstance.aspx
    internal class MyCloneConverter : IValueConverter
    {
        public static MyCloneConverter Instance = new MyCloneConverter();

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Freezable)
            {
                value = (value as Freezable).Clone();
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

    }
}
