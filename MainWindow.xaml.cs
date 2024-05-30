using System.Windows;
using System.Windows.Controls;
using Capital.Entity;
using Capital.Enums;

namespace Capital;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        Init();
    }

    private readonly List<StrategyType> _strategies = new List<StrategyType>()
    {
        StrategyType.Fix,
        StrategyType.Capitalization,
        StrategyType.Progress,
        StrategyType.Downgrade
    };



    private readonly Random _random = Random.Shared;


    private void Init()
    {
        _comboBox.ItemsSource = _strategies;
        _comboBox.SelectionChanged += ComboBox_SelectionChanged;
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

    private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var comboBox = (ComboBox)sender;
        var index = comboBox.SelectedIndex;
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        Calculate();
    }

    private void Calculate()
    {
        var depoStart = GetDecimalFromString(_depo.Text);
        var startLot = GetIntFromString(_startLot.Text);
        var take = GetDecimalFromString(_take.Text);
        var stop = GetDecimalFromString(_stop.Text);
        var comiss = GetDecimalFromString(_comiss.Text);
        var countTrades = GetIntFromString(_countTrades.Text);
        var percProfit = GetDecimalFromString(_percentProfit.Text);
        var minStartPercent = GetDecimalFromString(_minStartPercent.Text);
        var go = GetDecimalFromString(_go.Text);

        var datas = _strategies.Select(type => new Data(depoStart, type)).ToList();

        var lotPercent = startLot;
        var percent = startLot * go * 100 / depoStart;
        var multiply = take / stop;
        var lotProgress = CalculateLot(depoStart, minStartPercent, go);
        var lotDown = startLot;

        for (var i = 0; i < countTrades; i++)
        {
            var rnd = _random.Next(1, 100);

            if (rnd <= percProfit)
            {
                // Сделка прибыльная

                // 1 strategy
                datas[0].ResultDepo += (take - comiss) * startLot;

                // 2 strategy
                datas[1].ResultDepo += (take - comiss) * lotPercent;

                var newLot = CalculateLot(datas[1].ResultDepo, percent, go);

                if (lotPercent < newLot) lotPercent = newLot;

                // 3 strategy
                datas[2].ResultDepo += (take - comiss) * lotProgress;

                lotProgress = CalculateLot(depoStart, minStartPercent * multiply, go);

                // 4 strategy
                datas[3].ResultDepo += (take - comiss) * lotDown;

                lotDown = startLot;
            }
            else
            {
                // Сделка убыточная

                // 1 strategy
                datas[0].ResultDepo -= (stop + comiss) * startLot;

                // 2 strategy
                datas[1].ResultDepo -= (stop + comiss) * lotPercent;

                // 3 strategy
                datas[2].ResultDepo -= (stop + comiss) * lotProgress;

                lotProgress = CalculateLot(depoStart, minStartPercent, go);

                // 4 strategy
                datas[3].ResultDepo -= (stop + comiss) * lotDown;

                lotDown /= 2;

                if (lotDown == 0) lotDown = 1;
            }
        }

        _dataGrid.ItemsSource = datas;
    }

    private int CalculateLot(decimal currentDepo, decimal percent, decimal go)
    {
        if (percent > 100) percent = 100;


        var lot = currentDepo / go / 100 * percent;

        return (int)lot;
    }

    private decimal GetDecimalFromString(string str) => decimal.TryParse(str, out var result) ? result : 0;

    private int GetIntFromString(string str) => int.TryParse(str, out var result) ? result : 0;
}