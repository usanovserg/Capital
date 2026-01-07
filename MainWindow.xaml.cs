using Capital.Entity;
using Capital.Enums;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using OxyPlot.Wpf;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
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
        private bool _isInitialized = false; // флаг для запрета обработки события  _comboBox.SelectionChanged до тех пор, пока окно не загрузится

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        #region Fields

        List<StrategyType> _strategies = new List<StrategyType>
        {
            StrategyType.FIX,
            StrategyType.CAPITALIZATION,
            StrategyType.PROGRESS,
            StrategyType.DOWNGRADE,
            StrategyType.ALL_STRATEGIES
        };

        Random _random = new Random();
        ObservableCollection<Data> datas = new ObservableCollection<Data>(); // глобальная коллекция

        int _totalProfitCount = 0;
        int _totalLossCount = 0;

        #endregion

        #region Methods

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Init();
            _isInitialized = true;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                Calculate(); // выполняем первый расчет стратегий
                Draw(datas, _comboBox.SelectedIndex); // Рисуем график (по умолчанию — ALL_STRATEGIES)
            }));
        }

        private void Init()
        {
            _comboBox.ItemsSource = _strategies;
            _comboBox.SelectionChanged += _comboBox_SelectionChanged;
            _comboBox.SelectedIndex = 4;

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
            if (!_isInitialized) return;

            int index = _comboBox.SelectedIndex;

            if (index == _strategies.IndexOf(StrategyType.ALL_STRATEGIES))
            {
                DrawAllStrategies(datas);
            }
            else if (index >= 0 && index < datas.Count)
            {
                DrawSingleGraph(datas[index]);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Calculate();
            Draw(datas, _comboBox.SelectedIndex);
        }

        // метод без возврата (void)
        private void Calculate()
        {
            // привязка ItemsSource один раз, безопасно
            if (_dataGrid.ItemsSource == null)
            {
                _dataGrid.ItemsSource = datas;
            }

            decimal depoStart = GetDecimalFromString(_depo.Text);
            int startLot = GetIntFromString(_startLot.Text);
            decimal take = GetDecimalFromString(_take.Text);
            decimal stop = GetDecimalFromString(_stop.Text);
            decimal comiss = GetDecimalFromString(_comiss.Text);
            int countTrades = GetIntFromString(_countTrades.Text);
            decimal percentProfit = GetDecimalFromString(_percentProfit.Text);
            decimal minStartPercent = GetDecimalFromString(_minStartPercent.Text);
            decimal go = GetDecimalFromString(_go.Text);

            if (depoStart <= 0)
            {
                MessageBox.Show("Депозит должен быть больше нуля!", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (countTrades <= 0)
            {
                MessageBox.Show("Количество сделок должно быть больше нуля!");
                return;
            }

            _totalProfitCount = 0;
            _totalLossCount = 0;

            // используем ГЛОБАЛЬНУЮ коллекцию
            datas.Clear(); // Очищаем существующую ObservableCollection

            // Создаём объекты и добавляем в глобальную коллекцию
            foreach (StrategyType type in _strategies)
            {
                if (type != StrategyType.ALL_STRATEGIES)
                {
                    datas.Add(new Data(depoStart, type));
                }
            }

            // Проверка, что есть данные для расчёта
            if (datas.Count < 4) return;

            int lotPercent = startLot;
            decimal percent = startLot * go * 100 / depoStart;
            decimal multiply = take / stop;
            int lotProgress = CalculateLot(depoStart, minStartPercent, go);
            int lotDown = startLot;

            for (int i = 0; i < countTrades; i++)
            {
                int rnd = _random.Next(1, 100);

                if (rnd <= percentProfit) // Прибыльная
                {
                    _totalProfitCount++;
                    datas[0].ResultDepo += (take - comiss) * startLot;
                    datas[1].ResultDepo += (take - comiss) * lotPercent;
                    int newLot = CalculateLot(datas[1].ResultDepo, percent, go);
                    if (lotPercent < newLot) lotPercent = newLot;
                    datas[2].ResultDepo += (take - comiss) * lotProgress;
                    lotProgress = CalculateLot(depoStart, minStartPercent * multiply, go);
                    datas[3].ResultDepo += (take - comiss) * lotDown;
                    lotDown = startLot;
                }
                else // Убыточная
                {
                    _totalLossCount++;
                    datas[0].ResultDepo -= (stop + comiss) * startLot;
                    datas[1].ResultDepo -= (stop + comiss) * lotPercent;
                    datas[2].ResultDepo -= (stop + comiss) * lotProgress;
                    lotProgress = CalculateLot(depoStart, minStartPercent, go);
                    datas[3].ResultDepo -= (stop + comiss) * lotDown;
                    lotDown /= 2;
                    if (lotDown == 0) lotDown = 1;
                }
            }

            _totalProfit.Text = _totalProfitCount.ToString();
            _totalLoss.Text = _totalLossCount.ToString();
        }

        // принимает ObservableCollection<Data>
        private void Draw(ObservableCollection<Data> datas, int index)
        {
            _plotView.Model = null;
            if (datas == null || datas.Count == 0) return;

            if (index == _strategies.IndexOf(StrategyType.ALL_STRATEGIES))
            {
                DrawAllStrategies(datas);
                return;
            }

            if (index < 0 || index >= datas.Count) return;
            var listEquity = datas[index].GetListEquity();
            if (listEquity == null || listEquity.Count == 0) return;

            DrawSingleGraph(datas[index]);
        }

        // принимает ObservableCollection<Data>
        private void DrawAllStrategies(ObservableCollection<Data> datas)
        {
            var plotModel = new PlotModel
            {
                Title = "Результаты всех стратегий"
            };

            // Создаём легенду
            var legend = new Legend
            {
                LegendTitle = "Стратегии",
                LegendPosition = LegendPosition.LeftTop,
                //plotModel.LegendBorderColor = OxyColors.Black,
                //plotModel.LegendBackgroundColor = OxyColor.FromAColor(150, OxyColors.White)
            };

            // Добавляем легенду в модель
            plotModel.Legends.Add(legend);

            // Ось Х с сеткой
            plotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Количество сделок",
                Minimum = 0,
                Maximum = datas[0].GetListEquity().Count - 1,
                MajorGridlineStyle = LineStyle.Automatic,
                MajorGridlineColor = OxyColors.LightGray
            });

            // ось Y с сеткой
            decimal min = datas.SelectMany(d => d.GetListEquity()).Min(); // выбираем все значения депозита и помещаем в общий список, из которого выбираем минимум 
            decimal max = datas.SelectMany(d => d.GetListEquity()).Max();
            plotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Депозит (руб.)",
                Minimum = (double)min,
                Maximum = (double)max,
                StringFormat = "N0",
                MajorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColors.LightGray
            });

            var colors = new[] { OxyColors.Red, OxyColors.Green, OxyColors.Blue, OxyColors.Orange };

            for (int i = 0; i < datas.Count; i++)
            {
                var listEquity = datas[i].GetListEquity();
                var lineSeries = new LineSeries
                {
                    Title = datas[i].StrategyType.ToString(),
                    Color = colors[i],
                    MarkerType = MarkerType.None,
                    StrokeThickness = 2,
                    LineStyle = LineStyle.Solid,
                    TrackerFormatString = "Стратегия: {0}\nСделка: {1}\nДепо: {2:N0} руб."
                };

                for (int j = 0; j < listEquity.Count; j++)
                {
                    lineSeries.Points.Add(new DataPoint(j, (double)listEquity[j]));
                }

                plotModel.Series.Add(lineSeries);
            }

            _plotView.Model = plotModel;
        }

        private void DrawSingleGraph(Data selectedData)
        {
            if (selectedData == null) return;

            var equity = selectedData.GetListEquity();
            if (equity == null || equity.Count == 0) return;

            var plotModel = new PlotModel
            {
                Title = $"Стратегия: {selectedData.StrategyType}",
                PlotMargins = new OxyThickness(60, 10, 10, 50)
            };

            plotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Количество сделок",
                Minimum = 0,
                Maximum = equity.Count - 1,
                MajorStep = Math.Max(1, (equity.Count - 1) / 5),
                IsPanEnabled = false,
                IsZoomEnabled = false
            });

            decimal minEquity = equity.Min();
            decimal maxEquity = equity.Max();
            double marginY = (double)(maxEquity - minEquity) * 0.05;
            if (marginY == 0) marginY = 1000;

            plotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Депозит (руб.)",
                Minimum = (double)minEquity - marginY,
                Maximum = (double)maxEquity + marginY,
                StringFormat = "N0",
                IsPanEnabled = false,
                IsZoomEnabled = false
            });

            var lineSeries = new LineSeries
            {
                Title = selectedData.StrategyType.ToString(),
                Color = OxyColors.Blue,
                MarkerType = MarkerType.None,
                StrokeThickness = 2,
                LineStyle = LineStyle.Solid
            };

            for (int i = 0; i < equity.Count; i++)
            {
                lineSeries.Points.Add(new DataPoint(i, (double)equity[i]));
            }

            plotModel.Series.Add(lineSeries);
            _plotView.Model = plotModel;
        }

        private int CalculateLot(decimal currentDepo, decimal percent, decimal go)
        {
            if (percent > 100) percent = 100;
            decimal lot = currentDepo / go / 100 * percent;
            return (int)lot;
        }

        private decimal GetDecimalFromString(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return 0;
            str = str.Trim().Replace(" ", "").Replace(",", ".");
            if (decimal.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal result))
                return result;
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