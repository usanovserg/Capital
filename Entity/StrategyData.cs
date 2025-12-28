using Capital.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capital.Entity
{
    internal class StrategyData
    {
        #region Properties ===================================
        public StrategyType StrategyType { get; set; }
        public decimal StartDepo { get; set; }
        public decimal ResultDepo { get; set; }
        public decimal Profit { get => ResultDepo - StartDepo; }
        public decimal PersentProfit { get => Profit * 100 / StartDepo; }
        public decimal MaxDrawdown { get; set; }
        public decimal PercentMaxDrawdown { get; set; }
        #endregion


        #region Fields =======================================
        #endregion


        public StrategyData(StrategyType strategyType, decimal startDepo)
        {
            StrategyType = strategyType;
            StartDepo = startDepo;
            ResultDepo = startDepo;
        }


        #region Methods =====================================
        #endregion
    }
}
