using Capital.Entity;
using Capital.Enums;
using OxyPlot;
using OxyPlot.Series;
using System;
using System.Globalization;
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
        PlotModel model;

        #endregion

        #region Methods ===========================================

        private void Init()
        {
            //_comBox.ItemsSource = _strategies;
            
            foreach (StrategyType type in _strategies)
            {
                _comBox.Items.Add(type.ToString());
            }

            _comBox.Items.Add("Все стратегии");
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

            ShowPlot(index);
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

            // создаем серии для PlotModel
            List<LineSeries> series = new List<LineSeries>();
            for (int k = 0; k < _strategies.Count; k++)
            {
                series.Add(new LineSeries());
                series[k].Points.Add(new DataPoint(0, (double)depoStart));
            }

            foreach (StrategyType type in _strategies)
            {
                datas.Add(new Data(depoStart, type));
                maxDrawDown.Add(decimal.MaxValue);
                maxResultDepo.Add(decimal.MinValue);
                percentDrawDown.Add(decimal.MaxValue);

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
                    //percentDrawDown[k] = maxDrawDown[k] / maxResultDepo[k] * 100;
                    percentDrawDown[k] = Math.Min(percentDrawDown[k], (datas[k].ResultDepo - maxResultDepo[k])/ maxResultDepo[k]*100);
                }

                // добавляем новую точку в series
                for (int k = 0; k < _strategies.Count; k++)
                {
                    if (i!=0) 
                    {
                        series[k].Points.Add(new DataPoint(i, (double)datas[k].ResultDepo));

                    }
                }

            }



            int j = 0;
            foreach (var data in datas) 
            {
                data.Profit = data.ResultDepo - data.Depo;
                //data.PercentProfit = Math.Round(data.Profit/data.Depo*100,2);
                data.PercentProfit = data.Profit / data.Depo * 100;
                data.MaxDrawDown = maxDrawDown[j];
                data.PercentDrawDown = percentDrawDown[j];
                j++;
            }

            _dataGrid.ItemsSource= datas;

            // добавляем series в PlotModel
            model = new PlotModel { Title = "График изменения капитала" };
            for (int k = 0; k < _strategies.Count; k++)
            {
                model.Series.Add(series[k]);
            }
            plotView?.Model = model;
            ShowPlot(_comBox.SelectedIndex);
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
                    AllActivate = true;
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

        /// <summary>
        /// обработчик события прорисовки колонки таблицы
        /// </summary>
        private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.Column is DataGridTextColumn col)
            {
                // Форматирование числа с разделителем тысяч
                string format =
                    e.PropertyName == "PercentProfit" || e.PropertyName == "PercentDrawDown"
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
        #endregion
    }
}