using Capital.Entity;
using Capital.Enums;
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

        Random _random =new Random();

        #endregion

        #region Methods ===========================================

        private void Init()
        {
            _comBox.ItemsSource = _strategies;
            _comBox.SelectionChanged += _comBox_SelectionChanged;
            _comBox.SelectedIndex = 0;
            _depo.Text = "100000";
            _startLot.Text = "10";
            _take.Text= "300";
            _stop.Text= "100";   
            _comiss.Text= "5";
            _countTrades.Text = "1000";
            _percentProfit.Text = "30";
            _go.Text= "5000";
            _minStartPercent.Text = "20";
        }

        private void _comBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
            decimal depoStart = GetDecimalFromString(_depo.Text);
            int startLot = GetIntFromString(_startLot.Text);
            decimal take = GetDecimalFromString(_take.Text);
            decimal stop = GetDecimalFromString(_stop.Text);
            decimal comiss = GetDecimalFromString(_comiss.Text);
            int countTrades = GetIntFromString(_countTrades.Text);
            decimal percProfit = GetDecimalFromString(_percentProfit.Text);
            decimal minStartPercent = GetDecimalFromString(_minStartPercent.Text);
            decimal go = GetDecimalFromString(_go.Text);

            List<Data> datas = new List<Data>();
            List<decimal> maxDrawDown = new List<decimal>();
            List<decimal> maxResultDepo = new List<decimal>();
            List<decimal> percentDrawDown = new List<decimal>();

            foreach (StrategyType type in _strategies)
            {
                datas.Add(new Data(depoStart, type));
                maxDrawDown.Add(decimal.MaxValue);
                maxResultDepo.Add(decimal.MinValue);
                percentDrawDown.Add(0);
            }

            int lotPercent = startLot;
            decimal percent = startLot * go * 100 / depoStart;

            decimal multiply = take / stop;

            int lotProgress = CalculateLot(depoStart, minStartPercent, go);
            int lotDown = startLot;

            for (int i=0; i<countTrades;i++)
            {
                int rnd = _random.Next(1,100);
                if (rnd <= percProfit)
                { //Сделка прибыльная
                    // ==============1 startegy===========================
                    datas[0].ResultDepo += (take - comiss) * startLot;

                    // ==============2 startegy===========================
                    datas[1].ResultDepo+= (take - comiss) * lotPercent;
                    int newLot = CalculateLot(datas[1].ResultDepo,percent,go);
                    if(lotPercent < newLot) lotPercent = newLot;

                    // ==============3 startegy===========================
                    datas[2].ResultDepo += (take - comiss) * lotProgress;
                    lotProgress = CalculateLot(depoStart,minStartPercent*multiply,go);

                    // ==============4 startegy===========================
                    datas[3].ResultDepo += (take - comiss) * lotDown;
                    lotDown = startLot;
                }
                else 
                { //Сделка убыточная
                    // ==============1 startegy===========================
                    datas[0].ResultDepo -= (stop + comiss) * startLot;
                    
                    // ==============2 startegy===========================
                    datas[1].ResultDepo -= (stop + comiss) * lotPercent;

                    // ==============3 startegy===========================
                    datas[2].ResultDepo -= (stop + comiss) * lotProgress;
                    lotProgress = CalculateLot(depoStart, minStartPercent, go);

                    // ==============4 startegy===========================
                    datas[3].ResultDepo -= (stop + comiss) * lotDown;
                    lotDown /= 2;

                    if (lotDown == 0) lotDown = 1;
                }

                for(int k=0;k<datas.Count;k++) 
                { 
                    maxResultDepo[k] = Math.Max(maxResultDepo[k], datas[k].ResultDepo);
                    maxDrawDown[k] = Math.Min(maxDrawDown[k],  datas[k].ResultDepo-maxResultDepo[k]);
                    percentDrawDown[k] = maxDrawDown[k] / maxResultDepo[k] * 100;               
                }

            }



            int j = 0;
            foreach (var data in datas) 
            {
                data.Profit = data.ResultDepo - data.Depo;
                data.PercentProfit = Math.Round(data.Profit/data.Depo*100,2);
                data.MaxDrawDown = Math.Round(maxDrawDown[j],2);
                data.PercentDrawDown = Math.Round(percentDrawDown[j],2);
                j++;
            }

            _dataGrid.ItemsSource= datas;
            var trt = _dataGrid;
        }


        private int CalculateLot(decimal currentDepo, decimal percent, decimal go)
        {
            if(percent>100) { percent = 100; }

            decimal lot = currentDepo / go / 100 * percent;

            return (int)lot;
        }
        private decimal GetDecimalFromString(string str)
        {
            if(decimal.TryParse(str, out decimal result)) return result;
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