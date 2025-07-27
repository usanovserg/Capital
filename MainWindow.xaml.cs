using Capital.Enams;
using Capital.Entity;
using System;
using System.ComponentModel;
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

        #region Fields ======================================================================

        private readonly List<StrategyType> _strategies = [.. Enum.GetValues<StrategyType>()];

        Random _random = new();

        private List<Data> _dataList = [];

        #endregion

        #region Methods =====================================================================

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
            _countTrades.Text = "1000";
            _percentProfit.Text = "30";
            _go.Text = "5000";
            _minStartPercent.Text = "20";
        }

        private void _comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;

            int index = comboBox.SelectedIndex;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {           
            _dataList = Calculate();

            Draw(_dataList);
        }

        private List<Data> Calculate()
        {
            decimal depoStart = GetNumberFromString<decimal>(_depo.Text);
            int startLot = GetNumberFromString<int>(_startLot.Text);
            decimal take = GetNumberFromString<decimal>(_take.Text);
            decimal stop = GetNumberFromString<decimal>(_stop.Text);
            decimal comiss = GetNumberFromString<decimal>(_comiss.Text);
            int countTrades = GetNumberFromString<int>(_countTrades.Text);
            decimal percentProfit = GetNumberFromString<decimal>(_percentProfit.Text);
            decimal minStartPercent = GetNumberFromString<decimal>(_minStartPercent.Text);
            decimal go = GetNumberFromString<decimal>(_go.Text);

            int lotPercent = startLot;
            decimal percent = startLot * go * 100 / depoStart;

            decimal multiply = take / stop;
            int lotProgress = CalculateLot(depoStart, minStartPercent, go);

            int lotDown = startLot;

            List<Data> dataList = [.. _strategies.Select(x => new Data(depoStart, x))];


            for (int i=0; i<countTrades; i++)
            {
                int rnd = _random.Next(1, 100);

                if (rnd <= percentProfit)
                {
                    // Сделка прибыльная
                    // ================ 1 strategy ==================================

                    dataList[0].ResultDepo += (take - comiss) * startLot;

                    // ================ 2 strategy ==================================

                    dataList[1].ResultDepo += (take - comiss) * lotPercent;

                    int newLot = CalculateLot(dataList[1].ResultDepo, percent, go);

                    if (lotPercent < newLot) lotPercent = newLot;

                    // ================ 3 strategy ==================================

                    dataList[2].ResultDepo += (take - comiss) * lotProgress;

                    lotProgress = CalculateLot(depoStart, minStartPercent * multiply, go);

                    // ================ 4 strategy ==================================

                    dataList[3].ResultDepo += (take - comiss) * lotDown;

                    lotDown = startLot;
                }
                else
                {
                    // Сделака убыточная
                    // ================ 1 strategy ==================================

                    dataList[0].ResultDepo -= (stop + comiss) * startLot;

                    // ================ 2 strategy ==================================

                    dataList[1].ResultDepo -= (stop + comiss) * lotPercent;

                    // ================ 3 strategy ==================================

                    dataList[2].ResultDepo -= (stop + comiss) * lotProgress;

                    lotProgress = CalculateLot(depoStart, minStartPercent, go);

                    // ================ 4 strategy ==================================

                    dataList[3].ResultDepo -= (stop + comiss) * lotDown;

                    lotDown /= 2;

                    if (lotDown == 0) lotDown = 1;
                }
            }          

            _dataGrid.ItemsSource = dataList;

            return dataList; 
        }

        private void Draw(List<Data> dataList)
        {
            _canvas.Children.Clear();

            int index = _comboBox.SelectedIndex;

            List<decimal> ListEquity = dataList[index].GetListEquity();

            int count = ListEquity.Count;
            decimal maxEquity = ListEquity.Max();
            decimal minEquity = ListEquity.Min();

            double stepX = _canvas.ActualWidth / (count - 1);
            double koef = (double)(maxEquity - minEquity) / _canvas.ActualHeight;

            double x = 0;
            double y = 0;

            List<Point> points = new();

            for (int i = 0; i < count; i++)
            {
                y = _canvas.ActualHeight - (double)(ListEquity[i] - minEquity) / koef;
                x = i * stepX;

                points.Add(new Point(x, y));
            }

            if (points.Count < 2) return;

            for (int i = 0; i < points.Count - 1; i++)
            {
                Line line = new()
                {
                    X1 = points[i].X,
                    Y1 = points[i].Y,
                    X2 = points[i + 1].X,
                    Y2 = points[i + 1].Y,
                    Stroke = Brushes.Black,
                    StrokeThickness = 2
                };

                _canvas.Children.Add(line);
            }
        }

        private static int CalculateLot(decimal currentDepo, decimal percent, decimal go)
        {
            if (percent > 100) { percent = 100; }

            decimal lot = currentDepo / go / 100 * percent;

            return (int)lot;
        }

        public static T GetNumberFromString<T>(string input) where T : struct
        {
            if (typeof(T) == typeof(int))
            {
                if (int.TryParse(input, out int intResult))
                {
                    return (T)(object)intResult;
                }
            }
            else if (typeof(T) == typeof(decimal))
            {
                if (decimal.TryParse(input, out decimal decimalResult))
                {
                    return (T)(object)decimalResult;
                }
            }

            throw new InvalidOperationException($"Cannot convert '{input}' to {typeof(T).Name}");
        }

        #endregion

        private void Canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {   
            if (_dataList.Count != 0)
            {
                Draw(_dataList);
            }            
        }

        private void ComboBox_DropDownClosed(object sender, EventArgs e)
        {
            if (_dataList.Count != 0)
            {
                Draw(_dataList);
            }
            else
            {
                _dataList = Calculate();

                Draw(_dataList);
            }            
        }

        private void _dataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyDescriptor is PropertyDescriptor descriptor)
            {
                var displayName = descriptor.Attributes[typeof(DisplayNameAttribute)] as DisplayNameAttribute;
                if (displayName != null && !string.IsNullOrEmpty(displayName.DisplayName))
                {
                    e.Column.Header = displayName.DisplayName;
                }
            }
        }
    }
}