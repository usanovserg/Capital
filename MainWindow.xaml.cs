using Capital.Entity;
using Capital.Enums;
using System;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;

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

        List<Data> datas = new List<Data>();

        int g = 0;

        #endregion

        #region Methods ===========================================

        private void Init()
        {
            _comboBox.ItemsSource = _strategies;

            _comboBox.SelectionChanged += _comboBox_SelectionChanged;
            _comboBox.SelectedIndex = 0;

            _depo.Text = "10000";
            _startLot.Text = "10";
            _take.Text = "300";
            _stop.Text = "100";
            _comiss.Text = "5";
            _countTrades.Text = "1000";
            _percentProfit.Text = "30";
            _go.Text = "5000";
            _minStartPercent.Text = "20";
        }

        private void _comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;

            int index = comboBox.SelectedIndex;

             if ( g == 1 ) {Draw(datas, index);}

            return;

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
          datas = Calculate();

           Draw(datas, 0);
        }

        private List<Data> Calculate()
        {
            decimal depoStart = GetDecimalFromString(_depo.Text);
            int startLot = GetIntFromString(_startLot.Text);
            decimal take = GetDecimalFromString(_take.Text);
            decimal stop = GetDecimalFromString(_stop.Text);
            decimal comiss = GetDecimalFromString(_comiss.Text);
            int countTrades = GetIntFromString(_countTrades.Text);
            decimal percentProfit = GetDecimalFromString(_percentProfit.Text);
            decimal minStartPercent = GetDecimalFromString(_minStartPercent.Text);
            decimal go = GetDecimalFromString(_go.Text);

            List<Data> datas = new List<Data>();

            foreach (StrategyType type in _strategies)
            {
                datas.Add(new Data(depoStart, type));
            }

            int lotpercent = startLot;
            decimal percent = startLot * go * 100 / depoStart;

            decimal multyply = take / stop;
            int lotProgress = CalculateLot(depoStart, minStartPercent, go);

            int lotDown = startLot;

            for (int i = 0; i < countTrades; i++)
            {
                int rnd = _random.Next(1, 100);

                if (rnd <= percentProfit)
                {
                    // Сделка прибыльная

                    //============ 1 strategy =========================
                    datas[0].ResultDepo += (take - comiss) * lotpercent;

                    //============ 2 strategy ========================= 

                    datas[1].ResultDepo += (take - comiss) * lotpercent;

                    int newLot = CalculateLot(datas[1].ResultDepo, percent, go);

                    if (lotpercent < newLot) lotpercent = newLot;

                    //============ 3 strategy =========================

                    datas[2].ResultDepo += (take - comiss) * lotProgress;

                    lotProgress = CalculateLot(depoStart, minStartPercent * multyply, go);

                    //============ 4 strategy =========================

                    datas[3].ResultDepo += (take - comiss) * lotDown;

                    lotDown = startLot;
                }
                else
                {
                    // Сделка убыточная
                    //============ 1 strategy =========================
                    datas[0].ResultDepo -= (stop + comiss) * startLot;

                    //============ 2 strategy =========================

                    datas[1].ResultDepo -= (stop + comiss) * lotpercent;

                    //============ 3 strategy =========================

                    datas[2].ResultDepo -= (stop + comiss) * lotProgress;

                    lotProgress = CalculateLot(depoStart, minStartPercent , go);

                    //============ 4 strategy =========================

                    datas[3].ResultDepo -= (stop + comiss) * lotDown;

                    lotDown /= 2;

                    if (lotDown == 0) lotDown = 1;
                }
            }

            _dataGrid.ItemsSource = datas;

            g = 1;

            return datas;
        }

        private void Draw(List<Data> datas, int index)
        {
            _canvas.Children.Clear();

            index = _comboBox.SelectedIndex;

            List<decimal> listEquity = datas[index].GetListEquity();

            if ( listEquity.Count == 0) return;

            int count = listEquity.Count;
            decimal maxEquity = listEquity.Max();
            decimal minEquity = listEquity.Min();

            double stepX = _canvas.ActualWidth / count;
            double koef = (double)(maxEquity - minEquity) / _canvas.ActualHeight;

            double x = 0;
            double y = 0;

            double lX1 = 0; 
            double lY1 = 0; 
            double lX2 = 0; 
            double lY2 = 0; 

            for (int i = 0; i < count; i++)
            {
              
                lY1 = lY2;

                y = _canvas.ActualHeight - (double)(listEquity[i] - minEquity) / koef;

                lY2 = y;

                Line line = new Line()
                {
                    Stroke = Brushes.Black,
                    StrokeThickness = 2,
                    X1 = lX1,
                    X2 = lX2,
                    Y1 = lY1,
                    Y2 = lY2
                };

                lX1 = lX2;
                x += stepX;
                lX2 = x;
                _canvas.Children.Add(line);

                
            }


        }

        private void Canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
             int index = _comboBox.SelectedIndex;

            if (g == 1) { Draw(datas, index); }

            return;
        }
        private int CalculateLot(decimal currentDepo, decimal percent, decimal go)
        {
            if (percent > 100) { percent = 100; }

            decimal lot = currentDepo / go / 100 * percent;

            return (int) lot;
        }

        private decimal GetDecimalFromString(string str)
        {
            if (decimal.TryParse(str, out decimal result)) return result;

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