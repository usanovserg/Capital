using System.Windows;
using System.Windows.Controls;
using MyCapital.Entity;
using MyCapital.Enums;
using ScottPlot.Plottables;
using Color = System.Drawing.Color;

namespace MyCapital;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Init();
    }

    private readonly Random _random = new Random();
    private int _indexCountCalc = 0;
    private readonly List<Scatter> _plot = [];
    private int _indexCombo;

    private readonly List<StrategyType> _strategies =
    [
        StrategyType.FIX,
        StrategyType.CAPITALIZATON,
        StrategyType.PROGRESS,
        StrategyType.DOWNGRADE
    ];

    private void Init()
    {
        _comboBox.ItemsSource = _strategies;

        _comboBox.SelectionChanged += _comboBox_SelectionChanged;
        _comboBox.SelectedIndex = 0;

        _depo.Text = "100000";
        _startLot.Text = "10";
        _take.Text = "300";
        _stop.Text = "100";
        _comiss.Text = "0.05";
        _countTrades.Text = "1000";
        _percentProfit.Text = "30";
        _minStartPercent.Text = "20";
        _go.Text = "5000";
    }

    private void _comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ComboBox? comboBox = sender as ComboBox;
        _indexCombo = comboBox.SelectedIndex;

        if (_indexCountCalc > 0)
        {
            PlotCalc(_indexCombo, true);
            CheckBoxState(this, null);
        }
    }

    private void CheckBoxState(object sender, EventArgs e)
    {
        CheckBox[] checkBoxArray = [CB_Fix, CB_Cap, CB_Prog, CB_Dawn];

        for (int i = 0; i < checkBoxArray.Length; i++)
        {
            if (checkBoxArray[i] != sender)
                checkBoxArray[i].IsChecked = false;

            if (i == _indexCombo)
            {
                checkBoxArray[i].IsChecked = true;
            }
        }
    }

    private void PlotCalc(int index, bool clear)
    {
        var t1 = _plot[index].Data.GetScatterPoints();
        double[] dataX1 = t1.Select(x => x.X).ToArray();
        double[] dataY1 = t1.Select(x => x.Y).ToArray();

        ScottPlot.Color color = default;

        switch (index)
        {
            case 0:
                color = ScottPlot.Color.FromColor(Color.CornflowerBlue);
                break;
            case 1:
                color = ScottPlot.Color.FromColor(Color.DarkRed);
                break;
            case 2:
                color = ScottPlot.Color.FromColor(Color.DarkGreen);
                break;
            case 3:
                color = ScottPlot.Color.FromColor(Color.Orange);
                break;
        }
        
        if (clear)
        {
            WpfPlot1.Plot.Clear();
        }

        WpfPlot1.Plot.Add.ScatterLine(dataX1, dataY1, color);
        WpfPlot1.Plot.Axes.AutoScale();
        WpfPlot1.Refresh();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        List<Data> datas = Calculate();
        _plot.Clear();
        _indexCountCalc++;
        CheckBox cbDown = new CheckBox();

        Draw(datas);
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
        decimal minStartProcent = GetDecimalFromString(_minStartPercent.Text);
        decimal go = GetDecimalFromString(_go.Text);

        List<Data> datas = new List<Data>();

        foreach (StrategyType type in _strategies)
        {
            datas.Add(new Data(depoStart, type));
        }

        int lotPercent = startLot;
        decimal percent = startLot * go * 100 / depoStart;

        decimal multyplay = take / stop;
        int lotProgress = CalculateLot(depoStart, minStartProcent, go);
        int lotDown = startLot;

        for (int i = 0; i < countTrades; i++)
        {
            int rnd = _random.Next(0, 100);

            
            if (rnd <= percProfit)
            {
                //Сделка прибыльная

                // 1-я стратегия
                datas[0].ResultDepo += (take - comiss) * startLot;

                // 2-я стратегия
                datas[1].ResultDepo += (take - comiss) * lotPercent;
                int newLot = CalculateLot(datas[1].ResultDepo, percent, go);
                if (lotPercent < newLot) lotPercent = newLot;

                // 3-я стратегия
                datas[2].ResultDepo += (take - comiss) * lotProgress;
                lotProgress = CalculateLot(depoStart, minStartProcent * multyplay, go);

                // 4-я стретегия
                datas[3].ResultDepo += (take - comiss) * lotDown;
                lotDown = startLot;
            }
            else
            {
                //Сделка убыточная

                // 1-я стратегия
                datas[0].ResultDepo -= (stop + comiss) * startLot;

                // 2-я стратегия
                datas[1].ResultDepo -= (stop + comiss) * lotPercent;

                // 3-я стратегия
                datas[2].ResultDepo -= (stop + comiss) * lotProgress;
                lotProgress = CalculateLot(depoStart, minStartProcent, go);

                // 4-я стретегия
                datas[3].ResultDepo -= (stop + comiss) * lotDown;
                lotDown /= 2;
                if (lotDown == 0) lotDown = 1;
            }
        }

        _dataGrid.ItemsSource = datas;

        return datas;
    }

    private void Draw(List<Data> datas)
    {
        for (int j = 0; j < _comboBox.Items.Count; j++)
        {
            List<decimal> listEquity = datas[j].GetListEquity();
            int count = listEquity.Count;

            double[] dataX = new double[listEquity.Count];
            double[] dataY = new double[listEquity.Count];

            for (int i = 0; i < count; i++)
            {
                dataX[i] = i;
                dataY[i] = (double)(listEquity[i]);
            }
            _plot.Add(WpfPlot1.Plot.Add.ScatterLine(dataX, dataY));
        }

        IsCheckBoxesChecked();
    }

    private int CalculateLot(decimal currentDepo, decimal percent, decimal go)
    {
        if (percent > 100) { percent = 100; }

        decimal lot = currentDepo / go / 100 * percent;

        return (int)lot;
    }

    private decimal GetDecimalFromString(string str)
    {
        if (str.Contains("."))
        {
            str = str.Replace(".", ",");
        }
        return decimal.TryParse(str, out decimal result) ? result : 0;
    }

    private int GetIntFromString(string str)
    {
        return int.TryParse(str, out int result) ? result : 0;
    }

    // Чекбоксы

    private void CB_Checked(object sender, RoutedEventArgs e)
    {
        if (_indexCountCalc > 0)
        {
            IsCheckBoxesChecked();
        }
    }

    private void IsCheckBoxesChecked()
    {
        WpfPlot1.Plot.Clear();

        CheckBox[] checkBoxArray = [CB_Fix, CB_Cap, CB_Prog, CB_Dawn];

        for (int i = 0; i < checkBoxArray.Length; i++)
        {
            if ((bool)checkBoxArray[i].IsChecked!)
            {
                PlotCalc(i, false);
            }
        }
    }
}
