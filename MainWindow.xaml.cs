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
               
        };

        List<StrategyType> _comboBoxStrategies = new List<StrategyType>()
        {
                StrategyType.FIX,
                StrategyType.CAPITALIZATION,
                StrategyType.PROGRESS,
                StrategyType.DOWNGRADE,
                StrategyType.ALL
        };

        Random _random = new Random();

        List<Data> datas = new List<Data>();
        bool checkButtonClick = false;
        #endregion

        #region Methods ===========================================

        private void Init() 
        {
            _comboBox.ItemsSource = _comboBoxStrategies;

            _comboBox.SelectionChanged += _comboBox_SelectionChanged;
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
            ComboBox comboBox = (ComboBox)sender;
            
            int index = comboBox.SelectedIndex;

            if (checkButtonClick) 
            {
                GetTrend(index);
            }

            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            datas = Calculate();
                        
            int index = _comboBox.SelectedIndex;

            GetTrend(index);

            checkButtonClick = true;
        }


        private List<Data> Calculate() 
        {
            decimal depoStart = GetDecimalFromString(_depo.Text);
            int startLot = GetIntFromString(_startLot.Text);
            decimal take = GetDecimalFromString(_take.Text);
            decimal stop = GetDecimalFromString(_stop.Text);
            decimal comiss = GetDecimalFromString(_comiss.Text);
            int countTrade = GetIntFromString(_countTrades.Text);
            decimal percProfit = GetDecimalFromString(_percentProfit.Text);
            decimal minStartPercent = GetDecimalFromString(_minStartPercent.Text);
            decimal go = GetDecimalFromString(_go.Text);

            List<Data> datas = new List<Data>();

            foreach (StrategyType type in _strategies)
            {
                datas.Add(new Data(depoStart, type));
            }

            datas[0].StrategyColor = StrategyColor.Black.ToString();
            datas[1].StrategyColor = StrategyColor.Orange.ToString();
            datas[2].StrategyColor = StrategyColor.Green.ToString();
            datas[3].StrategyColor = StrategyColor.Blue.ToString();

            int lotPercent = startLot;
            decimal percent = startLot * go * 100 / depoStart;
            decimal multiply = take / stop;
            int lotProgress = CalculateLot(depoStart, minStartPercent, go);
            int lotDown = startLot;

            for (int i = 0; i < countTrade; i++) 
            {
                int rnd = _random.Next(1, 100);

                if (rnd <= percProfit) //Расчет прибыльной сделки.
                {
                    //Стратегия FIX
                    datas[0].ResultDepo += (take - comiss) * startLot;

                    //Стратегия CAPITALIZATION
                    datas[1].ResultDepo += (take - comiss) * lotPercent;
                    int newlot = CalculateLot(datas[1].ResultDepo, percent, go);
                    if(lotPercent < newlot) lotPercent = newlot;

                    //Стратегия PROGRESS
                    datas[2].ResultDepo += (take - comiss) * lotProgress;
                    lotProgress = CalculateLot(depoStart, minStartPercent * multiply, go);

                    // Стратегия DOWNGRADE
                    datas[3].ResultDepo += (take - comiss) * lotDown;
                    lotDown = startLot;

                }
                else //Расчет убыточной сделки.
                {
                    //Стратегия FIX
                    datas[0].ResultDepo -= (stop + comiss) * startLot;

                    //Стратегия CAPITALIZATION
                    datas[1].ResultDepo -= (stop + comiss) * lotPercent;

                    //Стратегия PROGRESS
                    datas[2].ResultDepo -= (stop + comiss) * lotProgress;
                    lotProgress = CalculateLot(depoStart, minStartPercent, go);

                    // Стратегия DOWNGRADE
                    datas[3].ResultDepo -= (stop + comiss) * lotDown;
                    lotDown /= 2;
                    if (lotDown == 0) lotDown = 1;
                }
            }

            _dataGrid.ItemsSource = datas;
            return datas;

        }

        private decimal GetDecimalFromString(string str) 
        {
            if(decimal.TryParse(str, out decimal result))
            {
                return result;
            }
            else
            {
                return 0;
            };
      
        }

        private int GetIntFromString(string str)
        {
            if (int.TryParse(str, out int result))
            {
                return result;
            }
            else
            {
                return 0;
            };

        }

        private int CalculateLot(decimal currentDepo, decimal percent, decimal go) 
        {
            if (percent > 100) { percent = 100; }

            decimal lot = currentDepo / go / 100 * percent;
            return (int)lot;
        }

        private void Draw(List<Data> datas, int index) 
        {
            List<decimal> ListEquity;

            ListEquity = datas[index].GetListEquity();
         
            int count = ListEquity.Count;

            //decimal maxEquity = ListEquity.Max();
            //decimal minEquity = ListEquity.Min();


            // Расчет границ для построения графиков 

            decimal maxEquity;
            decimal minEquity;

            if (_comboBox.SelectedIndex == 4)
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
            //===================================================================

            if (_ellipse.IsChecked == true) 
            {
                // График, посторенный точками ==============================
                
                double stepX = _canvas.ActualWidth / count;
                double koef = (double)(maxEquity - minEquity) / _canvas.ActualHeight;

                double x = 0;
                double y = 0;

                for (int i = 0; i < count; i++)
                {
                    y = _canvas.ActualHeight - (double)(ListEquity[i] - minEquity) / koef;

                    Ellipse ellipse = new Ellipse();

                    if (index == 0) 
                    {
                        ellipse = new Ellipse
                        {
                            Width = 2,
                            Height = 2,
                            Stroke = Brushes.Black
                        };
                    }

                    if (index == 1)
                    {
                        ellipse = new Ellipse
                        {
                            Width = 2,
                            Height = 2,
                            Stroke = Brushes.Orange
                        };
                    }

                    if (index == 2)
                    {
                        ellipse = new Ellipse
                        {
                            Width = 2,
                            Height = 2,
                            Stroke = Brushes.Green
                        };
                    }

                    if (index == 3)
                    {
                        ellipse = new Ellipse
                        {
                            Width = 2,
                            Height = 2,
                            Stroke = Brushes.Blue
                        };
                    }


                    Canvas.SetLeft(ellipse, x);
                    Canvas.SetTop(ellipse, y);

                    _canvas.Children.Add(ellipse);

                    x += stepX;
                }

                // =======================================================

            }

            if (_line.IsChecked == true) 
                
            {
                // График, построенный линиями ============================
                double canvasWidth = _canvas.ActualWidth; //Фактическая ширина
                double canvasHeight = _canvas.ActualHeight; // Фактическая высота
                double lineStepX = _canvas.ActualWidth / count;
                double lineKoef = (double)(maxEquity - minEquity) / _canvas.ActualHeight;

                var polyline = new Polyline();
                if (index == 0)
                {
                    polyline = new Polyline
                    {
                        Stroke = Brushes.Black,
                        StrokeThickness = 1
                    };
                }


                if (index == 1)
                {
                    polyline = new Polyline
                    {
                        Stroke = Brushes.Orange,
                        StrokeThickness = 1
                    };
                }

                if (index == 2)
                {
                    polyline = new Polyline
                    {
                        Stroke = Brushes.Green,
                        StrokeThickness = 1
                    };
                }

                if (index == 3)
                {
                    polyline = new Polyline
                    {
                        Stroke = Brushes.Blue,
                        StrokeThickness = 1
                    };
                }

                double lineX = 0;
                double lineY = _canvas.ActualHeight;

                var points = new PointCollection();
                for (int k = 0; k < count; k++)
                {
                    points.Add(new Point(lineX, lineY));
                    polyline.Points = points;

                    lineY = _canvas.ActualHeight - (double)(ListEquity[k] - minEquity) / lineKoef;
                    lineX += lineStepX;

                }
                _canvas.Children.Add(polyline);

                //==========================================================

            }



        }

        private void GetTrend(int index) 
        {

                if (index == 4)
                {
                    _canvas.Children.Clear();

                    for (int i = 0; i < index; i++)
                    {
                        Draw(datas, i);
                    }
                }
                else
                {
                    _canvas.Children.Clear();
                    Draw(datas, index);
                }
           
        }

        private void _canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
        
            if (checkButtonClick)
            {
                int index = _comboBox.SelectedIndex;
                GetTrend(index);
            }

        }

        private void RadioButtonLineChecked(object sender, RoutedEventArgs e)
        {
            
            if (checkButtonClick)
            {
                int index = _comboBox.SelectedIndex;
                GetTrend(index);
            }
        }

        private void RadioButtonEllipseChecked(object sender, RoutedEventArgs e)
        {
          
            if (checkButtonClick)
            {
                int index = _comboBox.SelectedIndex;
                GetTrend(index);
            }
        }
#endregion
    }
}