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
        List<StrategyType> _strategies = new List<StrategyType>()
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

            
            var series1 = new LineSeries();
            var series2 = new LineSeries();
            var series3 = new LineSeries();
            var series4 = new LineSeries();
            series1.Points.Add(new DataPoint(0, (double)depoStart));
            series2.Points.Add(new DataPoint(0, (double)depoStart));
            series3.Points.Add(new DataPoint(0, (double)depoStart));
            series4.Points.Add(new DataPoint(0, (double)depoStart));

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
                    // ======== 1 strategy========================
                    datas[0].ResultDepo += (take - comiss) * startLot;

                    // ======== 2 strategy========================
                    datas[1].ResultDepo += (take - comiss) * lotPercent;
                    int newLot = CalculateLot(datas[1].ResultDepo, percent, go);
                    if (lotPercent<newLot) lotPercent = newLot;

                    // ======== 3 strategy========================
                    datas[2].ResultDepo += (take - comiss) * lotProgress;
                    lotProgress = CalculateLot(depoStart, minStartPercent* multiply, go);

                    // ======== 4 strategy========================
                    datas[3].ResultDepo += (take - comiss) * lotDown;
                    lotDown = startLot;
                }
                else
                {
                    // убытки
                    // ========1 strategy========================
                    datas[0].ResultDepo -= (stop + comiss) * startLot;

                    // ======== 2 strategy========================
                    datas[1].ResultDepo -= (stop + comiss) * lotPercent;

                    // ======== 3 strategy========================
                    datas[2].ResultDepo -= (stop + comiss) * lotProgress;
                    lotProgress = CalculateLot(depoStart, minStartPercent, go);

                    // ======== 4 strategy========================
                    datas[3].ResultDepo -= (stop + comiss) * lotDown;
                    lotDown /= 2;
                    if (lotDown == 0) lotDown = 1;

                }

                series1.Points.Add(new DataPoint(i+1, (double)datas[0].ResultDepo));
                series2.Points.Add(new DataPoint(i+1, (double)datas[1].ResultDepo));
                series3.Points.Add(new DataPoint(i+1, (double)datas[2].ResultDepo));
                series4.Points.Add(new DataPoint(i+1, (double)datas[3].ResultDepo));
            }
            _dataGrid.ItemsSource = datas;
 

            model = new PlotModel { Title = "График изменения капитала" };
            model.Series.Add(series1);
            model.Series.Add(series2);
            model.Series.Add(series3);
            model.Series.Add(series4);

            plotView?.Model = model;
            ShowPlot(_comboBox.SelectedIndex);
        }
        private void ShowPlot(int index)
        {
            plotView?.Model?.Series[0]?.IsVisible = false;
            plotView?.Model?.Series[1]?.IsVisible = false;
            plotView?.Model?.Series[2]?.IsVisible = false;
            plotView?.Model?.Series[3]?.IsVisible = false;
            switch (index)
            {
                case 0:
                    plotView?.Model?.Series[0]?.IsVisible = true;
                    break;
                case 1:
                    plotView?.Model?.Series[1]?.IsVisible = true;
                    break;
                case 2:
                    plotView?.Model?.Series[2]?.IsVisible = true;
                    break;
                case 3:
                    plotView?.Model?.Series[3]?.IsVisible = true;
                    break;
                case 4:
                    plotView?.Model?.Series[0]?.IsVisible = true;
                    plotView?.Model?.Series[1]?.IsVisible = true;
                    plotView?.Model?.Series[2]?.IsVisible = true;
                    plotView?.Model?.Series[3]?.IsVisible = true;
                    break;
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
        private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.Column is DataGridTextColumn col)
            {
                // Форматирование числа с разделителем тысяч
                col.Binding = new Binding(e.PropertyName)
                {
                    StringFormat = "{0:N0}",
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