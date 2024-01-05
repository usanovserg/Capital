using Capital.Entity;
using Capital.Enums;
using System.ComponentModel;
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
        #region Fields =============================================================
        List<StrategyType> strategies = new List<StrategyType>()
        {
            StrategyType.FIX,
            StrategyType.CAPITALIZATION,
            StrategyType.PROGRESS,
            StrategyType.DOWNGRADE
        };

        Random random = new Random();
        #endregion



        #region Properties =========================================================

        #endregion




        #region Methods =============================================================
        private void Init() 
        {
            _comboBoxx.ItemsSource = strategies;
            _comboBoxx.SelectionChanged += ComboBoxx_SelectionChanged;
            _comboBoxx.SelectedIndex = 0;

            _deposit.Text = "100000";
            _initLot.Text = "10";
            _take.Text = "300";
            _stop.Text = "100";
            _commis.Text = "5";
            _winrate.Text = "30";
            _countTrades.Text = "1000";
            _minStartPercent.Text = "20";
            _go.Text = "5000";


        }

        private void ComboBoxx_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox? comboBox = sender as ComboBox;
            int index = comboBox != null ? comboBox.SelectedIndex:-1;
        }
        #endregion


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Calculate();
        }

        private void Calculate() 
        { 
            decimal deposit = ConvertStringToDecimal(_deposit.Text);
            int initLot = ConvertStringToInt(_initLot.Text);
            decimal take = ConvertStringToDecimal(_take.Text);
            decimal stop = ConvertStringToDecimal(_stop.Text);
            decimal commis = ConvertStringToDecimal(_commis.Text);
            int winrate = ConvertStringToInt(_winrate.Text);
            int countTrades = ConvertStringToInt(_countTrades.Text);
            int minStartPercent = ConvertStringToInt(_minStartPercent.Text);
            decimal go = ConvertStringToDecimal(_go.Text);

            List<Data> datas = new List<Data>();

            foreach (StrategyType type in strategies)
            { 
                datas.Add(new Data(type, deposit));  
            }

            int lotPercent = initLot;
            decimal percent = initLot * go * 100/ deposit;
            decimal multiply = take / stop;
            int lotProgress = CalculateLot(deposit, minStartPercent, go);
            int lotDown = initLot;

            for (int i = 0; i < countTrades; i++)
            { 
                int rnd = random.Next(1,100);
                if (rnd <= winrate)
                { //Сделка прибыльная
                    // FIX - стратегия
                    datas[0].ResultDepo += (take - commis) * initLot;
                    // CAPITALIZATION - стратегия
                    datas[1].ResultDepo += (take - commis) * lotPercent;
                    int newLot = CalculateLot(datas[1].ResultDepo, percent, go);
                    if (lotPercent < newLot) lotPercent = newLot;
                    // PROGRESS - стратегия
                    datas[2].ResultDepo += (take - commis) * lotProgress;
                    lotProgress = CalculateLot(deposit, minStartPercent*multiply, go);
                    // DOWNGRADE - стратегия
                    datas[3].ResultDepo += (take - commis) * lotDown;
                    lotDown = initLot;
                }
                else 
                { //Сделка убыточная
                    // FIX - стратегия
                    datas[0].ResultDepo -= (stop + commis) * initLot;
                    // CAPITALIZATION - стратегия
                    datas[1].ResultDepo -= (stop + commis) * lotPercent;

                    // PROGRESS - стратегия
                    datas[2].ResultDepo -= (stop + commis) * lotProgress;
                    lotProgress = CalculateLot(deposit, minStartPercent, go);
                    // DOWNGRADE - стратегия
                    datas[3].ResultDepo -= (stop + commis) * lotDown;
                    lotDown /= 2;
                    if (lotDown == 0) lotDown = 1;
                }
            }

            _datagrid.ItemsSource = datas;
        }

        private int CalculateLot(decimal currentDepo, decimal percent, decimal go)
        {
            if (percent > 100) percent = 100;
            decimal lot = currentDepo / go / 100 * percent;
            return (int)lot;
        }

        private decimal ConvertStringToDecimal(string str)
        {
            if (decimal.TryParse(str, out decimal result)) return result;
            return 0;
        }
        private int ConvertStringToInt(string str)
        {
            if (int.TryParse(str, out int result)) return result;
            return 0;
        }
    }
}