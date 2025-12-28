using Capital.Entity;
using Capital.Enums;
using System.Collections;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
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
        Random _random = new Random();
        List<StrategyData> _strategiesData = new List<StrategyData>();
        #endregion


        #region Methods ===========================================
        private void Init()
        {
            _comboBox.ItemsSource = Enum.GetValues<StrategyType>();
            _comboBox.SelectedIndex = 0;
            _comboBox.SelectionChanged += (sender, e) => DrawSelectedStrategy();
            _canvas.SizeChanged += (sender, e) => DrawSelectedStrategy();
            _button.Click += (sender, e) => { Calculate(); DrawSelectedStrategy(); };
            _depo.Text = "100000";
            _startLot.Text = "10";
            _take.Text = "300";
            _stop.Text = "100";
            _comiss.Text = "5";
            _percentProfit.Text = "30";
            _countTrades.Text = "1000";
            _minDepoPercent.Text = "20";
            _go.Text = "5000";
        }

        private void Calculate()
        {
            var startDepo = TryParseDecimal(_depo);
            var startLot = TryParseInt(_startLot);
            var takeProfit = TryParseDecimal(_take);
            var stopLoss = TryParseDecimal(_stop);
            var comission = TryParseDecimal(_comiss);
            var countTrades = TryParseInt(_countTrades);
            var percentProfit = TryParseDecimal(_percentProfit);
            var percentMinDepo = TryParseDecimal(_minDepoPercent);
            var go = TryParseDecimal(_go);

            var data = new List<StrategyData>();
            foreach (var type in Enum.GetValues<StrategyType>())
            {
                data.Add(new StrategyData(type, startDepo));
            }

            var capitalizedLot = startLot;
            var percentCapitalizedLot = startLot * go * 100 / startDepo;
            var multiply = takeProfit / stopLoss;
            var progressedLot = CalculateMaxLot(startDepo, percentMinDepo, go);
            var dawngradeLot = startLot;

            for (var i = 0; i < countTrades; i++)
            {
                if (_random.Next(0, 100) > percentProfit)
                {
                    // loss
                    data[0].ResultDepo -= (stopLoss + comission) * startLot;

                    data[1].ResultDepo -= (stopLoss + comission) * capitalizedLot;

                    data[2].ResultDepo -= (stopLoss + comission) * progressedLot;
                    progressedLot = CalculateMaxLot(startDepo, percentMinDepo, go);

                    data[3].ResultDepo -= (stopLoss + comission) * dawngradeLot;
                    dawngradeLot /= 2;

                }
                else
                {
                    // profit
                    data[0].ResultDepo += (takeProfit - comission) * startLot;

                    data[1].ResultDepo += (takeProfit - comission) * capitalizedLot;
                    var newLot = CalculateMaxLot(data[1].ResultDepo, percentCapitalizedLot, go);
                    if (capitalizedLot < newLot) capitalizedLot = newLot;

                    data[2].ResultDepo += (takeProfit - comission) * progressedLot;
                    progressedLot = CalculateMaxLot(data[2].ResultDepo, percentMinDepo * multiply, go);

                    data[3].ResultDepo += (takeProfit - comission) * dawngradeLot;
                    dawngradeLot = startLot;
                }
            }

            _dataGrid.ItemsSource = data;
            _strategiesData = data;
        }

        private int CalculateMaxLot(decimal currentDepo, decimal percentMaxLot, decimal go)
        {
            if (percentMaxLot > 100) percentMaxLot = 100;
            var maxLot = currentDepo / 100 / go * percentMaxLot;
            return (int)maxLot;
        }

        private decimal TryParseDecimal(TextBox textBox)
        {
            if (decimal.TryParse(textBox.Text, out decimal value))
                return value;
            return 0;
        }

        private int TryParseInt(TextBox textBox)
        {
            if (int.TryParse(textBox.Text, out int value))
                return value;
            return 0;
        }

        private void DrawSelectedStrategy()
        {
            if (_strategiesData.Count == 0)
                return;

            _canvas.Children.Clear();

            var equity = _strategiesData[_comboBox.SelectedIndex].GetEquity();

            // Получаем размеры Canvas
            double width = _canvas.ActualWidth;
            double height = _canvas.ActualHeight;

            // Находим минимальное и максимальное значения
            decimal minValue = equity.Min();
            decimal maxValue = equity.Max();

            // Вычисляем расстояние между точками по оси X
            double xStep = width / (equity.Count - 1);

            // Вычисляем масштаб по оси Y
            double yScale = height / (double)(maxValue - minValue);

            // Создаем кривую линию для графика
            Polyline polyline = new Polyline
            {
                Stroke = Brushes.Blue,
                StrokeThickness = 2
            };

            // Добавляем точки, через которые должна проходить кривая
            for (int i = 0; i < equity.Count; i++)
            {
                double x = i * xStep;
                double y = height - ((double)(equity[i] - minValue) * yScale);
                polyline.Points.Add(new Point(x, y));
            }

            // Добавляем кривую линию на Canvas
            _canvas.Children.Add(polyline);
        }

        #endregion
    }
}