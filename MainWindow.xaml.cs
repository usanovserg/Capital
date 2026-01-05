using Capital.Entity;
using Capital.Enums;
using System;
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


/*
 * 
В программе используется 4 стратегии:

1. Фиксированный лот. Торговля происходит всегда с одним и тем же лотом. То есть прибыль и убыток всегда неизменны.

2. Капитализация. Рабочий лот рассчитывается исходя из текущего депо. Если депо растёт, то лот увеличивается. 
Если депо уменьшается, то рабочий лот не изменяется.

3. Прогрессирующий. Если предыдущая сделка была прибыльной, то увеличиваем рабочий лот на соотношения тейка и стопа. 
(Пример: тейк 300, стоп 100, коэффициент увеличения лота равен 300/100 = 3).

4. Понижение. Здесь обратная стратегия - если текущая сделка была убыточной, то следующую совершаем в два раза меньшим 
объемом. Если получили прибыль, то лот сразу возвращается к первоначальному размеру.

*/


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


        #region Fields ====================================================================================

        List<StrategyType> _strategies = new List<StrategyType>()
        {
            StrategyType.FIX,
            StrategyType.CAPITALISATION,
            StrategyType.PROGRESS,
            StrategyType.DOWNGRADE,
            StrategyType.ALL
        };

        List<ColorsMy> _colors = new List<ColorsMy>()
        {
            ColorsMy.Red,
            ColorsMy.Green,
            ColorsMy.Blue, 
            ColorsMy.Yellow
        };

        Random _random = new Random();
                
        List<Data> datas;

        #endregion Fields




        #region Methods ====================================================================================

        // начальное заполнение для текст-боксов и подписка на изменение комбо-бокса
        private void Init()
        {
            _depo.Text = "100000";
            _startLot.Text = "10";
            _take.Text = "300";
            _stop.Text = "100";
            _commis.Text = "5";
            _percentProfit.Text = "30";
            _countTrades.Text = "1000";
            _minStartPersent.Text = "20";
            _go.Text = "5000"; 
            _comboBox.ItemsSource = _strategies;
            _comboBox.SelectionChanged += _comboBox_SelectionChanged;   // подписка на изменение комбо-бокса
            _comboBox.SelectedIndex = 0;            
        }



        // Обработчик: изменение комбо-бокса
        private void _comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // способ 1
            //ComboBox? comboBox = sender as ComboBox;
            // способ 2
            ComboBox comboBox = (ComboBox)sender;
            
            Draw(datas, comboBox.SelectedIndex);            
        }



        // Обработчик: изменения размера canvas (при изменении размера окна)
        private void _canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {            
            Draw(datas, _comboBox.SelectedIndex);
        }



        // обработчик: нажать на кнопку "Рассчитать"
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            List<Data> datas = Calculate();            
            Draw(datas, _comboBox.SelectedIndex);
        }



        // метод для кнопки "Рассчитать". Он возвращает список datas объектов класса Data 
        private List<Data> Calculate()
        {
            decimal depoStart = GetDecimalFromString(_depo.Text);
            int startLot = GetIntFromString(_startLot.Text);
            decimal take = GetDecimalFromString(_take.Text);
            decimal stop = GetDecimalFromString(_stop.Text);
            decimal comiss = GetDecimalFromString(_commis.Text);
            int countTrades = GetIntFromString(_countTrades.Text);                  // кол-во сделок
            decimal percProfit = GetDecimalFromString(_percentProfit.Text);         // процент прибыльных сделок
            decimal minStartPercent = GetDecimalFromString(_minStartPersent.Text);
            decimal go = GetDecimalFromString(_go.Text);
            // спец.переменные для цикла:
            int lotPercent = startLot;                              // для стратегии 2
            decimal percent = startLot * go * 100 / depoStart;      // для стратегии 2
            decimal multiply = take / stop;                         // для стратегии 3
            int lotProgress = CalculateLot(depoStart, minStartPercent, go); // для стратегии 3
            int lotDown = startLot;                                 // для стратегии 4

            // создали список datas, состоящий из объектов типа Data. Этот список потом засунем в _dataGrid.
            // Названия колонок в _dataGrid - это свойства объектов Data (потом я вручную их создал в XAML)
            datas = new List<Data>();

            // теперь заполним список datas новыми объектами:            
            foreach (StrategyType type in _strategies)
            {                
                datas.Add(new Data(depoStart, type));
            }            

            // цикл, который:
            // 1. создаёт рандомные сделки
            // 2. для каждой стратегии считает ResultDepo
            for (int i = 0; i < countTrades; i++)
            {
                int rnd = _random.Next(1, 100);     // эти цифры в процентах

                if (rnd <= percProfit)              // если сделка прибыльная
                {
                    // стратегия 1. С постоянным лотом
                    datas[0].ResultDepo += (take - comiss) * startLot;

                    // стратегия 2. Если депо увеличился, то мы увеличиваем рабочий лот. А если депо уменьшился, то лот мы НЕ понижаем, оставляем последний какой был
                    datas[1].ResultDepo += (take - comiss) * lotPercent;
                    int newLot = CalculateLot(datas[1].ResultDepo, percent, go);
                    if (lotPercent < newLot)    // если были убыточные сделки и депо уменьшился, то при след прибыльной сделке лот пересчитается и станет меньше. А мы этого не хотим:
                    {
                        lotPercent = newLot;    // т.е., оставляем большой лот. Наш лот может только повышаться
                    }

                    // стратегия 3. Если сделка прибыльная, то рискнём этой суммой и след лот увеличим в 3 раза (на эту сумму (типа хорошая стратегия))
                    datas[2].ResultDepo += (take - comiss) * lotProgress;
                    lotProgress = CalculateLot(depoStart, minStartPercent * multiply, go);

                    // стратегия 4. В случае убытка уменьшаем лот в 2 раза. В случае прибыли возвращаем к первоначальному. Как бы ни уменьшился, при первой положительной сразу в начальный
                    datas[3].ResultDepo += (take - comiss) * lotDown;
                    lotDown = startLot;     

                }
                else // если сделка убыточная
                {
                    // стратегия 1
                    datas[0].ResultDepo -= (stop + comiss) * startLot;

                    // стратегия 2
                    datas[1].ResultDepo -= (stop + comiss) * lotPercent;

                    // стратегия 3. Если убыток, то lotProgress возвращается к стандартному значению
                    datas[2].ResultDepo -= (stop + comiss) * lotProgress;
                    lotProgress = CalculateLot(depoStart, minStartPercent, go);

                    // стратегия 4. Если убыток, но понижаем лот в 2 раза
                    datas[3].ResultDepo -= (stop + comiss) * lotDown;
                    lotDown /= 2;       // т.к. lotDown типа int, то дробная часть отбросится. И он может стать равен 0
                    if (lotDown == 0)   // проверим, если 0, тогда исправим на 1
                        lotDown = 1;
                }
            }

            // в таблицу результатов _dataGrid записываем столбец datas.ResultDepo (по идее, запись надо вынести в отдельный метод)
            _dataGrid.ItemsSource = datas;

            return datas;
        }



        //// Метод - отрисовка на Canvas через Ellipse. Передаём список и индекс из комбо-бокса
        //private void Draw(List<Data> datas, int index)
        //{
        //    _canvas.Children.Clear();   // очистка канваса

        //    //int index = _comboBox.SelectedIndex;                      // выбрали индекс из комбо-бокса            
        //    List<decimal> listEquity = datas[index].GetListEquity();    // получили список listEquity для графика из [i] объекта Data
        //    int count = listEquity.Count;                               // в этом списке найдём кол-во точек (кол-во сделок)
        //    decimal maxEquity = listEquity.Max();                       // находим макс Equity в списке
        //    decimal minEquity = listEquity.Min();                       // находим мин Equity в списке
        //    double stepX = _canvas.ActualWidth / count;                 // шаг по Х = [текущая ширина канваса] / [кол-во точек]. Расстояние между точками по горизонтали X
        //    double koefY = (double)(maxEquity - minEquity) / _canvas.ActualHeight;      // коэф по Y = [размах между мин и макс] / [текущую высоту канваса]
        //    //double koefY = _canvas.ActualHeight / (double)(maxEquity - minEquity);    // мой способ мне больше нравится - по аналогии с X
        //    double x = 0;                                               // текущая координата X
        //    double y = 0;                                               // текущая координата Y

        //    for (int i = 0; i < count; i++)
        //    {
        //        //y = _canvas.ActualHeight - (double)(listEquity[i] - minEquity) / koefY;       // преобразование, т.к. отсчёт по оси Y сверху вниз, то мы "отзеркаливаем" график относительно Y
        //        //y = _canvas.ActualHeight - (double)(listEquity[i] - minEquity) * koefY;       // мой способ мне больше нравится - по аналогии с X

        //        y = (double)(listEquity[i] - minEquity) / koefY;

        //        Ellipse ellipse = new Ellipse() // в WPF нет точки, поэтому взяли эллипс
        //        {
        //            Width = 2,
        //            Height = 2,
        //            Stroke = Brushes.Black      // цвет
        //        };
        //        Canvas.SetLeft(ellipse, x);
        //        //Canvas.SetTop(ellipse, y);    // преобразование нужно делать относительно отступа сверху, т.к. здесь ось Y сверху
        //        Canvas.SetBottom(ellipse, y);   // преобразование НЕ нужно делать относительно отступа снизу. Здесь ось Y уже снизу
        //        _canvas.Children.Add(ellipse);

        //        x += stepX;
        //    }
        //}



        //// Метод - отрисовка на Canvas через Line. Передаём список и индекс из комбо-бокса
        //private void Draw(List<Data> datas, int index)
        //{
        //    _canvas.Children.Clear();

        //    List<decimal> listEquity = datas[index].GetListEquity();
        //    int count = listEquity.Count;
        //    if (count < 2) return; // Нужно хотя бы 2 точки, чтобы нарисовать линию

        //    decimal maxEquity = listEquity.Max();
        //    decimal minEquity = listEquity.Min();
        //    double stepX = _canvas.ActualWidth / (count - 1); // чтобы последняя точка была в конце
        //    double koefY = (double)(maxEquity - minEquity) / _canvas.ActualHeight;

        //    double prevX = 0;
        //    double prevY = (double)(listEquity[0] - minEquity) / koefY;

        //    for (int i = 1; i < count; i++)
        //    {
        //        double x = i * stepX;
        //        double y = (double)(listEquity[i] - minEquity) / koefY;

        //        Line line = new Line()
        //        {
        //            X1 = prevX,
        //            Y1 = _canvas.ActualHeight - prevY, // инвертируем Y
        //            X2 = x,
        //            Y2 = _canvas.ActualHeight - y,     // инвертируем Y
        //            Stroke = Brushes.Black,
        //            StrokeThickness = 1
        //        };

        //        _canvas.Children.Add(line);

        //        prevX = x;
        //        prevY = y;
        //    }
        //}



        // Метод - отрисовка на Canvas через Polyline. Передаём список и индекс из комбо-бокса
        private void Draw(List<Data> datas, int index)
        {
            _canvas.Children.Clear();                       // очистить от предыдущих
            if (datas == null) return;                      // проверка на ошибку

            List<decimal> listEquity = datas[index].GetListEquity();
            int count = listEquity.Count;
            if (count < 2) return;                          // Нужно хотя бы 2 точки, чтобы нарисовать линию

            decimal maxEquity = listEquity.Max();
            decimal minEquity = listEquity.Min();
            double stepX = _canvas.ActualWidth / (count - 1);
            double koefY = (double)(maxEquity - minEquity) / _canvas.ActualHeight;

            PointCollection points = new PointCollection(); // создали коллекцию точек            
            for (int i = 0; i < count; i++)                 // заполним эту коллекцию points
            {
                double x = i * stepX;
                double y = _canvas.ActualHeight - (double)(listEquity[i] - minEquity) / koefY; // инвертируем Y
                points.Add(new Point(x, y));                // добавляем точку в коллекцию
            }

            Polyline polyline = new Polyline()
            {
                Points = points,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };

            _canvas.Children.Add(polyline);
        }



        // спец. метод для стратегии 2. percent - это % лота от первоначального депо (процентное соотнош. лот/депо должно соблюдаться)
        private int CalculateLot(decimal currentDepo, decimal percent, decimal go)
        {
            if (percent > 100)
            {
                percent = 100;
            }
            decimal lot = currentDepo / go / 100 * percent;
            return (int)lot;
        }



        private decimal GetDecimalFromString(string str)
        {
            if (decimal.TryParse(str, out decimal result))
            {
                return result;
            }
            return 0;
        }



        private int GetIntFromString(string str)
        {
            if (int.TryParse(str, out int result))
            {
                return result;
            }
            return 0;
        }



        #endregion Methods

        
    }   
}       