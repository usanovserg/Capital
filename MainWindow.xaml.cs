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

        List<Data> datas;

        List<StrategyType> _strategies = new List<StrategyType>()
        {
                StrategyType.FIX,
                StrategyType.CAPITALIZATION,
                StrategyType.PROGRESS,
                StrategyType.DOWNGRADE,
                StrategyType.ALL
        };

        Random _random = new Random();

        #endregion


        #region Methods ===========================================

        private void Init()
        {
            //_combobox.ItemsSource = new List<StrategyType>();
            _combobox.ItemsSource = _strategies ;

            _combobox.SelectionChanged += _combobox_SelectionChanged; //Подписались на событие
            _combobox.SelectedIndex = 0;

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

        private void _combobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            
            int index = comboBox.SelectedIndex;

            if (datas != null)  Draw(datas);

        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            List<Data> datas =  Calculate();

            Draw(datas);
        }

        private void Canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {            
            if (datas != null) Draw(datas);
                         
        }

        private List<Data> Calculate()
        {
            decimal depoStart = GetDecimalFromString( _depo.Text );
            int startLot = GetIntFromString( _startLot.Text );
            decimal take = GetDecimalFromString( _take.Text );
            decimal stop = GetDecimalFromString( _stop.Text );
            decimal comiss = GetDecimalFromString( _comiss.Text );
            int countTrades = GetIntFromString(_countTrades.Text );
            decimal percProfit = GetDecimalFromString(_percentProfit.Text );
            decimal minStartPercent = GetDecimalFromString(_minStartPercent.Text);
            decimal go = GetDecimalFromString( _go.Text );

            datas = new List<Data>();

           
            foreach (StrategyType type in _strategies)
            {
                datas.Add(new Data(depoStart, type));
            }

            int lotPercent = startLot;
            decimal percent = startLot * go * 100 / depoStart;

            decimal multiplay = take / stop;
            int lotProgress = CalculateLot(depoStart, minStartPercent, go);

            int lotDown = startLot;
            
            for (int i = 0; i < countTrades; i++)
            {
                int rnd = _random.Next(1, 100);

                if (rnd < percProfit)
                {
                    //Сделка прибыльная

                    //========================= 1 Strategy =========================== 
                    datas[0].ResultDepo += (take - comiss) * startLot;

                    //========================= 2 Strategy ===========================
                    datas[1].ResultDepo += (take - comiss) * lotPercent;
                    
                    int newLot = CalculateLot(datas[1].ResultDepo, percent, go);

                    if(lotPercent < newLot) lotPercent = newLot;

                    //========================= 3 Strategy =========================== 
                    datas[2].ResultDepo += (take - comiss) * lotProgress;

                    lotProgress = CalculateLot(depoStart, minStartPercent * multiplay, go);

                    //========================= 4 Strategy =========================== 
                    datas[3].ResultDepo += (take - comiss) * lotDown;

                    lotDown = startLot;
                }
                else
                {
                    //Сделка убыточная
                    //========================= 1 Strategy =========================== 
                    datas[0].ResultDepo -= (stop + comiss) * startLot;

                    //========================= 2 Strategy =========================== 
                    datas[1].ResultDepo -= (stop + comiss) * lotPercent;

                    //========================= 3 Strategy =========================== 
                    datas[2].ResultDepo -= (stop + comiss) * lotProgress;
                    
                    lotProgress = CalculateLot(depoStart, minStartPercent, go);                      
                    //========================= 4 Strategy =========================== 
                    datas[3].ResultDepo -= (stop + comiss) * lotDown;

                    lotDown /= 2;

                    if(lotDown == 0) lotDown = 1;
                }

            }

            _dataGrid.ItemsSource = datas; //Запись в таблицу

            return datas;
        }

        
        private void Draw(List<Data> datas)
        {
            _canvas.Children.Clear();

            int index = _combobox.SelectedIndex;  //Получаем индекс выбранного комбобокса.

            List<decimal> ListEquity = datas[index].GetListEquity();

            int count = ListEquity.Count; //Количество элементов
            decimal maxEquity = ListEquity.Max(); //Макс. значение элемента
            decimal minEquity = ListEquity.Min(); //Мин. значение

            double stepX = _canvas.ActualWidth / count;

            double koef = (double)(maxEquity - minEquity) / _canvas.ActualHeight; //К-т масштабирования по вертикали

            double x = 0; //текущие 
            double y = 0; //координаты

            for (int i = 0; i < count; i++)
            {
                double x_old = x;
                double y_old = y;

                y = _canvas.ActualHeight - (double)(ListEquity[i] - minEquity) / koef;
                               

                Line line = new Line();
                
                line.X1 = x_old;
                line.Y1 = y_old;
                line.X2 = x;
                line.Y2 = y;

                line.Stroke = Brushes.Black;
                line.StrokeThickness = 1;

                _canvas.Children.Add(line);

                x += stepX;
            }
        }
        //private void Draw(List<Data> datas)
        //{
        //    _canvas.Children.Clear();


        //    int index = _combobox.SelectedIndex;  //Получаем индекс выбранного комбобокса.

        //    if (StrategyType.ALL != (StrategyType)index) //Если выбрана любая стратегия кроме ВСЕХ
        //    {
        //        //  DrawOne(datas, index);
        //    }
        //    else                                         //Тут будем выводить все графики на один Canvas 
        //    {
        //        decimal maxEquity = 0;
        //        decimal minEquity = 0;

        //        int countOfEnum = Enum.GetValues<StrategyType>().Length;

        //        for (int i = 0; i < countOfEnum - 1; i++) //Считаем параметры (min, max. koeff) по всем стратегиям одновременно 
        //        {
        //            ListEquity = datas[index].GetListEquity();

        //            if (maxEquity < ListEquity.Max()) maxEquity = ListEquity.Max();  //Макс. значение элемента из всех стратегий
        //            if (minEquity > ListEquity.Min()) ; minEquity = ListEquity.Min(); //Мин. значение элемента из всех стратегий

        //        }

        //        //  int count = ListEquity.Count; //Количество элементов
        //        //  double stepX = _canvas.ActualWidth / count;  

        //        double koef = (double)(maxEquity - minEquity) / _canvas.ActualHeight; //К-т масштабирования по вертикали

        //        for (int i = 0; i < countOfEnum - 1; i++)
        //        {
        //            DrawOne(ListEquity, i, minEquity, koef);
        //        }


        //    }
        //}
        private void DrawOne(List<decimal> ListEquity, int index, decimal minEquity, double koef)
        {

            int count = ListEquity.Count; //Количество элементов
            //decimal maxEquity = ListEquity.Max(); //Макс. значение элемента
            //decimal minEquity = ListEquity.Min(); //Мин. значение

            double stepX = _canvas.ActualWidth / count;

            //double koef = (double)(maxEquity - minEquity) / _canvas.ActualHeight; //К-т масштабирования по вертикали

            double x = 0; //текущие 
            double y = 0; //координаты

            for (int i = 0; i < count; i++)
            {
                double x_old = x;
                double y_old = y;

                if (Math.Abs(koef) < double.Epsilon)
                {
                    y = 0;
                }
                else
                {
                    y = _canvas.ActualHeight - (double)(ListEquity[i] - minEquity) / koef;
                }

                Line line = new Line();

                line.X1 = x_old;
                line.Y1 = y_old;
                line.X2 = x;
                line.Y2 = y;

                line.Stroke = Brushes.Black;
                line.StrokeThickness = 1;

                _canvas.Children.Add(line);

                x += stepX;
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
            if(int.TryParse(str, out int result)) return result;

            return 0;
        }
        #endregion

    }
}