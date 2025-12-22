using Capital.Entity;
using Capital.Enums;
using System;
using System.Diagnostics;
using System.Globalization;
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
using System.Xml.Linq;

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

        List<StrategyType> _strategies = new List<StrategyType>() 
        { 
            StrategyType.FIX,
            StrategyType.CAPITALIZATION,
            StrategyType.PROGRESS,
            StrategyType.DOWNGRADE,
            StrategyType.ALL

        };

        List<Data> datas = new List<Data>();

        Random _random =new Random();

        #endregion

        #region Methods ===========================================

        private void Init()
        {
            //_comBox.ItemsSource = _strategies;
            
            foreach (StrategyType type in _strategies)
            {
                _comBox.Items.Add(type.ToString());
            }

            //_comBox.Items.Add("Все стратегии");
            _comBox.SelectionChanged += _comBox_SelectionChanged;
            _canvas.SizeChanged += _canvas_SizeChanged; ;
            _comBox.SelectedIndex = 0;
            
            _depo.Text = "100000";
            _startLot.Text = "10";
            _take.Text= "300";
            _stop.Text= "100";   
            _comiss.Text= "5";
            _countTrades.Text = "1000";
            _percentProfit.Text = "30";
            _go.Text= "5000";
            _minStartPercent.Text = "20";
        }

        private void _canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Draw();
        }

        private void _comBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;

            int index = comboBox.SelectedIndex;
            Draw();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Calculate();
            Draw();
        }

        private void Calculate() 
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

            datas = new List<Data>();

            foreach (StrategyType type in _strategies)
            {
                datas.Add(new Data(depoStart, type));
            }

            datas.RemoveAt(datas.Count-1);

            int lotPercent = startLot;
            decimal percent = startLot * go * 100 / depoStart;

            decimal multiply = take / stop;

            int lotProgress = CalculateLot(depoStart, minStartPercent, go);
            int lotDown = startLot;

            for (int i=0; i<countTrades;i++)
            {
                int rnd = _random.Next(1,100);
                if (rnd <= percProfit)
                { //Сделка прибыльная
                    // ==============1 startegy===========================
                    datas[0].ResultDepo += (take - comiss) * startLot;

                    // ==============2 startegy===========================
                    datas[1].ResultDepo+= (take - comiss) * lotPercent;
                    int newLot = CalculateLot(datas[1].ResultDepo,percent,go);
                    if(lotPercent < newLot) lotPercent = newLot;

                    // ==============3 startegy===========================
                    datas[2].ResultDepo += (take - comiss) * lotProgress;
                    lotProgress = CalculateLot(depoStart,minStartPercent*multiply,go);

                    // ==============4 startegy===========================
                    datas[3].ResultDepo += (take - comiss) * lotDown;
                    lotDown = startLot;
                }
                else 
                { //Сделка убыточная
                    // ==============1 startegy===========================
                    datas[0].ResultDepo -= (stop + comiss) * startLot;
                    
                    // ==============2 startegy===========================
                    datas[1].ResultDepo -= (stop + comiss) * lotPercent;

                    // ==============3 startegy===========================
                    datas[2].ResultDepo -= (stop + comiss) * lotProgress;
                    lotProgress = CalculateLot(depoStart, minStartPercent, go);

                    // ==============4 startegy===========================
                    datas[3].ResultDepo -= (stop + comiss) * lotDown;
                    lotDown /= 2;

                    if (lotDown == 0) lotDown = 1;
                }

               
            }

            _dataGrid.ItemsSource= datas;

        }

        private void Draw() 
        {
            _canvas.Children.Clear();
            _canvas2.Children.Clear();

            int index = _comBox.SelectedIndex;
            switch (index)
            {
                case 0:
                    DrawLine(index, Brushes.Orange);
                    DrawLine2(index, Brushes.Orange);
                    break;
                case 1:
                    DrawLine(index, Brushes.Red);
                    DrawLine2(index, Brushes.Red);
                    break;
                case 2:
                    DrawLine(index, Brushes.Green);
                    DrawLine2(index, Brushes.Green);
                    break;
                case 3:
                    DrawLine(index, Brushes.Blue);
                    DrawLine2(index, Brushes.Blue);
                    break;
                case 4:
                    DrawLine(0, Brushes.Orange);
                    DrawLine2(0, Brushes.Orange);
                    DrawLine(1, Brushes.Red);
                    DrawLine2(1, Brushes.Red);
                    DrawLine(2, Brushes.Green);
                    DrawLine2(2, Brushes.Green);
                    DrawLine(3, Brushes.Blue);
                    DrawLine2(3, Brushes.Blue);
                    break;
            }


        }

        private void DrawLine(int index,SolidColorBrush color) 
        {
            if (datas.Count == 0) return;

            List<decimal> ListEquity = datas[index].GetListEquity();
            int count =ListEquity.Count;

            decimal maxEquity;
            decimal minEquity;

            if (_comBox.SelectedItem.ToString() == StrategyType.ALL.ToString())
            {
                var listMaxEquity = new List<decimal>();
                var listMinEquity = new List<decimal>();

                foreach (var data in datas)
                {
                    listMaxEquity.Add(data.GetListEquity().Max());
                    listMinEquity.Add(data.GetListEquity().Min());

                }
                maxEquity = listMaxEquity.Max();
                minEquity = listMinEquity.Min();

            }
            else
            {
                maxEquity = ListEquity.Max();
                minEquity = ListEquity.Min();
            }


            double stepX = _canvas.ActualWidth/ count;
            double koef = (double)(maxEquity - minEquity) / _canvas.ActualHeight;

            double x = 0;
            double y = 0;

            double _x = 0;
            double _y = _canvas.ActualHeight - (double)(ListEquity[0] - minEquity) / koef;

            for (int i=0; i<count;i++)
            {
                y = _canvas.ActualHeight - (double)(ListEquity[i] - minEquity) / koef;
                
                /*Ellipse ellipse = new Ellipse() 
                { 
                    Width =2,
                    Height = 2,
                    Stroke = color
                };

                Canvas.SetLeft(ellipse, x);
                Canvas.SetTop(ellipse, y);
                _canvas.Children.Add(ellipse);
                */

                Line line = new Line()
                {
                    Stroke = color,
                    StrokeThickness = 4
                };
                line.X1 = _x;
                line.X2 = x;
                line.Y1 = _y;
                line.Y2 = y;

                _canvas.Children.Add(line);

                x += stepX;
                _x = x;
                _y = y;
            }
        }

        private void DrawLine2(int index, SolidColorBrush color)
        {
            if (datas.Count == 0) return;

            List<decimal> ListDD = datas[index].GetListDrawDown();
            int count = ListDD.Count;

            decimal maxDD;
            decimal minDD;

            if (_comBox.SelectedItem.ToString() == StrategyType.ALL.ToString())
            {
                var listMaxDD = new List<decimal>();
                var listMinDD = new List<decimal>();

                foreach (var data in datas)
                {
                    listMaxDD.Add(data.GetListDrawDown().Max());
                    listMinDD.Add(data.GetListDrawDown().Min());

                }
                maxDD = listMaxDD.Max();
                minDD = listMinDD.Min();

            }
            else
            {
                maxDD = ListDD.Max();
                minDD = ListDD.Min();
            }


            double stepX = _canvas2.ActualWidth / count;
            double koef = (double)(maxDD - minDD) / _canvas2.ActualHeight;

            double x = 0;
            double y = 0;

            double _x = 0;
            double _y = _canvas2.ActualHeight - (double)(ListDD[0] - minDD) / koef;

            for (int i = 0; i < count; i++)
            {
                y = _canvas2.ActualHeight - (double)(ListDD[i] - minDD) / koef;

                Line line = new Line()
                {
                    Stroke = color,
                    StrokeThickness = 4
                };
                line.X1 = _x;
                line.X2 = x;
                line.Y1 = _y;
                line.Y2 = y;

                _canvas2.Children.Add(line);

                x += stepX;
                _x = x;
                _y = y;
            }
        }
        /// <summary>
        /// прорисока графика по индексу из ComboBox
        /// </summary>
        /// <param name="index"></param>

        private int CalculateLot(decimal currentDepo, decimal percent, decimal go)
        {
            if(percent>100) { percent = 100; }

            decimal lot = currentDepo / go / 100 * percent;

            return (int)lot;
        }
        private decimal GetDecimalFromString(string str)
        {
            if(decimal.TryParse(str, out decimal result)) return result;
            return 0; 
        }

        private int GetIntFromString(string str)
        {
            if (int.TryParse(str, out int result)) return result;
            return 0; 
        }

        /// <summary>
        /// обработчик события прорисовки колонки таблицы
        /// </summary>
        private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.Column is DataGridTextColumn col)
            {
                // Форматирование числа с разделителем тысяч
                string format =
                    e.PropertyName == "PercentProfit" || e.PropertyName == "PercentDrawDown"
                    ? "{0:N2}"
                    : "{0:N0}";
                col.Binding = new Binding(e.PropertyName)
                {
                    StringFormat = format,
                    ConverterCulture = new CultureInfo("ru-RU")
                };
                // Стиль для отображения
                var displayStyle = new Style(typeof(TextBlock));
                displayStyle.Setters.Add(new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Right));
                col.ElementStyle = displayStyle;
            }
        }
        #endregion
    }
}