using Capital.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capital.Entity
{
    public class Data
    {
            public Data(decimal DepoStart, StrategyType strategyType)
        {
            this.StrategyType = strategyType;

            this.Depo = DepoStart;
        }

        #region Properties =========================================================
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

        public decimal Profit { get; set;  }    

        /// <summary>
        /// Относительный профит в процентах
        /// </summary>
        public decimal PercentProfit { get; set; }

        /// <summary>
        /// Максимальная просадка в деньгах
        /// </summary>
        public decimal MaxDrawDown
        {
            get => _maxDrawDown;

            set { _maxDrawDown = value; }
        }

        decimal _maxDrawDown;

        public decimal PercentDrawDown { get; set;  }
        #endregion
    };



}
