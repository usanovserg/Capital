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
        List<decimal> ListEquity = new List<decimal>();
        decimal MaxResultDepo;

        #region Properties =========================================================
        public StrategyType StrategyType { get; set; }

        public decimal Depo
        {
            get => _depo;

            set 
            { 
                _depo = value;
                ResultDepo = value;
                MaxResultDepo = value;
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
                PercentProfit = Profit*100/Depo;
                ListEquity.Add(ResultDepo);
                CalcDrawDown();
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

            set { _maxDrawDown = value;
                CalcPercentDrawDown();
            }
        }

        decimal _maxDrawDown;

        public decimal PercentDrawDown { get; set;  }

        private void CalcDrawDown() 
        {
            MaxResultDepo = Math.Max(MaxResultDepo, ResultDepo);
            MaxDrawDown = Math.Min(MaxDrawDown, ResultDepo - MaxResultDepo);

        }

        private void CalcPercentDrawDown() 
        {
            PercentDrawDown = Math.Min(PercentDrawDown, (ResultDepo - MaxResultDepo) / MaxResultDepo * 100);

        }

        public List<decimal> GetListEquity() 
        { 
            return ListEquity;
        
        }
        #endregion
    };



}
