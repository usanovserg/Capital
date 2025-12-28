using Capital.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace Capital.Entity
{
    internal class StrategyData
    {
        #region Fields =======================================
        private decimal _resultDepo;
        private List<decimal> _equity = new List<decimal>();
        private decimal _min = 0;
        private decimal _max = 0;
        #endregion


        #region Properties ===================================
        public StrategyType StrategyType { get; set; }
        public decimal StartDepo { get; set; }
        public decimal ResultDepo
        { 
            get { return _resultDepo; }
            set 
            { 
                _resultDepo = value;
                _equity.Add(value);
                Profit = value - StartDepo; 
                PersentProfit = Math.Round(Profit * 100 / StartDepo, 2);
                CalculateDrawdown();
                CalculatePercentDrawdown();
            } 
        }
        public decimal Profit { get; set; }
        public decimal PersentProfit { get; set; }
        public decimal MaxDrawdown { get; set; }
        public decimal PercentMaxDrawdown { get; set; }
        #endregion


        public StrategyData(StrategyType strategyType, decimal startDepo)
        {
            StrategyType = strategyType;
            StartDepo = startDepo;
            ResultDepo = startDepo;
        }


        #region Methods =====================================
        public List<decimal> GetEquity()
        {
            return _equity;
        }

        private void CalculateDrawdown()
        {
            if (_max < ResultDepo)
            {
                _max = ResultDepo;
                _min = ResultDepo;
            }
            if (_min > ResultDepo)
            {
                _min = ResultDepo;
                if (MaxDrawdown < _max - _min)
                    MaxDrawdown = _max - _min;
            }
        }

        private void CalculatePercentDrawdown()
        {
            var percent = MaxDrawdown * 100 / ResultDepo;
            if (percent > PercentMaxDrawdown)
                PercentMaxDrawdown = Math.Round(percent, 2);
        }
        #endregion
    }
}
