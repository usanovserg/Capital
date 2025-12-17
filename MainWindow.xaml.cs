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
using Capital.Entity;
using Capital.Enums;

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

        #region Fields ===================================================================

        List<StrategyType> _strategies = new List<StrategyType>() 
        {
                StrategyType.FIX,
                StrategyType.CAPITALIZATION,
                StrategyType.PROGRESS,
                StrategyType.DOWNGRADE
        };

        Random _random = new Random();

        #endregion

        #region Methods ===================================================================

        private void Init()
        {
            _comboBox.ItemsSource = _strategies;
           
           /// <summary>
           /// List<StrategyType> strategyTypes = new List<StrategyType>();
            
            ///    strategyTypes.Add(StrategyType.FIX);
            ///  strategyTypes.Add(StrategyType.CAPITALIZATION);
            ///strategyTypes.Add(StrategyType.PROGRESS);
            ///strategyTypes.Add(StrategyType.DOWNGRADE);

            ///_comboBox.ItemsSource = strategyTypes;
            /// </summary>
             _comboBox.SelectionChanged += _comboBox_SelectionChanged;
            _comboBox.SelectedIndex = 0;

            _depo.Text = "100000";
            _startLot.Text = "10";
            _take.Text = "300";
            _stop.Text = "100";
            _comiss.Text = "5";
            _percentProfit.Text = "30";
            _countTrades.Text = "1000";
            _minStartPercent.Text = "20";
            _go.Text = "5000";            
        }
        private void _comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox) sender;

            int index = comboBox.SelectedIndex;

        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Calculate();

        }

        private void Calculate()
        {
            decimal depoStart = GetDecimalFromString(_depo.Text);
            int startLot = GetIntFromString(_startLot.Text);
            decimal take = GetDecimalFromString(_take.Text);
            decimal stop = GetDecimalFromString(_stop.Text);
            decimal comiss = GetDecimalFromString(_comiss.Text);
            decimal percentProfit = GetDecimalFromString(_percentProfit.Text);
            int countTrades = GetIntFromString(_countTrades.Text);
            decimal minStartPercent = GetDecimalFromString(_minStartPercent.Text);
            decimal go = GetDecimalFromString(_go.Text);

            List<Data> datas = new List<Data>();


            foreach (StrategyType type in _strategies)
            {
                datas.Add(new Data(depoStart, type));
            }

            int lotPersent = startLot;
            decimal percent = startLot* go*100/ depoStart;
            decimal multiply = take / stop;
            int lotProgress = CalculateLot(depoStart, minStartPercent, go);

            int lotDown = startLot;

            for (int i = 0; i < countTrades; i++)
            {
                int rnd = _random.Next(1, 100);
                if (rnd <= percentProfit)
                {
                    // Сделка прибыльная

                    // ============= 1 stratage ===============================
                    datas[0].ResultDepo += (take - comiss) * startLot;

                    // ============= 2 stratage ===============================

                    datas[1].ResultDepo += (take - comiss) * lotPersent;
           
                    int newLot = CalculateLot(datas[1].ResultDepo, percent, go);

                    if (lotPersent<newLot) lotPersent= newLot;

                    // ============= 3 stratage ===============================
                    datas[2].ResultDepo += (take - comiss) * lotProgress;
                    lotProgress = CalculateLot(depoStart, minStartPercent * multiply, go);

                    // ============= 4 stratage ===============================
                    datas[3].ResultDepo += (take - comiss) * lotDown;

                    lotDown = startLot;

                }
                else
                {
                    // Сделка убыточная
                    // ============= 1 stratage ===============================
                    datas[0].ResultDepo -= (stop + comiss) * startLot;
                    // ============= 2 stratage ===============================
                    datas[1].ResultDepo -= (stop + comiss) * lotPersent;

                    // ============= 3 stratage ===============================
                    datas[2].ResultDepo -= (stop + comiss) * lotProgress;
                    lotProgress = CalculateLot(depoStart, minStartPercent, go);

                    // ============= 4 stratage ===============================
                    datas[3].ResultDepo -= (stop + comiss) * lotDown;
                    lotDown /= 2;
                    if (lotDown == 0) lotDown = 1;

                }

            }
            _dataGrid.ItemsSource = datas;
        }

        private int CalculateLot(decimal currentDepo,decimal percent, decimal go) 
        { 
            if (percent > 100) { percent = 100; }
            decimal lot = currentDepo/go/100*percent;
            return (int)lot;
        
        }
            //decimal.TryParse(_depo.Text, out depoStart);
             

        private decimal GetDecimalFromString(string str) 
        {
            if (decimal.TryParse(str,out decimal result)) return result;

            return 0;       
        }
        private int GetIntFromString(string str)
        {
            if (int.TryParse(str, out int result)) return result;

            return 0;
        }



        #endregion

    }
}