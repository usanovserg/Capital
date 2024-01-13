﻿using Capital.Entity;
using Capital.Enums;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
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

        #region Fields =========================================================

        List<StrategyType> _strategies = new List<StrategyType>()
        {
            StrategyType.FIX,
            StrategyType.CAPITALIZATION,
            StrategyType.PROGRESS,
            StrategyType.DOWNGRADE

        };

        Random _random = new Random();

        List<Data>? data { get; set; }

        #endregion

        #region Methods ========================================================

        private void Init()
        {
            _comboBox.ItemsSource = _strategies;
            _comboBox.SelectionChanged += _comboBox_SelectionChanged;
            _comboBox.SelectedIndex = 0;
            _canvas.SizeChanged += Window_SizeChanged;

            _depo.Text = "100000";
            _startLot.Text = "10";
            _take.Text = "300";
            _stop.Text = "100";
            _comiss.Text = "5";
            _countTrades.Text = "1000";
            _percentProfit.Text = "30";
            _go.Text = "5000";
            _minStartPercent.Text = "20";

        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            int index = _comboBox.SelectedIndex;

            if (data != null)
            {
                List<decimal> _data = GetData(data, index);
                Draw(_data);
            }
        }
        private void _comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            int index = comboBox.SelectedIndex;
            if (data != null)
            {
                List<decimal> _data = GetData(data, index);
                Draw(_data);
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int index = _comboBox.SelectedIndex;
            data = Calculate();//
            List<decimal> _data = GetData(data, index);
            Draw(_data);//вызываем метод Draw 
        }
        private List<Data> Calculate()// возвращаем лист дата для рисования графика
        {
            decimal depoStart = GetDecimalFromString(_depo.Text);
            int startLot = GetIntFromString(_startLot.Text);
            decimal take = GetDecimalFromString(_take.Text);
            decimal stop = GetDecimalFromString(_stop.Text);
            decimal comiss = GetDecimalFromString(_comiss.Text);
            int countTrades = GetIntFromString(_countTrades.Text);
            decimal persProfit = GetDecimalFromString(_percentProfit.Text);
            decimal minStartPercent = GetDecimalFromString(_minStartPercent.Text);
            decimal go = GetDecimalFromString(_go.Text);

            List<Data> datas = new List<Data>();

            foreach (StrategyType type in _strategies)
            {
                datas.Add(new Data(depoStart, type));
            }

            int lotPercent = startLot;
            decimal percent = startLot * go * 100 / depoStart;
            decimal multiply = take / stop;
            int lotProgress = CalculateLot(depoStart, minStartPercent, go);
            int lotDown = startLot;

            for (int i = 0; i < countTrades; i++)
            {
                int rnd = _random.Next(1, 100);
                if (rnd <= persProfit)
                {
                    // Сделка прибыльная

                    //============= 1 strategy ==============
                    datas[0].ResultDepo += (take - comiss) * startLot;

                    //============= 2 strategy ==============
                    datas[1].ResultDepo += (take - comiss) * lotPercent;
                    int newLot = CalculateLot(datas[1].ResultDepo, percent, go);
                    if (lotPercent < newLot) lotPercent = newLot;

                    //============= 3 strategy ==============
                    datas[2].ResultDepo += (take - comiss) * lotProgress;
                    lotProgress = CalculateLot(depoStart, minStartPercent * multiply, go);
                    //============= 4 strategy ==============
                    datas[3].ResultDepo += (take - comiss) * lotDown;
                    lotDown = startLot;
                }
                else
                {
                    // Сделка убыточная

                    //============= 1 strategy ==============
                    datas[0].ResultDepo -= (stop + comiss) * startLot;

                    //============= 2 strategy ==============
                    datas[1].ResultDepo -= (stop + comiss) * lotPercent;

                    //============= 3 strategy ==============
                    datas[2].ResultDepo -= (stop + comiss) * lotProgress;
                    lotProgress = CalculateLot(depoStart, minStartPercent, go);
                    //============= 4 strategy ==============
                    datas[3].ResultDepo -= (stop + comiss) * lotDown;
                    lotDown /= 2;

                    if (lotDown == 0) lotDown = 1;

                }
            }

            _dataGrid.ItemsSource = datas;
            return datas;// возвращаем для рисования графика
        }

        private void Draw(List<decimal> listEquity)//метод рисования графика. 
        {
            _canvas.Children.Clear();//
            int index = _comboBox.SelectedIndex;// получаем индекс из комбобокса

            //List<decimal> listEquity = datas[index].GetListEquity();

            int count = listEquity.Count;
            decimal maxEquity = listEquity.Max();
            decimal minEquity = listEquity.Min();

            double stepx = _canvas.ActualWidth / count;
            double koef = (double)(maxEquity - minEquity) / _canvas.ActualHeight;

            double x = 0;// текущая координата Х
            double y = 0;// текущая координата Y

            double prevX = 0; // прошлая координата Х
            double prevY = 0; // прошлая координата Y

            for (int i = 0; i < count; i++)
            {
                y = _canvas.ActualHeight - (double)(listEquity[i] - minEquity) / koef;
                /*Ellipse ellipse = new Ellipse()
                {
                    Width = 2,
                        Height = 2,
                        Stroke = Brushes.Brown
                };
                Canvas.SetLeft(ellipse, x);
                Canvas.SetBottom(ellipse, y);

                _canvas.Children.Add(ellipse);*/

                Line line = new Line();
                {
                    line.X1 = x;
                    line.Y1 = y;
                    line.X2 = prevX;
                    line.Y2 = prevY;
                    prevX = x;
                    prevY = y;
                    line.Stroke = Brushes.Black;
                };
                _canvas.Children.Add(line);
                x += stepx;
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
            if (decimal.TryParse(str, out decimal result)) return result;
            return 0;
        }

        private int GetIntFromString(string str)
        {
            if (int.TryParse(str, out int result)) return result;
            return 0;
        }
        private List<decimal> GetData(List<Data> list, int i)
        {
            List<decimal> listData = list[i].GetListEquity();
            return listData;
        }
        #endregion
    }

}
