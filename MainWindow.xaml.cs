﻿using Capital.Enams;
using Capital.Entity;
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

            this.SizeChanged += MainWindow_SizeChanged;
        }


        #region Fields ==============================================================================

        List<StrategyType> _strategies = new List<StrategyType>()
        {
            StrategyType.FIX,
            StrategyType.CAPITALIZATION,
            StrategyType.PROGRESS,
            StrategyType.DOWNGRADE,
        };

        Random _random = new Random();

        List<Data> datas = new List<Data>();

        #endregion

        #region Methods =============================================================================

        private void Init()
        {
            _comboBox.ItemsSource = _strategies;

            _comboBox.SelectionChanged += _comboBox_SelectionChanged;
            _comboBox.SelectedIndex = 0;

            _depo.Text = "100000";
            _startLot.Text = "10";
            _take.Text = "300";
            _stop.Text = "100";
            _comiss.Text = "5";
            _countTraids.Text = "1000";
            _percentProfit.Text = "30";
            _go.Text = "5000";
            _minStartPercent.Text = "20";
            
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (datas.Count != 0)
                Draw(datas);
        }
        private void _comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //ComboBox comboBox = (ComboBox)sender;
            //int index = comboBox.SelectedIndex;
            if (datas.Count != 0)
                Draw(datas);

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            datas = Calculate();
            Draw(datas);          

        }

        private List<Data> Calculate()
        {
            decimal depoStart = GetDecimalFromString(_depo.Text);
            int startLot = GetIntFromString(_startLot.Text);
            decimal take = GetDecimalFromString(_take.Text);
            decimal stop = GetDecimalFromString(_stop.Text);
            decimal comiss = GetDecimalFromString(_comiss.Text);
            int countTrades = GetIntFromString(_countTraids.Text);
            decimal percProfit = GetDecimalFromString(_percentProfit.Text);
            decimal minStartPercent = GetDecimalFromString(_minStartPercent.Text);
            decimal go = GetDecimalFromString(_go.Text);

            List<Data> datas = new List<Data>();

            foreach (StrategyType type in _strategies)
            {
                datas.Add(new Data(depoStart, type));
            }

            int lotPercent = startLot;
            decimal persent = startLot * go * 100 / depoStart;

            decimal multyply = take / stop;
            int lotProgress = CalculateLot(depoStart, minStartPercent, go);

            int lotDown = startLot;

            for (int i = 0; i < countTrades; i++)
            {
                int rnd = _random.Next(1, 100);

                if (rnd <= percProfit)
                {
                    //Сделка прибыльная

                    // 1 стратегия ===================================================
                    datas[0].ResultDepo += (take - comiss) * startLot;

                    // 2 стратегия ===================================================
                    datas[1].ResultDepo += (take - comiss) * lotPercent;

                    int newLot = CalculateLot(datas[1].ResultDepo, persent, go);

                    if (lotPercent < newLot) lotPercent = newLot;

                    // 3 стратегия ===================================================
                    datas[2].ResultDepo += (take - comiss) * lotProgress;

                    lotProgress = CalculateLot(depoStart, minStartPercent * multyply, go);

                    // 4 стратегия ===================================================
                    datas[3].ResultDepo += (take - comiss) * lotDown;

                    lotDown = startLot;
                }
                else
                {
                    //Сделка убыточная

                    // 1 стратегия ===================================================
                    datas[0].ResultDepo -= (stop + comiss) * startLot;

                    // 2 стратегия ===================================================
                    datas[1].ResultDepo -= (stop + comiss) * lotPercent;

                    // 3 стратегия ===================================================
                    datas[2].ResultDepo -= (stop + comiss) * lotProgress;

                    lotProgress = CalculateLot(depoStart, minStartPercent, go);

                    // 4 стратегия ===================================================
                    datas[3].ResultDepo -= (stop + comiss) * lotDown;

                    lotDown /= 2;

                    if (lotDown == 0) lotDown = 1;
                }
            }

            _dataGride.ItemsSource = datas;

            return datas;
        }

        private void Draw(List<Data> datas)
        {
            _canvas.Children.Clear();

            int index = _comboBox.SelectedIndex;

            List<decimal> listEquity = datas[index].GetListEquity();

            int count = listEquity.Count;

            decimal maxEquity = listEquity.Max();

            decimal minEquity = listEquity.Min();

            double stepX = _canvas.ActualWidth / count;

            double koef = (double)(maxEquity - minEquity) / _canvas.ActualHeight;

            double x = 0;

            double y = 0;

            double lastX = 0;

            double lastY = _canvas.ActualHeight - (double)(listEquity[0] - minEquity) / koef;

            for (int i = 0; i < count; i++)
            {
                y = _canvas.ActualHeight - (double)(listEquity[i] - minEquity) / koef;

                Line line = new Line()
                {
                    X1 = lastX,

                    Y1 = lastY,

                    X2 = lastX + stepX,

                    Y2 = y,

                    Stroke = Brushes.Black
                };

                _canvas.Children.Add(line);

                lastX = x;

                lastY = y;

                x += stepX;
            }

            /*double x = 0;

            double y = 0;

            for (int i = 0; i < count; i++)
            {
                y = _canvas.ActualHeight - (double)(listEquity[i] - minEquity) / koef;

                Ellipse ellips = new Ellipse()
                {
                    Width = 2,

                    Height = 2,

                    Stroke = Brushes.Black
                };

                Canvas.SetLeft(ellips, x);

                Canvas.SetTop(ellips, y);

                _canvas.Children.Add(ellips);

                x += stepX;
            }*/

        }

        private int CalculateLot(decimal currentDepo, decimal percent, decimal go)
        {
            if (percent > 100) { percent = 100; }
            decimal lot = currentDepo / go / 100 * percent;

            return (int)lot;
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