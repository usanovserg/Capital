using Capital.Entity;
using Capital.Enums;
using System.Collections;
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
        Random _random = new Random();


        #endregion


        #region Methods ===========================================

        private void Init()
        {
            _comboBox.ItemsSource = Enum.GetValues<StrategyType>();
            _comboBox.SelectedIndex = 0;
            _comboBox.SelectionChanged += (sender, e) => Calculate();
            _depo.Text = "100000";
            _startLot.Text = "10";
            _take.Text = "300";
            _stop.Text = "100";
            _comiss.Text = "5";
            _percentProfit.Text = "30";
            _countTrades.Text = "1000";
            _minDepoPercent.Text = "20";
            _go.Text = "5000";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Calculate();
        }

        private void Calculate()
        {
            var startDepo = TryParseDecimal(_depo);
            var startLot = TryParseInt(_startLot);
            var takeProfit = TryParseDecimal(_take);
            var stopLoss = TryParseDecimal(_stop);
            var comission = TryParseDecimal(_comiss);
            var countTrades = TryParseInt(_countTrades);
            var percentProfit = TryParseDecimal(_percentProfit);
            var percentMinDepo = TryParseDecimal(_minDepoPercent);
            var go = TryParseDecimal(_go);

            var data = new List<StrategyData>();
            foreach (var type in Enum.GetValues<StrategyType>())
            {
                data.Add(new StrategyData(type, startDepo));
            }

            var capitalizedLot = startLot;
            var percentCapitalizedLot = startLot * go * 100 / startDepo;
            var multiply = takeProfit / stopLoss;
            var progressedLot = CalculateMaxLot(startDepo, percentMinDepo, go);
            var dawngradeLot = startLot;

            for (var i = 0; i < countTrades; i++)
            {
                if (_random.Next(0, 100) > percentProfit)
                {
                    // loss
                    data[0].ResultDepo -= (stopLoss + comission) * startLot;

                    data[1].ResultDepo -= (stopLoss + comission) * capitalizedLot;

                    data[2].ResultDepo -= (stopLoss + comission) * progressedLot;
                    progressedLot = CalculateMaxLot(startDepo, percentMinDepo, go);

                    data[3].ResultDepo -= (stopLoss + comission) * dawngradeLot;
                    dawngradeLot /= 2;

                }
                else
                {
                    // profit
                    data[0].ResultDepo += (takeProfit - comission) * startLot;

                    data[1].ResultDepo += (takeProfit - comission) * capitalizedLot;
                    var newLot = CalculateMaxLot(data[1].ResultDepo, percentCapitalizedLot, go);
                    if (capitalizedLot < newLot) capitalizedLot = newLot;

                    data[2].ResultDepo += (takeProfit - comission) * progressedLot;
                    progressedLot = CalculateMaxLot(data[2].ResultDepo, percentMinDepo * multiply, go);

                    data[3].ResultDepo += (takeProfit - comission) * dawngradeLot;
                    dawngradeLot = startLot;
                }
            }

            _dataGrid.ItemsSource = data;
        }

        private int CalculateMaxLot(decimal currentDepo, decimal percentMaxLot, decimal go)
        {
            if (percentMaxLot > 100) percentMaxLot = 100;
            var maxLot = currentDepo / 100 / go * percentMaxLot;
            return (int)maxLot;
        }

        private decimal TryParseDecimal(TextBox textBox)
        {
            if (decimal.TryParse(textBox.Text, out decimal value))
                return value;
            return 0;
        }

        private int TryParseInt(TextBox textBox)
        {
            if (int.TryParse(textBox.Text, out int value))
                return value;
            return 0;
        }

        #endregion
    }
}