using Capital.Entity;
using Capital.Enums;

using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Capital
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Fields ========================================

        List<StrategyType> _strategies =
        [
            StrategyType.FIX,
            StrategyType.CAPITALIZATION,
            StrategyType.PROGRESS,
            StrategyType.DOWNGRADE
        ];

        Random _random = new();

        List<Data> currentDatas = [];

        #endregion

        public MainWindow()
        {
            InitializeComponent();

            Init();

            // Изменение размера окна - перерисовка графика
            this.SizeChanged += MainWindow_SizeChanged;
        }

        #region Methods ===========================================

        // Изменение размера окна - перерисовка графика
        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_dataGrid.ItemsSource is List<Data> datas)
            {
                Draw(datas);
            }
        }

        private void Init()
        {
            _comboBox.ItemsSource = _strategies;
            _comboBox.SelectedIndex = 0;

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

        // Расчёты эквити для разных стратегий
        private List<Data> Calculate()
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

            List<Data> datas = [];

            // Запись начальных значений депо
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

                if (rnd <= percProfit)
                {
                    // Сделка прибыльная

                    //============ 1 strategy =================

                    datas[0].ResultDepo += (take - comiss) * startLot;

                    //============ 2 strategy ================

                    datas[1].ResultDepo += (take - comiss) * lotPercent;

                    int newLot = CalculateLot(datas[1].ResultDepo, percent, go);

                    if (lotPercent < newLot) lotPercent = newLot;

                    //============ 3 strategy ================

                    datas[2].ResultDepo += (take - comiss) * lotProgress;

                    lotProgress = CalculateLot(depoStart, minStartPercent * multiply, go);

                    //=========== 4 strategy ================== 

                    datas[3].ResultDepo += (take - comiss) * lotDown;

                    lotDown = startLot;
                }
                else
                {
                    // Сделка убыточная

                    //============ 1 strategy =================

                    datas[0].ResultDepo -= (stop + comiss) * startLot;

                    //============ 2 strategy ==================

                    datas[1].ResultDepo -= (stop + comiss) * lotPercent;

                    //============ 3 strategy ==================

                    datas[2].ResultDepo -= (stop + comiss) * lotProgress;

                    lotProgress = CalculateLot(depoStart, minStartPercent, go);

                    //============ 4 strategy ==================

                    datas[3].ResultDepo -= (stop + comiss) * lotDown;

                    lotDown /= 2;

                    if (lotDown == 0) lotDown = 1;
                }
            }

            _dataGrid.ItemsSource = datas;

            return datas;
        }

        // Отрисовка графика
        private void Draw(List<Data> datas)
        {
            // Удаление старого графика перед расчётом нового
            _canvas.Children.Clear();

            // Индекс выбранной стратегии
            int index = _comboBox.SelectedIndex;

            // Список значений по выбранной стратегии
            List<decimal> listEquity = datas[index].GetListEquity();

            // Совет Михаила К. - добавить шум к первым трём стратегиям для наглядности изменений
            // Выбрать один из двух вариантов
            //if (index <= 2)
            //{
            //    Random rnd = new Random();

            //    // Первый вариант шума 
            //    listEquity = listEquity.Select(item => item + rnd.Next(1, 10000)).ToList();

            //    // Второй вариант шума
            //    //listEquity = Enumerable.Range(0, listEquity.Count).Select(i => listEquity[i] + 10000 * (i & 1)).ToList();
            //}

            // Кол-во значений по оси x
            int count = listEquity.Count;

            // Максимальное значение в списке
            decimal maxEquity = listEquity.Max();
            decimal minEquity = listEquity.Min();

            // ActualWidth - текущая ширина окна с графиком
            double stepX = _canvas.ActualWidth / count;

            // ActualHeight - текущая высота окна
            double koefY = (double)(maxEquity - minEquity) / _canvas.ActualHeight;

            double x1 = 0;
            double y1 = 0;
            double x2 = 0;
            double y2 = 0;

            _canvas.Children.Clear();

            // Создать кисть
            SolidColorBrush brush = new()
            {
                Color = Colors.Black
            };

            // Перебираем все значения эквити
            for (int i = 0; i < count; i++)
            {
                x2 = i * stepX;

                y2 = _canvas.ActualHeight - (double)(listEquity[i] - minEquity) / koefY;

                if (i >= 1)
                {
                    // Создать линию
                    Line lineEquity = new()
                    {
                        X1 = x1,
                        Y1 = y1,
                        X2 = x2,
                        Y2 = y2
                    };

                    // Ширина и цвет линии
                    lineEquity.StrokeThickness = 2;
                    lineEquity.Stroke = brush;

                    // Размещение линии на Canvas
                    _canvas.Children.Add(lineEquity);
                }

                x1 = x2;
                y1 = y2;
            }
        }

        // Выпадающий список - Выбор стратегии 
        private void _comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;

            //int index = comboBox.SelectedIndex;

            if (currentDatas != null && currentDatas.Count > 0)
            {
                Draw(currentDatas);
            }

            //click();
            // Програмная имитация клика по кнопке
            //Button_Click(comboBox.SelectedIndex, ev);
        }

        private void click()
        {
            currentDatas = Calculate();

            Draw(currentDatas);
        }

        // Кнопка Рассчитать - Расчёт данных по выбранной стратегии
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            click();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (currentDatas != null && currentDatas.Count > 0)
            {
                Draw(currentDatas);
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

        #endregion
    }
}