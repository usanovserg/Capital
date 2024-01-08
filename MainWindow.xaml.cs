using Capital.Entity;
using Capital.Enums;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
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

        #region=============================================================Fields==================================================

        List<StrategyType> _strategies = new List<StrategyType>()
        {
                StrategyType.Fix,

                StrategyType.Capitalisation,

                StrategyType.Progress,

                StrategyType.Downgrade

        };

        /// <summary>
        /// набор цветов для StrategyType
        /// </summary>
        List<string?> _colorString = new List<string?>() 
        {
            "Black",
            "Blue",
            "Green",
            "Red"
        };

        /// <summary>
        /// Будет запоминать индексы выбранных к показу графиков для данного Расчета
        /// </summary>
        List<int> _index = new List<int>();

        /// <summary>
        /// данные - точки для построения графика
        /// </summary>
        List<Data> datas = new List<Data>();

        Random _random = new Random();

        #endregion

        #region===========================================================Methods====================================================

        private void Init()
        {
            int i_color = 0;

            foreach (var strategy in _strategies)
            {
                TextBlock _textBlock = new TextBlock();

                _textBlock.Text = strategy.ToString();

                SolidColorBrush? _colorBrush = (SolidColorBrush?)new BrushConverter().ConvertFromString(_colorString[i_color]!.ToString());

                _textBlock.Foreground = _colorBrush;

                _comboBox.Items.Add(_textBlock);

                i_color++;

            }

            //_comboBox.ItemsSource = _strategies;

            _comboBox.SelectedIndex = 0;

            _comboBox.SelectionChanged += _comboBox_SelectionChanged;

            _depo.Text = "100000";

            _startLot.Text = "10";

            _take.Text = "300";

            _stop.Text = "100";

            _commiss.Text = "5";

            _countTrades.Text = "1000";

            _percentProfit.Text = "30";

            _go.Text = "5000";

            _minStartPercent.Text = "20";

        }

        private void _comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //ComboBox? comboBox = sender as ComboBox; //_comboBox передан в метод через универсальный объект sender, "вытаскиваем" его в comboBox, этот способ учитывает, что возможно не передан объект, т.е. Null

            ComboBox comboBox = (ComboBox)sender; // это второй способ, но в случае Null будет исключение

            int index = comboBox.SelectedIndex;

            if (!_index.Contains(index)) _index.Add(index); //добавляем индекс графика к показу

            if (datas.Count != 0)
                Draw(index); // (datas) //если данные уже были расчитаны, то прорисовка графика

        }

        private void Button_Clear_Click(object sender, RoutedEventArgs e)
        {
            _index.Clear();

            ChartClear();

        }


        /// <summary>
        /// Кнопка Рассчитать
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            datas.Clear();

            datas = Calculate();

            _index.Clear();

            ChartClear();

            _index.Add(_comboBox.SelectedIndex); //индекс текущиго выбранного графика добавляем к списку отображаемых

            Draw(_comboBox.SelectedIndex); //(datas)
        }

        /// <summary>
        /// Очистка графика
        /// </summary>
        private void ChartClear()
        {
            _canvas.Children.Clear();

            //_canvas.Children.Add(_buttonClear);
        }

        private List<Data> Calculate()
        {
            decimal depoStart = GetDecimalFromString(_depo.Text);

            int startLot = GetIntFromString(_startLot.Text);

            decimal take = GetDecimalFromString(_take.Text);

            decimal stop = GetDecimalFromString(_stop.Text);

            decimal commiss = GetDecimalFromString(_commiss.Text);

            int countTrades = GetIntFromString(_countTrades.Text);

            decimal percentProfit = GetDecimalFromString(_percentProfit.Text);

            decimal minStartPercent = GetDecimalFromString(_minStartPercent.Text);

            decimal go = GetDecimalFromString(_go.Text);

            List<Data> datas = new List<Data>();

            foreach (StrategyType type in _strategies)
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

                if (rnd <= percentProfit)
                {
                    //Сделка прибыльная

                    //стратегия FIX================================================

                    datas[0].ResultDepo += (take - commiss) * startLot;

                    //стратегия CAPITALIZATION================================================

                    datas[1].ResultDepo += (take - commiss) * lotPercent;

                    int newLot = CalculateLot(datas[1].ResultDepo, percent, go);

                    if (lotPercent < newLot)
                        lotPercent = newLot;

                    //стратегия PROGRESS================================================

                    datas[2].ResultDepo += (take - commiss) * lotProgress;

                    lotProgress = CalculateLot(depoStart, minStartPercent * multiply, go);

                    //стратегия DOWNGRADE===============================================

                    datas[3].ResultDepo += (take - commiss) * lotDown;

                    lotDown = startLot;
                }
                else
                {
                    //Сделка убыточная

                    //стратегия FIX================================================

                    datas[0].ResultDepo -= (stop + commiss) * startLot;

                    //стратегия CAPITALIZATION================================================

                    datas[1].ResultDepo -= (stop + commiss) * lotPercent;


                    //стратегия PROGRESS================================================

                    datas[2].ResultDepo -= (stop + commiss) * lotProgress;

                    lotProgress = CalculateLot(depoStart, minStartPercent, go);

                    //стратегия DOWNGRADE================================================

                    datas[3].ResultDepo -= (stop + commiss) * lotDown;

                    lotDown /= 2;

                    if (lotDown == 0)
                        lotDown = 1;

                }
            }

            _dataGrid.ItemsSource = datas;

            return datas;

        }

        private int CalculateLot(decimal currentDepo, decimal percent, decimal go)
        {
            if (percent > 100)
                percent = 100;

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

        /// <summary>
        /// Отрисовка графика 
        /// </summary>
        private void Draw(int index) 
        {            
            //int index = _comboBox.SelectedIndex;

            List<decimal> listEquity = datas[index].GetListEquity();

            int count = listEquity.Count;

            decimal maxEquity = listEquity.Max();

            decimal minEquity = listEquity.Min();

            double stepX = _canvas.ActualWidth / count;

            double koef = ((double)maxEquity - (double)minEquity) / _canvas.ActualHeight; // коэф для шага по оси Y

            double x = 0;

            double y = 0;

            Polyline polyline = new Polyline();

            polyline.Points = new PointCollection();

            switch (index)
            {
                case 0:

                    polyline.Stroke = Brushes.Black;

                    break;

                case 1:

                    polyline.Stroke = Brushes.Blue;

                    break;

                case 2:

                    polyline.Stroke = Brushes.Green;

                    break;

                case 3:

                    polyline.Stroke = Brushes.Red;

                    break;
            }

            for (int i = 0; i < count; i++)
            {
                y = _canvas.ActualHeight - ((double)listEquity[i] - (double)minEquity) / koef; //в Canvas ось X считается слева-направо, а ость Y - сверху-вниз(!!!), нографик рисовать снизу-вверх, поэтому такое вычисление Y

                /*Ellipse ellipse = new Ellipse() //одна точка графика
                {
                    Width = 2,

                    Height = 2,

                    Stroke = Brushes.Black

                };

                /*Canvas.SetLeft(ellipse, x);

                Canvas.SetTop(ellipse, y);

                _canvas.Children.Add(ellipse); //в children расположены все элементы, которые мы размещаем на Canvas*/


                polyline.Points.Add(new Point(x, y));
                
                x += stepX;
            }

            _canvas.Children.Add(polyline);

        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {

            if (datas.Count != 0)
            {
                ChartClear();

                foreach (int i in _index ) Draw(i);
            }
                
        }
        
        #endregion = Methods =

        
    }
}