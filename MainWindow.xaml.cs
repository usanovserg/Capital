using Capital.Entity;
using Capital.Enams;
using System.Text;        
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Capital
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Init();
        }

        #region Fields ========================================

        List<StrategyType> _strategies = new List<StrategyType>()
        {
                StrategyType.FIX,
                StrategyType.CAPITALIZATION,
                StrategyType.PROGRESS,
                StrategyType.DOWNGRADE
        };

        Random _random = new Random();

        #endregion


        #region Methods ===========================================

        private void Init()
        {
            //_combobox.ItemsSource = new List<StrategyType>();
            _combobox.ItemsSource = _strategies ;

            _combobox.SelectionChanged += _combobox_SelectionChanged;
            _combobox.SelectedIndex = 0;

            _depo.Text = "100000";
            _startLot.Text = "10";
            _take.Text = "300";
            _stop.Text = "100";
            _comiss.Text = "5";
            _countTrades.Text = "1000";
            _percentProffit.Text = "30";
            _go.Text = "5000";
            _minStartPercent.Text = "20";

        }

        private void _combobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            
            int index = comboBox.SelectedIndex;
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Calculate();
        }


        private void Calculate()
        {

        }

        #endregion

    }
}