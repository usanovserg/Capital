using Capital.Entity;
using Capital.Enums;
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


        #region Fields ====================================================================================

        List<StrategyType> _strategies = new List<StrategyType>()
        {
            StrategyType.FIX,
            StrategyType.CAPITALISATION,
            StrategyType.PROGRESS,
            StrategyType.DOWNGRADE
        };


        Random _random = new Random();

        #endregion Fields




        #region Methods ====================================================================================


        private void Init()
        {
            _comboBox.ItemsSource = _strategies;


            //// способ 2
            //List<StrategyType> strategyTypes = new List<StrategyType>();
            //strategyTypes.Add(StrategyType.FIX);
            //strategyTypes.Add(StrategyType.CAPITALISATION);
            //strategyTypes.Add(StrategyType.PROGRESS);
            //strategyTypes.Add(StrategyType.DOWNGRADE);
            //_comboBox.ItemsSource = strategyTypes;

            _comboBox.SelectionChanged += _comboBox_SelectionChanged;
            _comboBox.SelectedIndex = 0;

            _depo.Text = "100000";
            _startLot.Text = "10";
            _take.Text = "300";
            _stop.Text = "100";
            _commis.Text = "5";
            _countTrades.Text = "1000";
            _percentProfit.Text = "30";
            _go.Text = "5000";
            _minStartPersent.Text = "20";

        }

        private void _comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // способ 1
            //ComboBox? comboBox = sender as ComboBox;

            // способ 2
            ComboBox comboBox = (ComboBox)sender;

            int index = comboBox.SelectedIndex;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Calculate();
        }


        private void Calculate()
        {
            decimal depoStart = GetDecimalFromString(_depo.Text);
            int startLot = GetIntFromString(_startLot.Text);
            decimal take = GetDecimalFromString(_take.Text);
            decimal stop = GetDecimalFromString(_stop.Text);
            decimal comiss = GetDecimalFromString(_commis.Text);
            int countTrades = GetIntFromString(_countTrades.Text);
            decimal percProfit = GetDecimalFromString(_percentProfit.Text);
            decimal minStartPercent = GetDecimalFromString(_minStartPersent.Text);
            decimal go = GetDecimalFromString(_go.Text);

            List<Data> datas = new List<Data>();

            //// способ 1 - через хард код
            //for (int i = 0; i < 4; i++)
            //{
            //    Data data = new Data();
            //    data.StrategyType = _strategies[i];
            //    datas.Add(new Data());
            //}

            //// способ 2 - гораздо лучше
            //foreach (StrategyType type in _strategies)
            //{
            //    Data data = new Data();
            //    data.StrategyType = type;
            //    datas.Add(new Data());
            //}

            // способ 3 ещё покороче. В классе Data добавили конструктор
            foreach (StrategyType type in _strategies)
            {
                ////способ 1
                //Data data = new Data(type);                
                //datas.Add(new Data());

                // способ 2 покороче
                datas.Add(new Data(depoStart, type));
            }


            int lotPercent = startLot;

            decimal percent = startLot * go * 100 / depoStart;

            decimal multiply = take / stop;
            int lotProgress = CalculateLot(depoStart, minStartPercent, go);

            int lotDown = startLot;

            //decimal minDepo = depoStart;





            // цикл, который будет создавать рандомные сделки
            for (int i = 0; i < countTrades; i++)
            {
                int rnd = _random.Next(1, 100);     // эти цифры в проентах

                if (rnd <= percProfit)  // если сделка прибыльная
                {
                    // стратегия 1
                    datas[0].ResultDepo += (take - comiss) * startLot;

                    // стратегия 2
                    datas[1].ResultDepo += (take - comiss) * lotPercent;
                    int newLot = CalculateLot(datas[1].ResultDepo, percent, go);
                    if (lotPercent < newLot)
                    {
                        lotPercent = newLot;
                    }

                    // стратегия 3
                    datas[2].ResultDepo += (take - comiss) * lotProgress;
                    lotProgress = CalculateLot(depoStart, minStartPercent * multiply, go);

                    // стратегия 4
                    datas[3].ResultDepo += (take - comiss) * lotDown;
                    lotDown = startLot;

                }
                else    // иначе сделка убыточная
                {
                    // стратегия 1
                    datas[0].ResultDepo -= (stop + comiss) * startLot;
                    // стратегия 2
                    datas[1].ResultDepo -= (stop + comiss) * lotPercent;
                    // стратегия 3
                    datas[2].ResultDepo -= (stop + comiss) * lotProgress;
                    lotProgress = CalculateLot(depoStart, minStartPercent, go);
                    // стратегия 4
                    datas[3].ResultDepo -= (stop + comiss) * lotDown;
                    lotDown /= 2;
                    if (lotDown == 0)
                        lotDown = 1;

                }

                //// столбец Profit
                //datas[0].Profit = datas[0].ResultDepo - datas[0].Depo;
                //datas[1].Profit = datas[1].ResultDepo - datas[1].Depo;
                //datas[2].Profit = datas[2].ResultDepo - datas[2].Depo;
                //datas[3].Profit = datas[3].ResultDepo - datas[3].Depo;

                //// столбец PercentProfit
                //datas[0].PercentProfit = Math.Round(datas[0].Profit / datas[0].Depo * 100, 2);
                //datas[1].PercentProfit = Math.Round(datas[1].Profit / datas[1].Depo * 100, 2);
                //datas[2].PercentProfit = Math.Round(datas[2].Profit / datas[2].Depo * 100, 2);
                //datas[3].PercentProfit = Math.Round(datas[3].Profit / datas[3].Depo * 100, 2);


                //// столбец MaxDrawDown
                //// стратегия 1 в лоб

                //decimal peak1 = depoStart;
                //decimal drawDown1 = 0;
                //decimal maxDrawDown1 = 0;

                //if (datas[0].ResultDepo > peak1)
                //{
                //    peak1 = datas[0].ResultDepo;
                //}
                //else
                //{
                //    drawDown1 = peak1 - datas[0].ResultDepo;
                //}
                //if (drawDown1 > maxDrawDown1)
                //{
                //    maxDrawDown1 = drawDown1;
                //    datas[0].MaxDrawDown = maxDrawDown1;
                //    datas[0].PercentDrawDown = (peak1 - datas[0].ResultDepo) / peak1 * 100;
                //}

                // стратегия 1 через методы
                //datas[0].MaxDrawDown = MaxDrawDown(datas[0].ResultDepo, depoStart);
                //datas[0].PercentDrawDown = PercentDrawDown(datas[0].ResultDepo, depoStart);

                //// стратегия 2 в лоб
                //decimal peak2 = depoStart;
                //decimal drawDown2 = 0;
                //decimal maxDrawDown2 = 0;
                //if (datas[1].ResultDepo > peak2)
                //{
                //    peak2 = datas[1].ResultDepo;
                //}
                //else
                //{
                //    drawDown2 = peak2 - datas[1].ResultDepo;
                //}
                //if (drawDown2 > maxDrawDown2)
                //{
                //    maxDrawDown2 = drawDown2;
                //    datas[1].MaxDrawDown = maxDrawDown2;
                //    datas[1].PercentDrawDown = (peak2 - datas[1].ResultDepo) / peak2 * 100;
                //}





            }


            // в самом конце
            _dataGrid.ItemsSource = datas;
        }




        //private decimal MaxDrawDown(decimal ResultDepo, decimal depoStart)
        //{
        //    decimal peak = depoStart;
        //    decimal drawDown = 0;
        //    decimal maxDrawDown = 0;

        //    if (ResultDepo > peak)
        //    {
        //        peak = ResultDepo;
        //    }
        //    else
        //    {
        //        drawDown = peak - ResultDepo;
        //    }

        //    if (drawDown > maxDrawDown)
        //    {
        //        maxDrawDown = drawDown;
        //        //datas[0].PercentDrawDown = (peak - datas[0].ResultDepo) / peak * 100;
        //    }

        //    return maxDrawDown;
        //}



        //private decimal PercentDrawDown(decimal ResultDepo, decimal depoStart)
        //{
        //    decimal peak = depoStart;
        //    decimal drawDown = 0;
        //    decimal maxDrawDown = 0;

        //    if (ResultDepo > peak)
        //    {
        //        peak = ResultDepo;
        //    }
        //    else
        //    {
        //        drawDown = peak - ResultDepo;
        //    }

        //    if (drawDown > maxDrawDown)
        //    {
        //        maxDrawDown = drawDown;
        //        //datas[0].PercentDrawDown = (peak - datas[0].ResultDepo) / peak * 100;
        //    }

        //    return maxDrawDown / peak * 100;
        //}



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





    }   // class MainWindow : Window
}       // namespace Capital