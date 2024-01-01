using Capital.Entity;
using Capital.Enams;
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

        #region===============================Fields=======================================

        List<StrategyType> _strategies = new List<StrategyType>()
        {
            StrategyType.FIX,
            StrategyType.CAPITALIZATION,
            StrategyType.PROGRESS,
            StrategyType.DOWNGRADE
        };

        Random _random = new Random();

        List<Data> datas = new List<Data>();

        #endregion

        #region===============================Methods=======================================

        private void Init() // метод инициализации
        {
            _comboBox.ItemsSource = _strategies; // в комбоБокс через свойство ItemsSource добавляем список инамов 

            _comboBox.SelectionChanged += _combobox_SelectionChanged; // метод _combobox_SelectionChanged подписываем на событие SelectionChanged, событие вызывается изменениеми в комбобоксе
            _comboBox.SelectedIndex = 0; // Начальное значение комбобокса по индексу инама

            _depo.Text = "100000"; // Начальные значения текстбоксов 
            _startLot.Text = "10";
            _take.Text = "300";
            _stop.Text = "100";
            _comiss.Text = "5";
            _countTrades.Text = "1000";
            _percentProfit.Text = "30";
            _go.Text = "30";
            _minStartPercent.Text = "20";
        }

        private void _combobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender; // вынимаем комбобокс ранее переданный в object sender. В переменной comboBox и локальной переменной _comboBox сейчас находится ссылка на объект comboBox

            int index = comboBox.SelectedIndex; // в переменную index присваиваем значение индекса инама выбранного в комбобоксе

            if (datas.Count != 0) // проверка на наличие расчета стратегий
            {
                Draw(); // вызываем метод для отрисовки графика другого значения комбоБокса
            }




        }
        private void Button_Click(object sender, RoutedEventArgs e) 
        {
            datas = Calculete(); // при нажатии кнопки Расчитать вызывается метод Calculete

            Draw(); // вызываем метод для отрисовки графика первоначального значения комбоБокса
        }

        private List<Data> Calculete() 
        {
            decimal depoStart = GetDecimalFromString(_depo.Text); // преобразовываем строку через метод GetDecimalFromString
            int startLot = GetIntFromString(_startLot.Text); // преобразовываем строку через метод GetIntFromString
            decimal take = GetDecimalFromString(_take.Text); // итд
            decimal stop = GetDecimalFromString(_stop.Text);
            decimal comiss = GetDecimalFromString(_comiss.Text);
            int countTrades = GetIntFromString(_countTrades.Text);
            decimal percProfit = GetDecimalFromString(_percentProfit.Text);
            decimal minStartPercent = GetDecimalFromString(_minStartPercent.Text);
            decimal go = GetDecimalFromString(_go.Text);

            List<Data> datas = new List<Data>();  

            foreach (StrategyType type in _strategies) 
            {
                datas.Add(new Data(depoStart, type)); // создаем четыри объекта типа Data и добавляем их в лист datas
                //Data data = new Data(startLot, type); // анологичная запись
                //datas.Add(data);                      // datas.Add(new Data(depoStart, type));
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
                    // Сделка прибыльная

                    //================ 1 strategy ===============
                    datas[0].ResultDepo += (take - comiss) * startLot; 

                    //================ 2 strategy ===============

                    datas[1].ResultDepo += (take - comiss) * lotPercent; 

                    int newlot = CalculateLot(datas[1].ResultDepo, percent, go); 

                    if (lotPercent < newlot) lotPercent = newlot; 

                    //================ 3 strategy ===============

                    datas[2].ResultDepo += (take - comiss) * lotProgress; 

                    lotProgress = CalculateLot(depoStart, minStartPercent * multiply, go); 

                    //================ 4 strategy ===============

                    datas[3].ResultDepo += (take - comiss) * lotDown; 

                    lotDown = startLot; 
                }
                else
                {
                    // Сделка убыточная
                    //================ 1 strategy ===============
                    datas[0].ResultDepo -= (stop + comiss) * startLot; 

                    //================ 2 strategy ===============

                    datas[1].ResultDepo -= (stop + comiss) * lotPercent; 

                    //================ 3 strategy ===============

                    datas[2].ResultDepo -= (stop + comiss) * lotProgress; 

                    lotProgress = CalculateLot(depoStart, minStartPercent * multiply, go); 

                    //================ 4 strategy ===============

                    datas[3].ResultDepo -= (stop + comiss) * lotDown; 

                    lotDown /= 2; //

                    if (lotDown == 0) lotDown = 1; 
                }
            }

            _dataGrid.ItemsSource = datas; // в датаГрид через свойство ItemsSource помещаем лист (объект) datas
             
            return datas;

        }

        private void Draw() 
        {
            _canvas.Children.Clear(); // очищаем канвас перед новым расчетом

            int index = _comboBox.SelectedIndex; // в index присваиваем значение текущего комбоБокса

            List<decimal> listEquity = datas[index].GetListEquity();

            int count = listEquity.Count; // из списка listEquity достаем кол-во элементов списка
            decimal maxEquity = listEquity.Max();
            decimal minEquity = listEquity.Min();

            double stepX = _canvas.ActualWidth / count; // вычисляем шаг Х относительно ширины кансваса
            double koef = (double)(maxEquity - minEquity) / _canvas.ActualHeight;

            double x = 0; // текущая координата Х
            double y = 0; // текущая координата Y

            double lastX = 0; // прошлая координата Х

            double lastY = 0; // прошлая координата Y

            for (int i = 0; i < count; i++) // перебераем значения листа listEquity
            {
                y = _canvas.ActualHeight - (double)(listEquity[i] - minEquity) / koef;

                //Ellipse ellipse = new Ellipse()
                //{
                //    Width = 2, // ширина элипса
                //    Height = 2, // высота элипса
                //    Stroke = Brushes.Black
                //};

                Line line = new Line();
                {
                    line.X1 = x;
                    line.Y1 = y;
                    line.X2 = lastX;
                    line.Y2 = lastY;
                    lastX = x;
                    lastY = y;
                    line.Stroke = Brushes.Black;
                };

                //Canvas.GetLeft(line); // расположение слева // с ними и без работает одинаково ??????????
                //Canvas.GetTop(line); // расположение снизу

                _canvas.Children.Add(line); // помещаем линию на канвас


                x += stepX; // расчет координаты Х для текщего значения цикла 
            }


        }

        private int CalculateLot(decimal currentDepo, decimal percent, decimal go) 
        {
            if (percent > 100) { percent = 100; } 

            decimal lot = currentDepo / go / 100 * percent; 

            return (int)lot; 
        }

        private decimal GetDecimalFromString(string str) // метод преобразовывающий строку в decimal
        {
            if (decimal.TryParse(str, out decimal result)) return result; // проверка качественного преобразования в decimal и возврат результата

            return 0; // если преобразование не удачное возвращаем ноль
        }

        private int GetIntFromString(string str) // метод преобразовывающий строку в int
        {
            if (int.TryParse(str, out int result)) return result; // проверка качественного преобразования в int и возврат результата

            return 0; // если преобразование не удачное возвращаем ноль
        }

        #endregion
    }
}