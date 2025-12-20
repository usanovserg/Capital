using Capital.Entity;
using Capital.Enums;
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
            //_canvas.SizeChanged += _canvas_SizeChanged;
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
            //ComboBox comboBox = (ComboBox)sender;

            if (!_isInitialized) return; // не обрабатываем событие, пока окно не загрузилось

            Draw(datas, _comboBox.SelectedIndex);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            datas = Calculate();

            Draw(datas, _comboBox.SelectedIndex);
        }

        private List<Data> Calculate()
        {
            Console.WriteLine($"[DEBUG] _depo.Text = '{_depo.Text}'");
            decimal depoStart = GetDecimalFromString(_depo.Text);
            Console.WriteLine($"[DEBUG] depoStart = {depoStart}");

            // decimal depoStart = GetDecimalFromString(_depo.Text);
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
            _canvas.Children.Clear();

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
            DrawSingleGraph(listEquity, Brushes.Black);           
        }

        private void DrawAllStrategies(List<Data> datas)
        {
            if (datas == null || datas.Count == 0) return; // выходим из метода при ошибке

            // Собираем все точки из всех стратегий
            List<List<decimal>> allEquities = new List<List<decimal>>();

            foreach ( var data in datas)
            {
                var equity = data.GetListEquity();
                if (equity != null && equity.Count > 0) 
                    allEquities.Add(equity);
            }
            if (allEquities.Count == 0) return;

            // Находим общий максимум и минимум
            decimal maxEquity = allEquities.SelectMany(x => x).Max();
            decimal minEquity = allEquities.SelectMany(x => x).Min();
            
            if (maxEquity == minEquity) minEquity = maxEquity - 1; // защита от деления на ноль, если все значения одинаковы

            double canvasWidth = _canvas.ActualWidth;// размер холста
            double canvasHeight = _canvas.ActualHeight;

            if (canvasHeight <= 1 || canvasWidth <= 1) return;

            double stepX = _canvas.ActualWidth / Math.Max(1, allEquities[0].Count - 1); // шаг по Х расстояние между точками
            double rangeY = (double)(maxEquity - minEquity); 
            double scaleY = rangeY > 0 ? canvasHeight / rangeY : 1;

            // Цвета для каждой стратегии
            Brush[] colors = { Brushes.Red, Brushes.Green, Brushes.Blue, Brushes.Orange };

            // Начальная позиция для текста
            double textY = 20; // отступ сверху
            double textX = 10; // отступ слева

            for (int i = 0; i < allEquities.Count; i++)
            {
                var points = new PointCollection();
                var listEquity = allEquities[i];

                for (int j = 0; j < listEquity.Count; j++)
                {
                    double x = j * stepX;
                    double y = canvasHeight - (double)(listEquity[j] - minEquity) * scaleY;
                    points.Add(new Point(x, y));
                }

                var polyline = new Polyline
                {
                    Stroke = colors[i % colors.Length], // циклиически (на каждой итерации цикла) выбираем цвет
                    StrokeThickness = 2,
                    Points = points
                };

                _canvas.Children.Add(polyline);

                // Добавляем текстовую метку
                TextBlock label = new TextBlock
                {
                    Text = datas[i].StrategyType.ToString(), // выводим назавние стратегии
                    Foreground = colors[i % colors.Length], // цвет как у линии
                    FontSize = 12,
                    FontWeight = FontWeights.Bold,
                };

                // Позиционируем текст рядом с последней точкой линии
                double lastX = (listEquity.Count - 1) * stepX;
                double lastY = canvasHeight - (double)(listEquity.Last() - minEquity) * scaleY;

                Canvas.SetLeft(label, lastX + 5); // немного справа от последней точки
                Canvas.SetTop(label, lastY - 15); // немного выше

                _canvas.Children.Add(label);
            }
        }

        private void DrawSingleGraph(List<decimal> listEquity, Brush color)
        {
            if (_canvas.ActualWidth == 0 || _canvas.ActualHeight == 0) return;

            int count = listEquity.Count;

            double canvasWidth = _canvas.ActualWidth;// размер холста
            double canvasHeight = _canvas.ActualHeight;
            
            if (canvasWidth <= 1 || canvasHeight <= 1) return;

            double stepX = canvasWidth / Math.Max(1, listEquity.Count - 1); // шаг по Х расстояние между точками
            decimal maxEquity = listEquity.Max(); // определяем максимум и минимум для масштабирования
            decimal minEquity = listEquity.Min();

            if (maxEquity == minEquity) minEquity = maxEquity - 1; // защита от деления на ноль, если все значения одинаковы            

            double rangeY = (double)(maxEquity - minEquity); // мaсштаб по Y
            double scaleY = rangeY > 0 ? canvasHeight / rangeY : 1;           
            

            var points = new PointCollection(); // создание конструктора для точки в canvas

            for (int i = 0; i < listEquity.Count; i++)
            {
                double x = i * stepX;

                double y = canvasHeight - (double)(listEquity[i] - minEquity) * scaleY;

                points.Add(new Point(x, y));                
            }

            var polyline = new Polyline  // создание линии
            {
                Stroke = color,
                StrokeThickness = 2,
                Points = points
            };
           
            _canvas.Children.Add(polyline);
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

        //private void GroupBox_SizeChanged(object sender, SizeChangedEventArgs e)
        //{
        //    _dataGrid.ItemsSource = datas;
        //}
    }
}