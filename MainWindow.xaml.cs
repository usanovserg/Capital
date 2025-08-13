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

        #region fields ========================================================

        List<StrategyType> _strategies = new List<StrategyType>()
        {
            StrategyType.FIX,
            StrategyType.CAPITALISATION,
            StrategyType.PROGRESS,
            StrategyType.DOWNGRADE,
        };

        Random _random = new Random();



        List<Data> datas = new List<Data>();



        #endregion



        #region methods ========================================================

        private void Init()
        {
            //_comboBox.ItemsSource = new List<StrategyType>()
            //{
            //    StrategyType.FIX,
            //    StrategyType.CAPITALISATION,
            //    StrategyType.PROGRESS,
            //    StrategyType.DOWNGRADE,
            //};


            //_comboBox.Items.Add(StrategyType.FIX);

            //_comboBox.ItemsSource = _strategies;

            //_comboBox.SelectionChanged += _comboBox_SelectionChanged;
            //_comboBox.SelectedIndex = 0;

            _listBox.ItemsSource = _strategies;

            //_listBox.SelectionChanged += _listBox_SelectionChanged;
            _listBox.SelectedIndex = 0;

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

        //private void _comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    ComboBox comboBox = sender as ComboBox;

        //    if (DateGridData.CountButtonClicks == true)
        //    {
        //        //datas = Calculate();                

        //        Draw(datas);
        //    }

        //}

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            string content = checkBox.Content.ToString();
            bool isChecked = checkBox.IsChecked == true;

            // Ваш код обработки
            //MessageBox.Show($"Чекбокс \"{content}\" теперь {(isChecked ? "отмечен" : "снят")}");



            if (Data.CountButtonClicks == true)
            {
                //datas = Calculate();


                Draw(datas);
            }

        }




        //private void _listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    ListBox listBox = sender as ListBox;

        //    if (DateGridData.CountButtonClicks == true)
        //    {
        //        //datas = Calculate();
        //        Draw(datas);
        //    }
        //}

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            datas = Calculate();

            //int index = _checkBox.SelectedIndex;
            Draw(datas);
        }


        private void Canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Повторно нарисовать график с новыми размерами


            if (Data.CountButtonClicks == true)
            {
                Draw(datas);
            }

        }

        private List<Data> Calculate()
        {
            decimal depoStart = GetDecimalFromString(_depo.Text);
            int startLot = GetIntFromString(_startLot.Text);
            decimal take = GetDecimalFromString(_take.Text);
            decimal stop = GetDecimalFromString(_stop.Text);
            decimal comiss = GetDecimalFromString(_comiss.Text);
            int countTrades = GetIntFromString(_countTrades.Text);
            decimal percentProfit = GetDecimalFromString(_percentProfit.Text);
            decimal minStartPercent = GetDecimalFromString(_minStartPercent.Text);
            decimal go = GetDecimalFromString(_go.Text);

            List<Data> datas = new List<Data>();




            foreach (StrategyType type in _strategies)
            {
                //DateGridData data = new DateGridData();
                //data.StrategyType = type;

                //datas.Add(data);

                datas.Add(new Data(depoStart, type));


            }


            //-----------------------------------------------
            int lotPercent = startLot;
            decimal percent = startLot * go * 100 / depoStart;

            decimal multyply = take / stop;
            int lotProgress = CalculateLot(depoStart, minStartPercent, go);

            int lotDown = startLot;

            //---------------------------------------------------

            for (int i = 0; i < countTrades; i++)
            {
                int rnd = _random.Next(1, 100);

                if (rnd <= percentProfit)
                {
                    // profit

                    //============== 1 ====================================
                    datas[0].ResultDepo += (take - comiss) * startLot;

                    //============== 2 ====================================

                    datas[1].ResultDepo += (take - comiss) * lotPercent;

                    int newLot = CalculateLot(datas[1].ResultDepo, percent, go);

                    if (lotPercent < newLot)
                    {
                        lotPercent = newLot;
                    }

                    //============== 3 ===================================
                    datas[2].ResultDepo += (take - comiss) * lotProgress;

                    lotProgress = CalculateLot(depoStart, minStartPercent * multyply, go);

                    //============== 4 ===================================
                    datas[3].ResultDepo += (take - comiss) * lotDown;

                    lotDown = startLot;

                }
                else
                {
                    // loss

                    //============== 1 ===================================
                    datas[0].ResultDepo -= (stop + comiss) * startLot;

                    //============== 2 ===================================

                    datas[1].ResultDepo -= (stop + comiss) * lotPercent;

                    //============== 3 ===================================

                    datas[2].ResultDepo -= (stop + comiss) * lotProgress;

                    lotProgress = CalculateLot(depoStart, minStartPercent, go);

                    //============== 4 ===================================

                    datas[3].ResultDepo -= (stop + comiss) * lotDown;

                    lotDown /= 2;

                    if (lotDown == 0) { lotDown = 1; }

                }
            }



            _dataGrid.ItemsSource = datas;




            return datas;



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


        private int CalculateLot(decimal currentDepo, decimal percent, decimal go)
        {
            if (percent > 100) { percent = 100; }

            decimal lot = currentDepo / go / 100 * percent;

            return (int)lot;
        }





        //private void Draw(List<DateGridData> datas)       //-----------ТОЧКИ(ЭЛЛИПСЫ)--------------------
        //{

        //    _canvas.Children.Clear();

        //    int index = _comboBox.SelectedIndex;

        //    List<decimal> listEquity = datas[index].GetListEquity();

        //    int count = listEquity.Count;
        //    decimal maxEquity = listEquity.Max();
        //    decimal minEquity = listEquity.Min();

        //    double stepX = _canvas.ActualWidth / count;
        //    double koef = (double)(maxEquity - minEquity) / _canvas.ActualHeight;

        //    double x = 0;
        //    double y = 0;

        //    for (int i = 0; i < count; i++)
        //    {
        //        y = _canvas.ActualHeight - (double)(listEquity[i] - minEquity) / koef;

        //        Ellipse ellipse = new Ellipse()
        //        {
        //            Width = 2,
        //            Height = 2,
        //            Stroke = Brushes.Black
        //        };

        //        Canvas.SetLeft(ellipse, x);
        //        Canvas.SetTop(ellipse, y);

        //        _canvas.Children.Add(ellipse);

        //        x += stepX;
        //    }

        //}        

        private List<StrategyType> GetCheckedStrategies()
        {
            return _listBox.SelectedItems
                .Cast<StrategyType>()
                .ToList();
        }



        private void Draw(List<Data> datas)      //----------------------ЛИНИИ--------------------------------
        {

            _canvas.Children.Clear();

            //int index = _comboBox.SelectedIndex;

            //int index = _listBox.SelectedIndex;
            int index = 0;

            var checkedStrategies = GetCheckedStrategies();



            for (int j = 0; j < _strategies.Count; j++)
            {
                // Ваш код для каждого выбранного варианта
                //MessageBox.Show(strategy.ToString());
                /*Console.WriteLine(strategy);*/ // или, например, MessageBox.Show(strategy.ToString());
                for (int n = 0; n < checkedStrategies.Count; n++)
                {
                    if (_strategies[j].ToString() == checkedStrategies[n].ToString())
                    {
                        index = j;

                        List<decimal> listEquity = datas[index].GetListEquity();

                        int count = listEquity.Count;
                        if (count < 2) return; // Нужно минимум 2 точки для линии

                        decimal maxEquity = listEquity.Max();
                        decimal minEquity = listEquity.Min();

                        double stepX = _canvas.ActualWidth / (count - 1);
                        double koef = (double)(maxEquity - minEquity) / _canvas.ActualHeight;

                        double prevX = 0;
                        double prevY = _canvas.ActualHeight - (double)(listEquity[0] - minEquity) / koef;

                        for (int i = 0; i < count; i++)
                        {
                            double x = i * stepX;
                            double y = _canvas.ActualHeight - (double)(listEquity[i] - minEquity) / koef;

                            if (index == 0)
                            {
                                Line line = new Line()
                                {
                                    X1 = prevX,
                                    Y1 = prevY,
                                    X2 = x,
                                    Y2 = y,
                                    //Stroke = Brushes.Blue,

                                    Stroke = Brushes.Yellow,


                                    StrokeThickness = 2
                                };

                                _canvas.Children.Add(line);

                                prevX = x;
                                prevY = y;
                            }

                            if (index == 1)
                            {
                                Line line = new Line()
                                {
                                    X1 = prevX,
                                    Y1 = prevY,
                                    X2 = x,
                                    Y2 = y,
                                    //Stroke = Brushes.Blue,

                                    Stroke = Brushes.Green,


                                    StrokeThickness = 2
                                };

                                _canvas.Children.Add(line);

                                prevX = x;
                                prevY = y;
                            }

                            if (index == 2)
                            {
                                Line line = new Line()
                                {
                                    X1 = prevX,
                                    Y1 = prevY,
                                    X2 = x,
                                    Y2 = y,
                                    //Stroke = Brushes.Blue,

                                    Stroke = Brushes.LightBlue,


                                    StrokeThickness = 2
                                };

                                _canvas.Children.Add(line);

                                prevX = x;
                                prevY = y;
                            }

                            if (index == 3)
                            {
                                Line line = new Line()
                                {
                                    X1 = prevX,
                                    Y1 = prevY,
                                    X2 = x,
                                    Y2 = y,
                                    //Stroke = Brushes.Blue,

                                    Stroke = Brushes.Coral,


                                    StrokeThickness = 2
                                };

                                _canvas.Children.Add(line);

                                prevX = x;
                                prevY = y;
                            }



                        }

                    }
                }


            }




        }




        #endregion
    }
}
