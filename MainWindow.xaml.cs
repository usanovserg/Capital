using Capital.Entity;
using Capital.Enums;
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
        public MainWindow()     // конструктор
        {
            InitializeComponent();      // перевод кода Xaml в код C#

            Init();             // вызов метода, присваивающего начальные значения

            this.SizeChanged += MainWindow_SizeChanged;     // подписка на изменение размера окна
        }

        #region Fields =====================

        List<StrategyType> _strategies = new List<StrategyType>()   // создаем список вариантов управления капиталом
        {
            StrategyType.FIX,
            StrategyType.CAPITALIZATION,
            StrategyType.PROGRESS,
            StrategyType.DOWNGRADE
        };

        Random _random = new Random();

        List<Data> datas;

        bool _isCalculate = false;

        CurrentWindow currentWindow = new CurrentWindow();

        #endregion

        #region Methods =====================

        private void Init()     // метод присваивает начальные значения
        {
            _depo.Text = "100000";
            _startLot.Text = "10";
            _take.Text = "300";
            _stop.Text = "100";
            _comiss.Text = "5";
            _countTrades.Text = "1000";
            _percentProfit.Text = "30";
            _go.Text = "5000";
            _minStartPercent.Text = "20";

            _comboBox.ItemsSource = _strategies;    // заполнение списка выбора комбо-бокс

            _comboBox.SelectionChanged += _comboBox_SelectionChanged;   // подписка на изменение комбо-бокс
            _comboBox.SelectedIndex = 0;        // начальное значение комбо-бокс будет FIX
        }

        #region Event Handlers =====================

        private void _comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) // обработка изменения
        {
            ComboBox? comboBox = sender as ComboBox;    // выделить переданный объект комбо-бокс

            int index = comboBox.SelectedIndex;     // значение индекса в списке комбо-бокс

            if (_isCalculate) Draw(datas);
        }

        private void Button_Click(object sender, RoutedEventArgs e)     // обработчик нажатия кнопки "Рассчитать"
        {
            datas = Calculate();     // получаем список с результатами управления капиталом

            Draw(datas);        // передаем список с результатами управления капиталом на отрисовку графика
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)  // обработчик изменения размера окна
        {
            if (this.ActualHeight >= (12 * 30 + 350))
                currentWindow.actualHeigh = this.ActualHeight - (12 * 30 + 2 * 10 + 40);
            else currentWindow.actualHeigh = this.ActualHeight - (12 * 30 + 2 * 10);

            if (this.ActualWidth > (200))
                currentWindow.actualWidth = this.ActualWidth - (2 * 10 + 18);
            else currentWindow.actualWidth = this.ActualWidth - 10;

            if (_isCalculate) Draw(datas);
        }

        #endregion

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

            List<Data> datas = new List<Data>();        // создать список, в котором будут данные управлением капиталом

            foreach (StrategyType type in _strategies)  // перебор всех вариантов управления капиталом
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
                    // сделка прибыльная

                    // ===== 1 strategy =====
                    datas[0].ResultDepo += (take - comiss) * startLot;

                    // ===== 2 strategy =====
                    datas[1].ResultDepo += (take - comiss) * lotPercent;
                    int newLot = CalculateLot(datas[1].ResultDepo, percent, go);
                    if (lotPercent < newLot) lotPercent = newLot;

                    // ===== 3 strategy =====
                    datas[2].ResultDepo += (take - comiss) * lotProgress;
                    lotProgress = CalculateLot(depoStart, minStartPercent * multiply, go);

                    // ===== 4 strategy =====
                    datas[3].ResultDepo += (take - comiss) * lotDown;
                    lotDown = startLot;

                }
                else
                {
                    // сделка убыточная

                    // ===== 1 strategy =====
                    datas[0].ResultDepo -= (stop + comiss) * startLot;

                    // ===== 2 strategy =====
                    datas[1].ResultDepo -= (stop + comiss) * lotPercent;

                    // ===== 3 strategy =====
                    datas[2].ResultDepo -= (stop + comiss) * lotProgress;
                    lotProgress = CalculateLot(depoStart, minStartPercent, go);

                    // ===== 4 strategy =====
                    datas[3].ResultDepo -= (stop + comiss) * lotDown;
                    lotDown /= 2;
                    if (lotDown == 0) lotDown = 1;
                }
            }

            _dataGrid.ItemsSource = datas;      // помещаем в _dataGrid данные управлением капиталом

            _isCalculate = true;

            return datas;
        }

        private void Draw(List<Data> datas)
        {
            _canvas.Children.Clear();       // очистка графика

            int index = _comboBox.SelectedIndex;    // получаем индекс выбранного типа управления капиталом

            List<decimal> listEquity = datas[index].GetListEquity();    // получаем данные по выбранному типу управления

            int count = listEquity.Count;           // кол-во сохраненных сделок
            decimal maxEquity = listEquity.Max();   // максимальное полученное значение
            decimal minEquity = listEquity.Min();   // минимальное полученное значение

            /*
                        // координаты для эллипса
                        double x = 0;   // координата точки по Х
                        double y = 0;   // координата точки по Y
            */
            //double stepX = _canvas.ActualWidth / count; // шаг по х= текущую ширину графика / кол-во сделок
            //double koef = (double)(maxEquity - minEquity) / _canvas.ActualHeight;   // шаг по y

            // координаты для линии
            double x1 = 0;   // координата первой точки по Х
            double y1 = currentWindow.actualHeigh;   // координата первой точки по Y
            double x2 = 0;   // координата второй точки по Х
            double y2 = 0;   // координата второй точки по Y

            double stepX = currentWindow.actualWidth / count; // шаг по х= текущую ширину графика / кол-во сделок
            double koef = (double)(maxEquity - minEquity) / currentWindow.actualHeigh;   // шаг по y

            for (int i = 0; i < count; i++)     // перебираем listEquity
            {
                /*              // эллипс  
                                y = _canvas.ActualHeight - (double)(listEquity[i] - minEquity) / koef;

                                Ellipse ellipse = new Ellipse() // создаем объект эллипс (подобие точки - круг)
                                {
                                    Width = 2,                  // ширина эллипса
                                    Height = 2,                 // высота эллипса
                                    Stroke = Brushes.Black      // цвет эллипса
                                };

                                Canvas.SetLeft(ellipse, x);     // задаем расположение эллипса по Х
                                Canvas.SetTop(ellipse, y);      // задаем расположение эллипса по Y

                                _canvas.Children.Add(ellipse);  // помещаем эллипс на Canvas

                                x += stepX;
                */
                // линия
                x2 += stepX;
                y2 = currentWindow.actualHeigh - (double)(listEquity[i] - minEquity) / koef;

                Line line = new Line()          // создаем объект линия
                {
                    X1 = x1,                    // координата первой точки по Х
                    Y1 = y1,                    // координата первой точки по Y
                    X2 = x2,                    // координата второй точки по Х
                    Y2 = y2,                    // координата второй точки по Y
                    Stroke = Brushes.Black,     // цвет линии
                };

                _canvas.Children.Add(line);     // помещаем линию на Canvas

                x1 = x2;
                y1 = y2;
            }
        }

        private int CalculateLot(decimal currentDepo, decimal percent, decimal go)
        {
            if (percent > 100) { percent = 100; }

            decimal lot = currentDepo / go / 100 * percent;

            return (int)lot;
        }

        private decimal GetDecimalFromString(string str)    // преобразует строку в decimal
        {
            if (decimal.TryParse(str, out decimal result)) return result;

            return 0;
        }

        private int GetIntFromString(string str)        // преобразует строку в int
        {
            if (int.TryParse(str, out int result)) return result;

            return 0;
        }

        #endregion
    }
}