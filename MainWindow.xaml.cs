﻿using Capital.Enams;
using Capital.Entity;
using System;
using System.Runtime.CompilerServices;
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


        #region Fields=========================================================================================

        List<StrategyType> _strategies = new List<StrategyType>()

        {
                StrategyType.FIX,
                StrategyType.CAPITALIZATION,
                StrategyType.PROGRESS,
                StrategyType.DOWNGRADE
        };

        Random _random = new Random();
        
        List<Data> datas = new List<Data>();
        #endregion



        #region Methods=======================================================================================

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
            _countTrades.Text = "500";
            _persentProfit.Text = "30";
            _go.Text = "5000";
            _minStartPercent.Text = "20";

            /*List<StrategyType> strategyTypes = new List<StrategyType>();
                
                strategyTypes.Add(StrategyType.FIX);
                strategyTypes.Add(StrategyType.CAPITALIZATION);
                strategyTypes.Add(StrategyType.PROGRESS);
                strategyTypes.Add(StrategyType.DOWNGRADE);
            
            _comboBox.ItemsSource = strategyTypes;*/


        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            datas = Calculate();

            Draw(datas);
        }
        
        private void _comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //ComboBox comboBox = (ComboBox)sender;

            int index = _comboBox.SelectedIndex;
           
            if (datas.Count != 0) 
            
            Draw(datas);
        }

        private void _canvas_SizeChanged_2(object sender, SizeChangedEventArgs e)
        {
            if (datas.Count != 0)

            Draw(datas);
        }

        private List<Data> Calculate()
        {
            decimal depoStart = GetDecimalFromString(_depo.Text);
            int startLot = GetIntFromString(_startLot.Text);
            decimal take = GetDecimalFromString(_take.Text);
            decimal stop = GetDecimalFromString(_stop.Text);
            decimal comiss = GetDecimalFromString(_comiss.Text);
            int countTrade = GetIntFromString(_countTrades.Text);
            decimal percProfit = GetDecimalFromString(_persentProfit.Text);
            decimal minStartPersent = GetDecimalFromString(_minStartPercent.Text);
            decimal go = GetDecimalFromString(_go.Text);


            //List<Data> datas = new List<Data>();

            foreach (StrategyType type in _strategies)
            {
                datas.Add(new Data(depoStart, type));
            }

            int lotPercent = startLot;
            decimal percent = startLot * go * 100 / depoStart;

            decimal multiply = take / stop;
            decimal lotProgress = CalculateLot(depoStart, minStartPersent, go);

            int lotDown = startLot;

            for (int i = 0; i < countTrade; i++)
            {
                int rnd = _random.Next(1, 100);

                if (rnd <= percProfit)

                {
                    // Сделка прибыльная

                    //===================== 1 strategy ===========================================================

                    datas[0].ResultDepo += (take - comiss) * startLot;

                    //===================== 2 strategy ===========================================================

                    datas[1].ResultDepo += (take - comiss) * lotPercent;

                    int newLot = CalculateLot(datas[1].ResultDepo, percent, go);

                    if (lotPercent < newLot) lotPercent = newLot;

                    //===================== 3 strategy ===========================================================

                    datas[2].ResultDepo += (take - comiss) * lotProgress;

                    lotProgress = CalculateLot(depoStart, minStartPersent * multiply, go);

                    //===================== 4 strategy ===========================================================

                    datas[3].ResultDepo += (take - comiss) * lotDown;

                    lotDown = startLot;

                }
                else
                {
                    // Сделка убыточная

                    //===================== 1 strategy ===========================================================

                    datas[0].ResultDepo -= (stop + comiss) * startLot;

                    //===================== 2 strategy ===========================================================

                    datas[1].ResultDepo -= (stop + comiss) * lotPercent;

                    //===================== 3 strategy ===========================================================

                    datas[2].ResultDepo -= (stop + comiss) * lotProgress;

                    lotProgress = CalculateLot(depoStart, minStartPersent, go);

                    //===================== 4 strategy ===========================================================

                    datas[3].ResultDepo -= (stop + comiss) * lotDown;

                    lotDown /= 2;

                    if (lotDown == 0) lotDown = 1;
                }

            }

            _dataGrid.ItemsSource = datas;

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
            double nx = 0;
            double ny = _canvas.ActualHeight - (double) (listEquity[0]-minEquity)/koef;

            for (int i=1; i<count; i++)
            { 
                              
                Ellipse ellipse = new Ellipse()
                    {
                        Width = 5,
                        Height = 5,
                        Stroke = Brushes.Black,
                    };
                    Canvas.SetLeft(ellipse, x);
                    Canvas.SetTop(ellipse, ny);

                    _canvas.Children.Add(ellipse);
                    
                Line line = new Line();
                
                line.X1 = nx; line.Y1 = ny;
                                            
                x += stepX;
                y = _canvas.ActualHeight - (double)(listEquity[i] - minEquity) / koef;
                
                line.X2 = x; line.Y2 = y;
                line.Stroke = Brushes.Black;
                line.StrokeThickness = 2;
                _canvas.Children.Add(line);

                nx=x; ny=y;
            }

        }
 
        private int CalculateLot(decimal currentDepo, decimal percent, decimal go)
        {
            if (percent > 100) { percent = 100; }

            decimal lot = currentDepo / go / 100 * percent;

            return (int)lot;
        }

        private decimal GetDecimalFromString(string str)
        {
            if (decimal.TryParse(str, out decimal rezult)) return rezult;
            return 0;
        }

        private int GetIntFromString(string str)
        {
            if (int.TryParse(str, out int rezult)) return rezult;
            return 0;
        }

        #endregion
       
    }

}