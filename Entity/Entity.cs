using Capital.Enums;

namespace Capital.Entity
{
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
                
                Profit = ResultDepo - Depo;
                
                PercentProfit = Profit * 100 / Depo;
                
                ListEquity.Add(ResultDepo);

                CalcDrawDown();
            }
        }

        decimal _resultDepo;

        public decimal Profit { get; set; }
        
        /// <summary>
        /// Относительный профит в процентах
        /// </summary>
        public decimal PercentProfit { get; set; }

        /// <summary>
        /// Относительный профит в процентах
        /// </summary>
        public decimal MaxDrawDown
        {
            get => _maxDrawDown;
            set
            {
                _maxDrawDown = value;
                CalcPercentDrawDown();
            }
        }
        decimal _maxDrawDown;

        private void CalcPercentDrawDown()
        {
            decimal percent = MaxDrawDown * 100 / ResultDepo;
            
            if (percent > PercentDrawDown) PercentDrawDown = Math.Round(percent, 2);
        }
        
        /// <summary>
        /// Максимальная относительная просадка в процентах
        /// </summary>
        public decimal PercentDrawDown { get; set; }

        private List<decimal> ListEquity = new List<decimal>();

        private decimal _max = 0;
        
        private decimal _min = 0;
        
        public List<decimal> GetListEquity()
        {
            return ListEquity;
        }

        private void CalcDrawDown()
        {
            if (_max < ResultDepo)
            {
                _max = ResultDepo;
                _min = ResultDepo;
            }

            if (_min > ResultDepo)
            {
                _min = ResultDepo;

                if (MaxDrawDown < _max - _min)
                {
                    MaxDrawDown = _max - _min;
                }
            }
        }

    }
}