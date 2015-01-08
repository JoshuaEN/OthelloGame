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
using System.Windows.Shapes;

namespace OthelloGame
{
    /// <summary>
    /// Interaction logic for MinimaxTreeView.xaml
    /// </summary>
    public partial class MinimaxTreeView : Window
    {
        public MinimaxTreeView()
        {
            DataSet = new System.Collections.ObjectModel.ObservableCollection<Minimax.MinimaxDataSet>();
            InitializeComponent();
        }

        public System.Collections.ObjectModel.ObservableCollection<Minimax.MinimaxDataSet> DataSet { get; set; }

        public void view_tree(Minimax.MinimaxDataSet data)
        {
            DataSet.Clear();

            DataSet.Add(data);

            minimaxTree.ItemsSource = DataSet;
        }
    }
}
