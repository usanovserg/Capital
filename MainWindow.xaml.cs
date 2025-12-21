using Capital.Entity;
using Capital.Enums;
using OxyPlot;
using OxyPlot.Wpf;
using OxyPlot.Axes;
using OxyPlot.Series;
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
        #region Fields=======================================================

        List<StrategyType> _strategies = new List<StrategyType>
        {
                StrategyType.FIX,
                StrategyType.CAPITALIZATION,
                StrategyType.PROGRESS,
                StrategyType.DOWNGRADE, 
                StrategyType.ALL_STRATEGIES               
        };

        
        Random _random = new Random();

        List<Data> datas = new List<Data>();

        int _totalProfitCount = 0;
        int _totalLossCount = 0;     
        

        #endregion

        #region Methods======================================================

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Init();
            _isInitialized = true; // разрешаем обработку событий
            datas = Calculate(); // рассчитываем первый раз
            Draw(datas, _comboBox.SelectedIndex);
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
            //ComboBox comboBox = (ComboBox)sender;

            if (!_isInitialized) return;

            int index = _comboBox.SelectedIndex;

            if (index == _strategies.IndexOf(StrategyType.ALL_STRATEGIES))
            {
                DrawAllStrategies(datas); // ← рисует все 4 линии
            }
            else if (index >= 0 && index < datas.Count)
            {
                DrawSingleGraph(datas[index]); // ← рисует одну линию
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            datas = Calculate();

            Draw(datas, _comboBox.SelectedIndex);
        }

        private List<Data> Calculate()
        {
            decimal depoStart = GetDecimalFromString(_depo.Text);
            int startLot = GetIntFromString(_startLot.Text);
            decimal take = GetDecimalFromString(_take.Text);
            decimal stop = GetDecimalFromString(_stop.Text);
            decimal comiss = GetDecimalFromString(_comiss.Text);
            int countTrades = GetIntFromString(_countTrades.Text);
            decimal percentProfit = GetDecimalFromString(_percentProfit.Text); // процент прибыльных сделок
            decimal minStartPercent = GetDecimalFromString(_minStartPercent.Text);
            decimal go = GetDecimalFromString(_go.Text);

            if (depoStart <= 0)
            {
                MessageBox.Show("Депозит должен быть больше нуля!", "Ошибка ввода",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return new List<Data>();
            }

            if(countTrades <= 0)
            {
                MessageBox.Show("Количество сделок должно быть больше нуля!");
                return new List<Data>();
            }
            // сбрасываем счетчики
            _totalProfitCount = 0;
            _totalLossCount = 0;

            List<Data> datas = new List<Data>();

            foreach (StrategyType type in _strategies)
            {
                if (type != StrategyType.ALL_STRATEGIES)
                {
                    datas.Add(new Data(depoStart, type));
                }                                                   
            }

            int lotPercent = startLot;
            decimal percent = startLot * go * 100 / depoStart;

            decimal multiply = take / stop;

            int lotProgress = CalculateLot(depoStart, minStartPercent, go);

            int lotDown = startLot;

            for (int i = 0; i < countTrades; i++)
            {
                int rnd = _random.Next(1, 100);

                if (rnd <= percentProfit)            // Сделка прибыльная
                { 
                    _totalProfitCount++;
                    //==================================== 1 стратегия ===============================

                    datas[0].ResultDepo += (take - comiss) * startLot;
                    

                    //==================================== 2 стартегия ===============================

                    datas[1].ResultDepo += (take - comiss) * lotPercent;

                    int newLot = CalculateLot(datas[1].ResultDepo, percent, go);

                    if (lotPercent < newLot) lotPercent = newLot;

                    //=================================== 3 стратегия ================================

                    datas[2].ResultDepo += (take - comiss) * lotProgress;

                    lotProgress = CalculateLot(depoStart, minStartPercent * multiply, go);

                    //=================================== 4 стратегия ================================

                    datas[3].ResultDepo += (take - comiss) * lotDown;

                    lotDown = startLot;
                    
                }
                else                                  //Сделка убыточная
                {   
                    _totalLossCount++;
                    //==================================== 1 стратегия ===============================

                    datas[0].ResultDepo -= (stop + comiss) * startLot;                    

                    //==================================== 2 стартегия ===============================

                    datas[1].ResultDepo -= (stop + comiss) * lotPercent;

                    //=================================== 3 стратегия ================================

                    datas[2].ResultDepo -= (stop + comiss) * lotProgress;

                    lotProgress = CalculateLot(depoStart, minStartPercent, go);

                    //=================================== 4 стратегия ================================

                    datas[3].ResultDepo -= (stop + comiss) * lotDown;

                    lotDown /= 2;

                    if (lotDown == 0) lotDown = 1;
                    
                }
                
            }
            _totalProfit.Text = _totalProfitCount.ToString();
            _totalLoss.Text = _totalLossCount.ToString();

            _dataGrid.ItemsSource = datas;            

            return datas;
        }
        /// <summary>
        /// рисуем график в canvas
        /// </summary>
        /// <param name="datas"></param>
        /// <param name="index"></param>
        private void Draw(List<Data> datas, int index)
        {
            // чистим предыдущую зарисовку canvas
            _plotView.Model = null;

            if (datas == null || datas.Count == 0) return; // выходим при ошибке
                        
            
            // Если выбрано ALL_STRATEGIES - рисуем все графики

            if (index == _strategies.IndexOf(StrategyType.ALL_STRATEGIES))
            {
                DrawAllStrategies(datas);
                return;
            }
            
            // Иначе - рисуем один график
            if (index < 0 || index >= datas.Count) return;

            List<decimal> listEquity = datas[index].GetListEquity();
            if (listEquity.Count == 0 || listEquity == null) return;

            // Рисуем один график
            DrawSingleGraph(datas[index]);           
        }

        private void DrawAllStrategies(List<Data> datas)
        {
            var plotModel = new PlotModel { Title = "Результаты всех стратегий" };

            // Ось X: номера сделок
            plotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Количество сделок",
                Minimum = 0,
                Maximum = datas[0].GetListEquity().Count - 1
            });

            // Ось Y: депо
            decimal min = datas.SelectMany(d => d.GetListEquity()).Min();
            decimal max = datas.SelectMany(d => d.GetListEquity()).Max();
            plotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Депозит (руб.)",
                Minimum = (double)min,
                Maximum = (double)max
            });

            // Цвета
            var colors = new[] { OxyColors.Red, OxyColors.Green, OxyColors.Blue, OxyColors.Orange };

            for (int i = 0; i < datas.Count; i++)
            {
                var listEquity = datas[i].GetListEquity();
                var lineSeries = new LineSeries
                {
                    Title = datas[i].StrategyType.ToString(),
                    Color = colors[i],
                    MarkerType = MarkerType.None,
                    StrokeThickness = 2, // толщина линии
                    LineStyle = LineStyle.Solid // сплошная линия
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
                PlotMargins = new OxyThickness(60, 10, 10, 50) // отступы для осей
            };

            // === Ось X: номера сделок (0, 1, 2, ..., N) ===
            plotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Количество сделок",
                Minimum = 0,
                Maximum = equity.Count - 1,
                MajorStep = Math.Max(1, (equity.Count - 1) / 5), // не более 5 меток
                IsPanEnabled = false,
                IsZoomEnabled = false
            });

            // === Ось Y: значения депо ===
            decimal minEquity = equity.Min();
            decimal maxEquity = equity.Max();
            // Небольшой отступ сверху и снизу
            double marginY = (double)(maxEquity - minEquity) * 0.05;
            if (marginY == 0) marginY = 1000;

            plotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Депозит (руб.)",
                Minimum = (double)minEquity - marginY,
                Maximum = (double)maxEquity + marginY,
                StringFormat = "N0", // формат: 100000 вместо 1E+05
                IsPanEnabled = false,
                IsZoomEnabled = false
            });

            // === Одна серия (одна линия) ===
            var lineSeries = new LineSeries
            {
                Title = selectedData.StrategyType.ToString(),
                Color = OxyColors.Blue,
                MarkerType = MarkerType.None,
                StrokeThickness = 2, // толщина линии
                LineStyle = LineStyle.Solid // сплошная линия 
            };

            for (int i = 0; i < equity.Count; i++)
            {
                lineSeries.Points.Add(new DataPoint(i, (double)equity[i]));
            }

            plotModel.Series.Add(lineSeries);

            // Присваиваем модель графику
            _plotView.Model = plotModel;
        }        

        private int CalculateLot(decimal currentDepo, decimal percent, decimal go)
        {
            if (percent > 100) { percent = 100; }

            decimal lot = currentDepo / go / 100 * percent;

            return (int)lot;
        }

        private decimal GetDecimalFromString(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return 0;

            str = str.Trim().Replace(" ", "").Replace(",", "."); // убираем пробелы и запятые на точки
            
            // пробуем распарсить, подключил using System.Globalization;
            if (decimal.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal result)) 
                return result;

            return 0; // если не удалось возвращаем 0, а валидация поймает это
        }

        private int GetIntFromString(string str)
        {
            if (int.TryParse(str, out int result)) return result;

            return 0;
        }

        #endregion

        private void _canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Draw(datas, _comboBox.SelectedIndex);
        }

        private void GroupBox_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _dataGrid.ItemsSource = datas;
        }

        
    }
}