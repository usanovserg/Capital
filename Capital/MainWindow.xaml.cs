using Capital.Entity;
using Capital.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
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
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
                        Init();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _comboBox.SelectedIndex = 0;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Draw();
        }

        #region Fields =======================================================================
        List<StrategyType> _stratigies = new List<StrategyType>
        {
            StrategyType.FIX,
            StrategyType.CAPITALIZATION,
            StrategyType.PROGRESS,
            StrategyType.DOWNGRADE
        };

        List<Data> datas = new List<Data>();

        Random _random = new Random();
        #endregion


        #region Methods ======================================================================
        private void Init ()
        {
            _comboBox.ItemsSource = _stratigies;
            _comboBox.SelectionChanged += _comboBox_SelectionChanged;

            _depo.Text = "100000";
            _startLot.Text = "10";
            _take.Text = "300";
            _stop.Text = "100";
            _comiss.Text = "5";
            _percentProfit.Text = "30";
            _countTrades.Text = "1000";
            _minStartPercent.Text = "20";
            _go.Text = "5000";
        }

        private void _comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = ((ComboBox)sender).SelectedIndex;
            Draw();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Calculate();
            Draw();
        }



        void Calculate ()
        {
            decimal depoStart = StringToDecimal(_depo.Text);
            int startLot = StringToInt(_startLot.Text);
            decimal take = StringToDecimal(_take.Text);
            decimal stop = StringToDecimal(_stop.Text);
            decimal comiss = StringToDecimal(_comiss.Text);
            decimal percentProfit = StringToDecimal(_percentProfit.Text);
            int countTrades = StringToInt(_countTrades.Text);
            decimal minStartPercent = StringToDecimal(_minStartPercent.Text);
            decimal go = StringToDecimal(_go.Text);

            //List<Data> datas = new List<Data>();

            foreach (var type in _stratigies)
            {
                datas.Add(new Data (depoStart, type));
            }

            int lotPercent = startLot;
            decimal percent = startLot * go * 100 / depoStart;
            decimal multiply = take / stop;
            int lotProgress = CalculateLot(depoStart, minStartPercent, go);
            int lotDown = startLot;

            for (int i = 0; i < countTrades; ++i)
            {
                int rnd = _random.Next(1 , 100);
                if (rnd <= percentProfit)
                {
                    // Сделка прибыльная
                    // 1 стратегия
                    datas[0].ResultDepo += (take - comiss) * startLot;

                    //2 страегия
                    datas[1].ResultDepo += (take - comiss) * lotPercent;
                    int newLot = CalculateLot(datas[1].ResultDepo, percent, go);
                    if (lotPercent < newLot) lotPercent = newLot;

                    // 3 стратегия
                    datas[2].ResultDepo += (take - comiss) * lotProgress; 
                    lotProgress = CalculateLot(depoStart, minStartPercent * multiply, go);

                    // 4 cтратегия
                    datas[3].ResultDepo += (take - comiss) * lotDown;
                    lotDown = startLot;
                } 
                else
                {
                    // Сделка убыточная
                    // 1 стратегия
                    datas[0].ResultDepo -= (stop + comiss) * startLot;

                    //2 страегия
                    datas[1].ResultDepo -= (stop + comiss) * lotPercent;

                    // 3 стратегия
                    datas[2].ResultDepo -= (stop + comiss) * lotProgress;
                    lotProgress = CalculateLot(depoStart, minStartPercent * multiply, go);

                    // 4 cтратегия
                    datas[3].ResultDepo -= (stop + comiss) * lotDown;
                    lotDown /= 2;
                    if (lotDown == 0) lotDown = 1;
                }
            }
            _dataGrid.ItemsSource = datas;
            //return datas;
        }

        private int CalculateLot (decimal currentDepo, decimal percent, decimal go)
        {
            if (percent > 100) { percent = 100;  };
            decimal lot = currentDepo / go / 100 * percent;
            return (int)lot;
        }

        decimal StringToDecimal (string num_string)
        {
            if (decimal.TryParse(num_string, out decimal num_decimal)) return num_decimal;
            return 0;
        }
        #endregion

        int StringToInt (string num_string)
        {
            if (int.TryParse(num_string, out int num_int)) return num_int;
            return 0;
        }

        private void Draw ()
        {
            // Очищаем Canvas перед перерисовкой
            _canvas.Children.Clear();

            int index = _comboBox.SelectedIndex;

            if (datas.Any())
            {
                List<decimal> listEqity = datas[index].GetListEqity();

                int count = listEqity.Count;

                double maxEquity = (double)listEqity.Max();
                double minEquity = (double)listEqity.Min();

                int Margin = 10; // Отступ от краев

                double canvasWidth = _canvas.ActualWidth - Margin * 2;
                double canvasHeight = _canvas.ActualHeight - Margin * 2;

                double xStep = canvasWidth / (count - 1);
                double yKoef = (double)(maxEquity - minEquity) / canvasHeight;


                for (int i = 0; i < count - 1; ++i)
                {
                    Line line = new Line()
                    {
                        X1 = Margin + i * xStep,
                        Y1 = Margin + canvasHeight - ((double)listEqity[i] - minEquity) / yKoef,
                        X2 = Margin + (i + 1) * xStep,
                        Y2 = Margin + canvasHeight - ((double)listEqity[i + 1] - minEquity) / yKoef,
                        Stroke = Brushes.Blue,
                        StrokeThickness = 2
                    };
                    _canvas.Children.Add(line);

                }
            }
            


        }

      
    }

}
 