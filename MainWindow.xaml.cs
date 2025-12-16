using Capital.Entity;
using Capital.Enums;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Wpf;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
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
using static System.Reflection.Metadata.BlobBuilder;


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
        #region Fields ==================================================================
        List<StrategyType> _strategies = new List<StrategyType>()   // выбираем стратегии которые будем использовать
        {
            StrategyType.FIX,
            StrategyType.CAPITALIZATION,
            StrategyType.PROGESS,
            StrategyType.DOWNGRADE
        };

        Random _random = new Random();
        PlotModel model;
       

        #endregion ======================================================================
        #region    Methods ==============================================================
        private void Init()
        {
          //  _comboBox.ItemsSource = _strategies;
            foreach (StrategyType type in _strategies)
            {
                _comboBox.Items.Add(type.ToString());
            }
            _comboBox.Items.Add("Все стратегии");

            _comboBox.SelectionChanged += _comboBox_SelectionChanged;
            _comboBox.SelectedIndex = 0;

            _depo.Text = "100000";
            _startLot.Text = "10";
            _take.Text = "300";
            _stop.Text= "100";
            _comiss.Text = "5";
            _countTrades.Text= "1000";
            _percetnProfit.Text= "30";
            _go.Text = "5000";
            _minStartPercent.Text = "20";
        }
        private void _comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            int index = comboBox.SelectedIndex;
            ShowPlot(index);
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Calculate();
        }

        private void Calculate()
        {
            decimal depoStart = GetDecimalFromString(_depo.Text);
            int startLot = GetIntlFromString(_startLot.Text);
            decimal take = GetDecimalFromString(_take.Text);
            decimal stop = GetDecimalFromString(_stop.Text);
            decimal comiss = GetDecimalFromString(_comiss.Text);
            int countTrades = GetIntlFromString(_countTrades.Text);
            decimal percetnProfit = GetDecimalFromString(_percetnProfit.Text);
            decimal minStartPercent = GetDecimalFromString(_minStartPercent.Text);
            decimal go = GetDecimalFromString(_go.Text);

            // создаем серии для PlotModel
            List<LineSeries> series = new List<LineSeries>();
            for (int j=0; j< _strategies.Count; j++)
            {
                series.Add(new LineSeries());
                series[j].Points.Add(new DataPoint(0, (double)depoStart));
            }

            List<Data> datas  = new List<Data>();
            foreach (StrategyType type in _strategies)
            {
                datas.Add(new Data(depoStart, type));
            }
            int lotPercent = startLot;
            decimal percent = startLot * go * 100 / depoStart; // процент от депозита

            decimal multiply = take / stop;
            int lotProgress = CalculateLot(depoStart, minStartPercent, go);

            int lotDown = startLot;

            for (int i=0; i<countTrades; i++)
            {
                int rnd = _random.Next(1,100);
                if (rnd <= percetnProfit)
                {
                    // сделка прибыльная
                    foreach (Data dt in datas)
                    {
                        switch (dt.StrategyType)
                        {
                            case StrategyType.FIX:
                                dt.ResultDepo += (take - comiss) * startLot;
                                break;
                            case StrategyType.CAPITALIZATION:
                                dt.ResultDepo += (take - comiss) * lotPercent;
                                int newLot = CalculateLot(dt.ResultDepo, percent, go);
                                if (lotPercent < newLot) lotPercent = newLot;
                                break;
                            case StrategyType.PROGESS:
                                dt.ResultDepo += (take - comiss) * lotProgress;
                                lotProgress = CalculateLot(depoStart, minStartPercent * multiply, go);
                                break;
                            case StrategyType.DOWNGRADE:
                                dt.ResultDepo += (take - comiss) * lotDown;
                                lotDown = startLot;
                                break;
                        }
                    }
                }
                else
                {
                    // убытки
                    foreach (Data dt in datas)
                    {
                        switch (dt.StrategyType)
                        {
                            case StrategyType.FIX:
                                dt.ResultDepo -= (stop + comiss) * startLot;
                                break;
                            case StrategyType.CAPITALIZATION:
                                dt.ResultDepo -= (stop + comiss) * lotPercent;
                                break;
                            case StrategyType.PROGESS:
                                dt.ResultDepo -= (stop + comiss) * lotProgress;
                                lotProgress = CalculateLot(depoStart, minStartPercent, go);
                                break;
                            case StrategyType.DOWNGRADE:
                                dt.ResultDepo -= (stop + comiss) * lotDown;
                                lotDown /= 2;
                                if (lotDown == 0) lotDown = 1;
                                break;
                        }
                    }
                }
                // вычисление MaxDrawDown PercentDrawDown
                foreach (Data dt in datas) 
                {
                    if (dt.Direction) // если двигались на повышение
                    {
                        if (dt.ResultDepo >= dt.Top)  // продолжение повышения
                        {
                            dt.Top = dt.ResultDepo;
                        }
                        else  // начались убытки
                        {
                            dt.Direction = false;
                            dt.Bottom=dt.ResultDepo;
                        }
                    }
                    else
                    {
                        if (dt.ResultDepo < dt.Bottom)// продолжение понижения
                        {
                            dt.Bottom = dt.ResultDepo;
                        }
                        else if (dt.ResultDepo > dt.Top)// новый максимум
                        {
                            dt.Direction = true;
                            decimal tmp = dt.Top - dt.Bottom;
                            if (dt.MaxDrawDown < tmp) dt.MaxDrawDown = tmp; // новый MaxDrawDown
                            if (dt.PercentDrawDown < tmp/ dt.Top*100 ) dt.PercentDrawDown = tmp / dt.Top * 100;// новый PercentDrawDown
                        }
                        // в диапозоне двигаемся
                    }
                }
                // добавляем новую точку в series
                for (int j = 0; j < _strategies.Count; j++)
                {
                    series[j].Points.Add(new DataPoint(i + 1, (double)datas[j].ResultDepo));
                }
            }
            // вычисление показателей
            foreach (Data dt in datas)
            {
                dt.Profit = dt.ResultDepo - dt.Depo;
                dt.PerecentProfit = dt.Profit / dt.Depo * 100;
                if (!dt.Direction)
                {
                    decimal tmp = dt.Top - dt.Bottom;
                    if (dt.MaxDrawDown < tmp) dt.MaxDrawDown = tmp; // новый MaxDrawDown
                    if (dt.PercentDrawDown < tmp / dt.Top * 100) dt.PercentDrawDown = tmp / dt.Top * 100;// новый PercentDrawDown
                }
            }
            _dataGrid.ItemsSource = datas;

            // добавляем series в PlotModel
            model = new PlotModel { Title = "График изменения капитала" };
            for (int j = 0; j < _strategies.Count; j++)
            {
                model.Series.Add(series[j]);
            }
            plotView?.Model = model;
            ShowPlot(_comboBox.SelectedIndex);
        }
        /// <summary>
        /// прорисока графика по индексу из ComboBox
        /// </summary>
        /// <param name="index"></param>
        private void ShowPlot(int index)
        {
            if (plotView?.Model?.Series != null)
            {
                Boolean AllActivate = false;
                if (index == plotView.Model.Series.Count)
                    AllActivate=true;
                for (int i = 0; i < plotView.Model.Series.Count; i++)
                {
                    if (i == index)
                        plotView.Model.Series[i].IsVisible = true;
                    else
                        plotView.Model.Series[i].IsVisible = AllActivate;
                }
            }
            plotView?.InvalidatePlot(true);
        }
        private int CalculateLot(decimal currentDepo, decimal percent, decimal go)
        {
            if (percent > 100) { percent = 100; }
            return (int)(currentDepo / go / 100 * percent);
        }
        private decimal GetDecimalFromString(string str)
        {
            if (decimal.TryParse(str, out decimal result)) return result;
            return 0;
        }
        private int GetIntlFromString(string str)
        {
            if (int.TryParse(str, out int result)) return result;
            return 0;
        }
        /// <summary>
        /// обработчик события прорисовки колонки таблицы
        /// </summary>
        private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.Column is DataGridTextColumn col)
            {
                // Форматирование числа с разделителем тысяч
                string format =
                    e.PropertyName == "PerecentProfit" || e.PropertyName == "PercentDrawDown"
                    ? "{0:N2}"
                    : "{0:N0}";
                col.Binding = new Binding(e.PropertyName)
                {
                    StringFormat = format,
                    ConverterCulture = new CultureInfo("ru-RU")
                };
                // Стиль для отображения
                var displayStyle = new Style(typeof(TextBlock));
                displayStyle.Setters.Add(new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Right));
                col.ElementStyle = displayStyle;
            }
        }


        #endregion ======================================================================


    }
}