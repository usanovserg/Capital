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
using Capital.Entity;
using Capital.Enums;

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

        #region Fields ===================================================================

        List<StrategyType> _strategies = new List<StrategyType>() 
        {
                StrategyType.FIX,
                StrategyType.CAPITALIZATION,
                StrategyType.PROGRESS,
                StrategyType.DOWNGRADE
        };

        Random _random = new Random();

        private List<Data>? _lastCalculatedDatas;////

        #endregion

        #region Methods ===================================================================

        private void Init()
        {
            _comboBox.ItemsSource = _strategies;
           
           /// <summary>
           /// List<StrategyType> strategyTypes = new List<StrategyType>();
            
            ///    strategyTypes.Add(StrategyType.FIX);
            ///  strategyTypes.Add(StrategyType.CAPITALIZATION);
            ///strategyTypes.Add(StrategyType.PROGRESS);
            ///strategyTypes.Add(StrategyType.DOWNGRADE);

            ///_comboBox.ItemsSource = strategyTypes;
            /// </summary>
             _comboBox.SelectionChanged += _comboBox_SelectionChanged;
            _comboBox.SelectedIndex = 0;

            _depo.Text = "100000";
            _startLot.Text = "10";
            _take.Text = "300";
            _stop.Text = "100";
            _comiss.Text = "5";
            _percentProfit.Text = "30";
            _countTrades.Text = "1000";
            _minStartPercent.Text = "20";
            _go.Text = "5000";            
        }

        /// <summary>
        /// вызывается при смене стратегии в комбобокс
        /// перерисовывает график без перерасчета
        /// </summary>

        private void _comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_lastCalculatedDatas != null && _canvas.IsLoaded)
            {
                Draw(_lastCalculatedDatas);
            }
        }
        /// <summary>
        /// обработчик нажатия кнопки (Расситать)
        /// Выполняет рассчет и сохраняет результат
        /// </summary>
      
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _lastCalculatedDatas = Calculate(); // сохраняем результат

            Draw(_lastCalculatedDatas);        // рисуем график

        }
        /// <summary>
        /// Выполняем рассчет всех стратегий
        /// </summary>
        
        private List<Data> Calculate()
        {
            decimal depoStart = GetDecimalFromString(_depo.Text);
            int startLot = GetIntFromString(_startLot.Text);
            decimal take = GetDecimalFromString(_take.Text);
            decimal stop = GetDecimalFromString(_stop.Text);
            decimal comiss = GetDecimalFromString(_comiss.Text);
            decimal percentProfit = GetDecimalFromString(_percentProfit.Text);
            int countTrades = GetIntFromString(_countTrades.Text);
            decimal minStartPercent = GetDecimalFromString(_minStartPercent.Text);
            decimal go = GetDecimalFromString(_go.Text);

            List<Data> datas = new List<Data>();
            foreach (StrategyType type in _strategies)
            {
                datas.Add(new Data(depoStart, type));
            }

            int lotPersent = startLot;
            decimal percent = startLot* go*100/ depoStart;
            decimal multiply = take / stop;
            int lotProgress = CalculateLot(depoStart, minStartPercent, go);

            int lotDown = startLot;

            for (int i = 0; i < countTrades; i++)
            {
                int rnd = _random.Next(1, 100);
                if (rnd <= percentProfit)
                {
                    // Сделка прибыльная

                    // ============= 1 stratage ===============================
                    datas[0].ResultDepo += (take - comiss) * startLot;

                    // ============= 2 stratage ===============================

                    datas[1].ResultDepo += (take - comiss) * lotPersent;
           
                    int newLot = CalculateLot(datas[1].ResultDepo, percent, go);

                    if (lotPersent<newLot) lotPersent= newLot;

                    // ============= 3 stratage ===============================
                    datas[2].ResultDepo += (take - comiss) * lotProgress;
                    lotProgress = CalculateLot(depoStart, minStartPercent * multiply, go);

                    // ============= 4 stratage ===============================
                    datas[3].ResultDepo += (take - comiss) * lotDown;

                    lotDown = startLot;

                }
                else
                {
                    // Сделка убыточная
                    // ============= 1 stratage ===============================
                    datas[0].ResultDepo -= (stop + comiss) * startLot;
                    // ============= 2 stratage ===============================
                    datas[1].ResultDepo -= (stop + comiss) * lotPersent;

                    // ============= 3 stratage ===============================
                    datas[2].ResultDepo -= (stop + comiss) * lotProgress;
                    lotProgress = CalculateLot(depoStart, minStartPercent, go);

                    // ============= 4 stratage ===============================
                    datas[3].ResultDepo -= (stop + comiss) * lotDown;
                    lotDown /= 2;
                    if (lotDown == 0) lotDown = 1;

                }

            }
            _dataGrid.ItemsSource = datas;

            return datas;
        }
        /// <summary>
        /// отрисовка графика эквити ЛИНИЕЙ (не точками)
        /// </summary>
       
        private void Draw(List<Data> datas) 
        {
            // очищаем от предыдущего графика
            _canvas.Children.Clear(); 
            // защита от ошибок
            if (datas == null) return; ///  
            int index = _comboBox.SelectedIndex;
            if (index <0 || index >= datas.Count) return;///

            List<decimal> listEquity = datas[index].GetListEquity();

            if (listEquity == null || listEquity.Count ==0) return; ///

            // находим максимум и минимум для масштабирования
            int count = listEquity.Count;
            decimal maxEquity = listEquity.Max();   
            decimal minEquity = listEquity.Min();

            // защита от деления на ноль, если все значения одинаковые
            if (maxEquity == minEquity) ///
                minEquity = maxEquity - 1;  ///

            // int count = listEquity.Count;

            //  размер холста
            double canvasWidth = _canvas.ActualWidth;///
            double canvasHeight = _canvas.ActualHeight;///
            // шаг по Х расстояниие между точками
            double stepX = _canvas.ActualWidth / Math.Max(1,count -1);///
            // масштаб по  Y
            double rangeY = (double)(maxEquity - minEquity); ///
            double scaleY = rangeY > 0 ? canvasHeight / rangeY : 1; ///

            ///double koef = (double)(maxEquity - minEquity) / _canvas.ActualHeight;

            ///double x = 0;
            ///double y = 0;
            // создаем ломаннную линию один раз
            var polyline = new Polyline 
            {
                Stroke = Brushes.Black, // цвет линии
                StrokeThickness = 1.5   // толщина линии

            };
            // собираем точку графика
            var points = new PointCollection();///
            for (int i = 0; i< count; i++) 
            {
               double x = i * stepX;///
               double y = _canvas.ActualHeight - (double)(listEquity[i] - minEquity) * scaleY;///
                points.Add(new Point(x, y)); ///
                ///_canvas.Children.Add(polyline);

                ///y = _canvas.ActualHeight - (double)(listEquity[i] - minEquity) / koef;

                ////Ellipse ellipse = new Ellipse()
                ///{
                /// Width = 2,
                /// Height = 2,
                /// Stroke = Brushes.Black

                ///};
                ///Canvas.SetLeft(ellipse, x);
                ///Canvas.SetTop(ellipse, y);
                ///x += stepX;
            }
            // присваеваем точки линии 
            polyline.Points = points;
            // добавляем на холст только один раз - после цикла
            _canvas.Children.Add(polyline);

        ///if (_dataGrid == null)
        }
        /// <summary>
        /// обновляем график при изменениие размера окна, вызывается автоматически
        /// </summary>
        
        private void Canvas_SizeChanged(object sender, SizeChangedEventArgs e) 
        {
            if (_lastCalculatedDatas != null)
            {
                Draw(_lastCalculatedDatas);
            }
             
        }
        /// <summary>
        /// Расчет лота по текущему депозиту и проценту
        /// </summary>
        
        private int CalculateLot(decimal currentDepo,decimal percent, decimal go) 
        { 
            if (percent > 100)  percent = 100; 
            decimal lot = currentDepo/go/100*percent;
            ///return (int)lot;
            return (int)Math.Max(1,lot);/// лот не может быть  меньше 1
        
        }
        //decimal.TryParse(_depo.Text, out depoStart);

        /// <summary>
        /// Безопасное преобразование строки в decimal
        /// </summary>

        private decimal GetDecimalFromString(string str) 
        {
           return decimal.TryParse(str, out decimal result)? result:0;///
           /// if (decimal.TryParse(str,out decimal result)) return result;

            ///return 0;       
        }
        /// <summary>
        /// безопасное преобразование строки в INT
        /// </summary>

        private int GetIntFromString(string str)
        {
            return int.TryParse(str, out int result)? result:0;///
            ///if (int.TryParse(str, out int result)) return result;

            ///return 0;
        }



        #endregion

    }
}