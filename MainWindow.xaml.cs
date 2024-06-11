using Capital.Enums;
using System.ComponentModel;
using System.Globalization;
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

        #region Fields ==============================================
        private List<Strategy> _strategies = [];
        #endregion

        #region Methods =============================================
        private void Init()
        {
            _combobox.ItemsSource = new List<StrategyType>()
            {
                StrategyType.FIX,
                StrategyType.CAPITALIZATION,
                StrategyType.PROGRESS,
                StrategyType.DOWNGRADE
            };
            _combobox.SelectedIndex = 0;

            _depot.Text = "100000";
            _startLot.Text = "10";
            _take.Text = "300";
            _stop.Text = "100";
            _comiss.Text = "5";
            _countTrades.Text = "1000";
            _percentGoodDeals.Text = "30";
            _go.Text = "5000";
            _minStartPercent.Text = "20";
        }
        #endregion

        private void _combobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                int index = comboBox.SelectedIndex;
                Enums.StrategyType strategyType = (Enums.StrategyType)comboBox.SelectedItem;
                int a = 1;
            }
        }
        private void _button_Click(object sender, RoutedEventArgs e)
        {
            Calculate();
        }

        public bool GetParam(string name, out decimal param)
        {
            bool bResult = false;

            var control = FindName(name) as TextBox;
            if (control == null)
            {
                param = default;
                MessageBox.Show($"Error: cannot find name '{name}' !");
            }
            else
            {
                string text = control.Text.Replace(',', '.');
                bResult = decimal.TryParse(text, CultureInfo.InvariantCulture, out param);
                if (!bResult)
                    MessageBox.Show($"Error: cannot parse field '{name}' !");
            }
            return bResult;
        }
        public bool GetParam(string name, out int param)
        {
            bool bResult = GetParam(name, out decimal dparam);
            param = (int)dparam;
            return bResult;
        }

        private void Calculate()
        {
            Params _p;

            if (!GetParam(_depot.Name, out _p.Depot)
                || !GetParam(_startLot.Name, out _p.StartLot)
                || !GetParam(_take.Name, out _p.Take)
                || !GetParam(_stop.Name, out _p.Stop)
                || !GetParam(_comiss.Name, out _p.Comiss)
                || !GetParam(_percentGoodDeals.Name, out _p.PercentGoodDeals)
                || !GetParam(_countTrades.Name, out _p.CountTrades)
                || !GetParam(_minStartPercent.Name, out _p.MinStartPercent)
                || !GetParam(_go.Name, out _p.Go))
                return;

            if (_combobox.ItemsSource is not List<StrategyType> types)
                return;

            _strategies = types.Select(type => Strategy.CreateStrategy(_p, type)).ToList();

            // list of deals, shared among all strategies
            // true: good deal | false: bad deal
            List<bool> deals = new(_p.CountTrades);

            Random rnd = new Random();
            for (int i = 0; i < _p.CountTrades; ++i)
                deals.Add(rnd.Next(0, 100) < _p.PercentGoodDeals);

            foreach (var strategy in _strategies)
                strategy.Calculate(deals);

            _dataGrid.ItemsSource = _strategies;
        }
    }
}
