using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for PlayerSelection.xaml
    /// </summary>
    public partial class PlayerSelection : UserControl, System.ComponentModel.INotifyPropertyChanged
    {
        public PlayerSelection()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty IsAIProperty = DependencyProperty.Register(
          "IsAI",
          typeof(bool),
          typeof(PlayerSelection)
        );

        public bool IsAI
        {
            get
            {
                return tbAI.IsSelected;
            }
            set
            {
                if (tbAI.IsSelected != value)
                {
                    tbAI.IsSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        public static readonly DependencyProperty IsHumanProperty = DependencyProperty.Register(
         "IsHuman",
         typeof(bool),
         typeof(PlayerSelection)
       );

        public bool IsHuman
        {
            get
            {
                return tbHu.IsSelected;
            }
            set
            {
                if (tbHu.IsSelected != value)
                {
                    tbHu.IsSelected = value;
                    OnPropertyChanged();
                }
            }
        }


        public static readonly DependencyProperty IsCruelProperty = DependencyProperty.Register(
          "IsCruel",
          typeof(bool),
          typeof(PlayerSelection)
        );

        public bool IsCruel
        {
            get
            {
                if (rdbCruel.IsChecked == true)
                    return true;
                else
                    return false;
            }
            set
            {
                if(rdbCruel.IsChecked != value)
                {
                    rdbCruel.IsChecked = value;
                    OnPropertyChanged();
                }
            }
        }

        public static readonly DependencyProperty IsSmartProperty = DependencyProperty.Register(
          "IsSmart",
          typeof(bool),
          typeof(PlayerSelection)
        );

        public bool IsSmart
        {
            get
            {
                if (rdbHard.IsChecked == true)
                    return true;
                else
                    return false;
            }
            set
            {
                if (rdbHard.IsChecked != value)
                {
                    rdbHard.IsChecked = value;
                    OnPropertyChanged();
                }
            }
        }

        public static readonly DependencyProperty IsConfusedProperty = DependencyProperty.Register(
          "IsConfused",
          typeof(bool),
          typeof(PlayerSelection)
        );

        public bool IsConfused
        {
            get
            {
                if (rdbEasy.IsChecked == true)
                    return true;
                else
                    return false;
            }
            set
            {
                if (rdbEasy.IsChecked != value)
                {
                    rdbEasy.IsChecked = value;
                    OnPropertyChanged();
                }
            }
        }

        public static readonly DependencyProperty DepthProperty = DependencyProperty.Register(
          "Depth",
          typeof(int),
          typeof(PlayerSelection)
        );

        public int Depth
        {
            get
            {
                return (int)sldrDepth.Value;
            }
            set
            {
                if ((int)sldrDepth.Value != value)
                {
                    sldrDepth.Value = value;
                    OnPropertyChanged();
                }
            }
        }

        public Settings Settings { get { return Globals.Settings; } }

        public Controllers.AIMinimax GetController(Game game, int player)
        {
            var controller = new Controllers.AIMinimax(game, player);

            controller.Tiebreak = new Tiebreaks.TileWeight();
            controller.Weighting = new Weighting.TieredWeightingCompressed_R2();
            controller.EndgameWeighting = new EndgameWeighting.DiskMaximizing();

            if (tbAI.IsSelected)
            {
                if (rdbCruel.IsChecked == true)
                    controller.MoveSelector = new MoveSelectors.Adaptive_V4_R8();
                else if (rdbHard.IsChecked == true)
                    controller.MoveSelector = new MoveSelectors.Best();
                else if (rdbEasy.IsChecked == true)
                    controller.MoveSelector = new MoveSelectors.Worst();
                else
                    throw new Exceptions.LogicError();
            }
            else
            {
                controller.MoveSelector = new MoveSelectors.Best();
            }

            if (tbAI.IsSelected)
                controller.Pause = false;
            else if (tbHu.IsSelected)
                controller.Pause = true;
            else
                throw new Exceptions.LogicError();

            controller.Depth = (int)sldrDepth.Value;

            return controller;
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (e == null)
                return;

            var v = (int)e.NewValue;

            lblAIDiffReadout.Content = v;
            //return;

            //if (v < 2)
            //    lblAIDiffReadout.Content = "Lowest";
            //else if (v == 2)
            //    lblAIDiffReadout.Content = "Lower";
            //else if (v == 3)
            //    lblAIDiffReadout.Content = "Low";
            //else if (v == 4)
            //    lblAIDiffReadout.Content = "Mediocre";
            //else if (v == 5)
            //    lblAIDiffReadout.Content = "Below Average";
            //else if (v == 6)
            //    lblAIDiffReadout.Content = "Recommended";
            //else if (v == 7)
            //    lblAIDiffReadout.Content = "Above Average";
            //else if (v == 8)
            //    lblAIDiffReadout.Content = "High";
            //else
            //    lblAIDiffReadout.Content = "Very High";

            if (v < 9)
                lblAIDiffPrefWarn.Visibility = System.Windows.Visibility.Hidden;
            else
                lblAIDiffPrefWarn.Visibility = System.Windows.Visibility.Visible;

        }

        private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName]string name = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source == tcMain && e.RemovedItems.Count != 0)
            {
                //OnPropertyChanged("IsAI");
                //OnPropertyChanged("IsHuman");
            }
        }
    }
}
