using MyCapital.Entity;
using MyCapital.Enums;
using System;
using System.CodeDom;
using System.Reflection.Metadata.Ecma335;
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

namespace MyCapital
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

        #region Fields =================================================

        List<StrategyType> _strategies = new List<StrategyType>()
        {
                StrategyType.FIX,
                StrategyType.CAPITALIZATION,
                StrategyType.PROGRESSIVE,
                StrategyType.DOWNGRADE
        };

        Random _random = new Random(123);

        List<Data> _datas;


        #endregion

        #region Methods ================================================

        private void Init()
        {
            _comboBox.ItemsSource = _strategies;
            
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

            //int index = _comboBox.SelectedIndex;

            if (_datas != null)
            {
                Draw(_datas);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _datas = Calculate();

            int index = _comboBox.SelectedIndex;

            Draw(_datas);
        }

        private List<Data> Calculate()
        {
            decimal depoStart = GetDecimalFromSrting(_depo.Text);
            int startLot = GetIntFromSrting(_startLot.Text);
            decimal take = GetDecimalFromSrting(_take.Text);
            decimal stop = GetDecimalFromSrting(_stop.Text);
            decimal comiss = GetDecimalFromSrting(_comiss.Text);
            int countTrades = GetIntFromSrting(_countTrades.Text);
            decimal percProfit = GetDecimalFromSrting(_percentProfit.Text);
            decimal minStartPercent = GetDecimalFromSrting(_minStartPercent.Text);
            decimal go = GetDecimalFromSrting(_go.Text);

            
            List<Data> datas = new List<Data>();

            foreach (StrategyType type in _strategies)
            {
                Data data = new Data(depoStart, type);
                datas.Add(data);   
            }

            // lots related variables
            int lotPercent = startLot;
            decimal percent = startLot * go * 100 / depoStart;
            decimal multiply = take / stop;
            int lotProgress = CalculateLot(depoStart, minStartPercent, go);
            int lotDown = startLot;

            for (int i = 0; i < countTrades; i++)
            {
                int rnd = _random.Next(1, 101);

                if (rnd <= percProfit)  //Прибыльная сделка
                {                  
                    //strategy 1 FIX ===============================================

                    datas[0].ResultDepo += (take - comiss) * startLot;

                    //strategy 2 CAPITALIZATION ====================================

                    datas[1].ResultDepo += (take - comiss) * lotPercent;

                    int newLot = CalculateLot(datas[1].ResultDepo, percent, go);

                    if (lotPercent < newLot) lotPercent = newLot;

                    //strategy 3 PROGRESS ==========================================

                    datas[2].ResultDepo += (take - comiss) * lotProgress;

                    lotProgress = CalculateLot(depoStart, minStartPercent * multiply, go);

                    //strategy 4 DOWNGRADE +========================================

                    datas[3].ResultDepo += (take - comiss) * lotDown;

                    lotDown = startLot; 

                    
                }
                else //Убыточная сделка
                {
                    //strategy 1 FIX ===============================================

                    datas[0].ResultDepo -= (stop + comiss) * startLot;

                    //strategy 2 CAPITALIZATION ====================================

                    datas[1].ResultDepo -= (stop + comiss) * lotPercent;

                    //strategy 3 PROGRESS ==========================================

                    datas[2].ResultDepo -= (stop + comiss) * lotProgress;
                    lotProgress = CalculateLot(depoStart, minStartPercent, go);

                    //strategy 4 DOWNGRADE +========================================

                    datas[3].ResultDepo -= (stop + comiss) * lotDown;

                    lotDown /= 2;

                    if (lotDown < 1) lotDown = 1;

                }
            }



            _dataGrid.ItemsSource = datas;

            return datas;
        }
        private void Draw(List<Data> datas)
        {
            //if (datas = null) return;
            int index = _comboBox.SelectedIndex;
            List<decimal> listEquity = datas[index].GetListEquity();

            int count = listEquity.Count;

            decimal maxEquty = listEquity.Max();
            decimal minEquty = listEquity.Min();

            double stepX = _canvas.ActualWidth / count;
            double koef = (double)(maxEquty - minEquty) / _canvas.ActualHeight;

            double x = 0;
            double y = 0;

            _canvas.Children.Clear();

            for (int i = 0; i < count; i++)
            {
                y = _canvas.ActualHeight - (double)(listEquity[i] - minEquty) / koef;

                Ellipse ellipse = new Ellipse()
                {
                    Width = 2,
                    Height = 2,
                    Stroke = Brushes.Black
                };

                Canvas.SetLeft(ellipse, x);
                Canvas.SetTop(ellipse, y);

                _canvas.Children.Add(ellipse);

                x += stepX;
            }
        }


        private int CalculateLot(decimal currentDepo, decimal percent, decimal go)
        {
            if (percent > 100) percent = 100; 
            decimal lot = currentDepo / go / 100 * percent;
            return (int)lot;
        }

        private decimal GetDecimalFromSrting(string str)
        {
            if (decimal.TryParse(str, out decimal result)) return result;

            return 0;
                      
        }

        private int GetIntFromSrting(string str)
        {
            if (int.TryParse(str, out int result)) return result;

            return 0;

        }

        #endregion

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_datas != null)
            {
                Draw(_datas);
            }
        }
    }
}