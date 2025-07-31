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

        #region Fields ========================================

        List<StrategyType> _strategies = new() { (StrategyType)0, (StrategyType)1, (StrategyType)2, (StrategyType)3 };
        

        List<Data> datas;
        Random _random = new Random();

        #endregion

        #region Methods ===========================================

       private void Init()
       {
            _comboBox.ItemsSource = _strategies;
            
            _comboBox.SelectionChanged += comboBox_SelectionChanged;
            _comboBox.SelectedIndex = 0;
            
            _firstCheckBox.Content = _strategies[0];
            _firstCheckBox.Foreground = GetColor(0);
            _firstCheckBox.Click += CheckBox_Click;
            
            _secondCheckBox.Content = _strategies[1];
            _secondCheckBox.Foreground = GetColor(1);
            _secondCheckBox.Click += CheckBox_Click;
 
            _thirdCheckBox.Content = _strategies[2];
            _thirdCheckBox.Foreground = GetColor(2);
            _thirdCheckBox.Click += CheckBox_Click;

            _fourthCheckBox.Content = _strategies[3];
            _fourthCheckBox.Foreground = GetColor(3);
            _fourthCheckBox.Click += CheckBox_Click;

            _canvas.SizeChanged += _canvas_SizeChanged;

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

        private void _canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (datas is null) return;
            Draw();
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if(datas is null) { return; };
            if (_firstCheckBox.IsChecked != null) datas[0]._isShown = (bool)_firstCheckBox.IsChecked;
            if (_secondCheckBox.IsChecked != null) datas[1]._isShown = (bool)_secondCheckBox.IsChecked;
            if (_thirdCheckBox.IsChecked != null) datas[2]._isShown = (bool)_thirdCheckBox.IsChecked;
            if (_fourthCheckBox.IsChecked != null) datas[3]._isShown = (bool)_fourthCheckBox.IsChecked;
            Draw();
        }

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;

            int index = comboBox.SelectedIndex;
            if (datas != null ) Draw();
        }
        

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            datas = Calculate();
            if (datas != null) CheckBox_Click(sender, e);//Draw()
        }

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

            List<Data> datas = new List<Data>();

            foreach (StrategyType type in _strategies) datas.Add(new Data(depoStart, type));

           


            int lotPercent = startLot;
            decimal percent = startLot * go * 100 / depoStart;

            decimal multiply = take / stop;
            int lotProgress = CalculateLot(depoStart, minStartPercent, go);

            int lotDown = startLot;

            int rnd;

            for (int i = 0; i < countTrades; i++)
            {
                rnd = _random.Next(1, 100);

                if (rnd <= percProfit)
                {
                    // Сделка прибыльная

                    //============ 1 strategy =================
                    datas[0].ResultDepo += (take - comiss) * startLot;

                    //============ 2 strategy ================

                    datas[1].ResultDepo += (take - comiss) * lotPercent;

                    int newLot = CalculateLot(datas[1].ResultDepo, percent, go);

                    if (lotPercent< newLot) lotPercent = newLot;

                    //============ 3 strategy ================

                    datas[2].ResultDepo += (take - comiss) * lotProgress;

                    lotProgress = CalculateLot(depoStart, minStartPercent * multiply, go);

                    //=========== 4 strategy ================== 

                    datas[3].ResultDepo += (take - comiss) * lotDown;

                    lotDown = startLot;
                }
                else
                {
                    // Сделка убыточная
                    //============ 1 strategy =================
                    datas[0].ResultDepo -= (stop + comiss) * startLot;

                    //============ 2 strategy ==================

                    datas[1].ResultDepo -= (stop + comiss) * lotPercent;

                    //============ 3 strategy ==================

                    datas[2].ResultDepo -= (stop + comiss) * lotProgress;

                    lotProgress = CalculateLot(depoStart, minStartPercent, go);

                    //============ 4 strategy ==================

                    datas[3].ResultDepo -= (stop + comiss) * lotDown;

                    lotDown /= 2;

                    if (lotDown < 1) lotDown = 1;
                }
            }

            _dataGrid.ItemsSource = datas;

            return datas;
        }

        private void Draw()
        {
            
            
            _canvas.Children.Clear();
            
            _legend.Foreground = Brushes.White;
            _legend.Text="Legend: ";

            
            decimal maxEquity = 0;
            decimal minEquity = 0;

            foreach (Data data in datas) {
                if (!data._isShown) continue; 
                if (maxEquity < data.MaxDepo) maxEquity = data.MaxDepo;
                if (minEquity > data.MinDepo) minEquity = data.MinDepo;
            }
            double koef = (double)(maxEquity - minEquity) / _canvas.ActualHeight;

            double x = 0;
            double y = 0;

            for (int index = 0; index < datas.Count; index++) {

                if( !datas[index]._isShown) continue;

                List<decimal> listEquity = datas[index].GetListEquity();
                
                double steoX = _canvas.ActualWidth / listEquity.Count;
                
                x = 0;
                y = _canvas.ActualHeight - (double)(listEquity[0] - minEquity) / koef;

                for (int i = 0; i < listEquity.Count; i++)
                {


                    /*Ellipse ellips = new Ellipse()
                    {
                        Width = 2,
                        Height = 2,
                        Stroke = GetColor(index)
                    };

                    Canvas.SetLeft(ellips, x);
                    Canvas.SetTop(ellips, y);

                    _canvas.Children.Add(ellips);*/
                    Line myLine = new Line()
                    {
                        Stroke = GetColor(index),
                        StrokeThickness = 2
                    };
                    myLine.X1 = x;
                    myLine.Y1 = y;
                    y = _canvas.ActualHeight - (double)(listEquity[i] - minEquity) / koef;
                    x += steoX;
                    myLine.X2 = x;
                    myLine.Y2 = y;

                    _canvas.Children.Add(myLine);
                }
                _legend.Inlines.Add(new Run(datas[index].StrategyType.ToString() + "  ") { Foreground = GetColor(index) });
                
            }
        }
        Brush GetColor(int index)
        {
            switch (index)
            {
                case 0: return Brushes.Orange;
                case 1: return Brushes.Red;
                case 2: return Brushes.Green;
                case 3: return Brushes.Blue;
                default: return Brushes.Yellow;
            }
        }
        private int CalculateLot(decimal currentDepo, decimal percent, decimal go)
        {
            if (percent > 100) { percent = 100; }

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

             #endregion
     }
}