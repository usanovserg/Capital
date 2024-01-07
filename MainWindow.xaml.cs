using Capital.Entity;
using Capital.Enums;
using System;
using System.Reflection;
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

        #region Fields ====================================

        List<StrategyType> _strategies = new List<StrategyType>()
        {
            StrategyType.FIX,
            StrategyType.CAPITALIZATION,
            StrategyType.PROGRESS,
            StrategyType.DOWNGRADE
        };

        /* а такое присвоение работает в методах:
        _strategies.Add(StrategyType.FIX); 
        _strategies.Add(StrategyType.CAPITALIZATION);
        _strategies.Add(StrategyType.PROGRESS);
        _strategies.Add(StrategyType.DOWNGRADE);
        */
        List<Data>? data { get; set; }
        Random _random = new Random();

        #endregion

        #region Methods ==================================

        private void Init()
        {
            _comboBox.ItemsSource = _strategies;
            _comboBox.SelectedIndex = 0;
            _depo.Text = "100000";
            _startLot.Text = "10";
            _take.Text = "300";
            _stop.Text = "100";
            _commiss.Text = "5";
            _countTrades.Text = "200";
            _percentProfit.Text = "30";
            _go.Text = "5000";
            _minStartPercent.Text = "20";
            _comboBox.SelectionChanged += _comboBox_SelectionChanged;
            _canvas.SizeChanged += _canvas_SizeChanged;

        }

        private void _canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            int index = _comboBox.SelectedIndex;
            if (data != null)
            {
                List<decimal> _data = GetData(data, index);
                Drawn(_data);
            }
            
        }

        private void _comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            int index = comboBox.SelectedIndex;
            if (data != null)
            {
                List<decimal> _data = GetData(data, index);
                Drawn(_data);
            }

            // MessageBox.Show(index.ToString());
            
            //string messageBoxText = "Do you want to save changes?";
            //string caption = "Word Processor";
            //MessageBoxButton button = MessageBoxButton.YesNoCancel;
            //MessageBoxImage icon = MessageBoxImage.Warning;
            //MessageBoxResult result;
            //result = MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            data = Calculate();
            int index = _comboBox.SelectedIndex;
            List<decimal> _data = GetData(data, index);
            Drawn(_data);
        }

        private List<Data> Calculate()
        {
            decimal depoStart = GetDecimalFromString(_depo.Text);
            int startLot = GetIntFromString(_startLot.Text);
            decimal take = GetDecimalFromString(_take.Text);
            decimal stop = GetDecimalFromString(_stop.Text);
            decimal commiss = GetDecimalFromString(_commiss.Text);
            int countTrades = GetIntFromString(_countTrades.Text);
            decimal percProfit = GetDecimalFromString(_percentProfit.Text);
            decimal go = GetDecimalFromString(_go.Text);
            decimal minStartPercent = GetDecimalFromString(_minStartPercent.Text);

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
                if (rnd <= percProfit)
                {
                    //сделка прибыльная
                    
                    //-----------------Strategy 1----------------------
                    datas[0].ResultDepo += (take - commiss) * startLot;

                    //-----------------Strategy 2----------------------
                    datas[1].ResultDepo += (take - commiss) * lotPercent;
                    int newLot = CalculateLot(datas[1].ResultDepo, percent, go);
                    if (lotPercent < newLot) lotPercent = newLot;

                    //-----------------Strategy 3----------------------
                    datas[2].ResultDepo += (take - commiss) * lotProgress;
                    lotProgress = CalculateLot(depoStart, minStartPercent * multiply, go);

                    //-----------------Strategy 4----------------------
                    datas[3].ResultDepo += (take - commiss) * lotDown;
                    lotDown = startLot;
                }
                else
                {
                    //сделка убыточная

                    //-----------------Strategy 1----------------------
                    datas[0].ResultDepo -= (stop + commiss) * startLot;

                    //-----------------Strategy 2----------------------
                    datas[1].ResultDepo -= (stop + commiss) * lotPercent;

                    //-----------------Strategy 3----------------------
                    datas[2].ResultDepo -= (stop + commiss) * lotProgress;
                    lotProgress = CalculateLot(depoStart, minStartPercent, go);

                    //-----------------Strategy 4----------------------
                    datas[3].ResultDepo -= (stop + commiss) * lotDown;
                    lotDown /= 2;
                    if (lotDown == 0) lotDown = 1;
                }
            }

            _dataGrid.ItemsSource = datas;
            return datas;
        }

        private int CalculateLot(decimal currentDepo, decimal percent, decimal go)
        {
            if (percent > 100) percent = 100;
            decimal lot = currentDepo / go / 100 * percent;
            return (int)lot;
        }

        private decimal GetDecimalFromString(string str)
        {
            if (decimal.TryParse(str, out decimal result)) return result;
            return 0;
        }

        private int GetIntFromString(string str)
        {
            if (int.TryParse(str, out int result)) return result;
            return 0;
        }

        private void Drawn(List<decimal> listEquity)
        {
            _canvas.Children.Clear();
            
            int count = listEquity.Count;
            decimal maxEquity = listEquity.Max();
            decimal minEquity = listEquity.Min();
            decimal depo = GetDecimalFromString(_depo.Text);

            //decimal result = max - min;
            //MessageBox.Show($"Стратегия {i} Max = {max}, Min = {min}, Max - Min = {result.ToString()}");\

            double stepX = (_canvas.ActualWidth) / count;
            double koef = (_canvas.ActualHeight) / (double)(maxEquity - minEquity);

            double x = 0;
            double y = (double)(maxEquity - depo) * koef;
            double x2 = 0;
            double y2;
 
            for (int i = 1; i < count; i++)
            {
                 Line line = new Line()
                {
                    StrokeThickness = 2,
                    Stroke = Brushes.Red
                };

                line.X1 = x;
                line.Y1 = y;

                x2 += stepX;
                y2 = (double)(maxEquity - listEquity[i]) * koef;

                line.X2 = x2;
                line.Y2 = y2;

                _canvas.Children.Add(line);

                x = x2;
                y = y2;
            }
            double elipse_x = 0;
            double elipse_y = 0;
            for (int i = 0; i < count; i++)
            {
                elipse_y = (double)(listEquity[i] - minEquity) * koef;

                Ellipse ellipse = new Ellipse()
                {
                    Width = 5,
                    Height = 5,
                    Stroke = Brushes.Black
                };

                Canvas.SetLeft(ellipse, elipse_x-2);
                Canvas.SetBottom(ellipse, elipse_y-2);

                _canvas.Children.Add(ellipse);

                elipse_x += stepX;

            }
            for (int i = 0; i < _canvas.ActualWidth; i++)
            {

                Ellipse zeroline = new Ellipse()
                {
                    Width = 2,
                    Height = 2,
                    Stroke = Brushes.Green
                };
                Canvas.SetLeft(zeroline, i);
                Canvas.SetBottom(zeroline, (double)(depo - minEquity)*koef);
                
                _canvas.Children.Add(zeroline);
            }
   
        }
        private List<decimal> GetData(List<Data> list, int i)
        {
            List<decimal> listData = list[i].GetListEquity();
            return listData;
        }
         #endregion

        
    }
}

