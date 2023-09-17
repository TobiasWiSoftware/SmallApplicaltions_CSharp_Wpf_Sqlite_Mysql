using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Printing;
using System.htmltext;
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

namespace Real_Estate_Management
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ViewModel? _model = null;
        public MainWindow()
        {
            InitializeComponent();
            Init();

        }

        private void Init()
        {

            Startup.CreateDataBase();
            _model = FindResource("vm") as ViewModel;
            if (_model != null)
                _model.Init();

        }
    }
}
