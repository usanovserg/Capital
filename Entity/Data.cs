using Capital.Enums;

namespace Capital.Entity;

public class Data
{
    public Data(decimal depoStart, StrategyType strategyType)
    {
        StrategyType = strategyType;
        Depo = depoStart;
    }

    public StrategyType StrategyType { get; set; }

    public decimal Depo
    {
        get => _depo;

        set
        {
            _depo = value;
            ResultDepo = value;
        }
    }
    decimal _depo;

    public decimal ResultDepo
    {
        get => _resultDepo;

        set
        {
            _resultDepo = value;
        }
    }

    decimal _resultDepo;

    public decimal Profit { get; set; }

    /// <summary>
    /// Относительный профит в процентах
    /// </summary>
    public decimal PercentProfit { get; set; }

    public decimal MaxDrawDown
    {
        get => _maxDrawDown;

        set
        {
            _maxDrawDown = value;
        }
    }

    decimal _maxDrawDown;

    /// <summary>
    /// Максимальная относительная просадка в процентах
    /// </summary>
    public decimal PercentDrawDown { get; set; }
}