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
        public Data(decimal depoStart, StrategyType strategyType)
        {
            StrategyType = strategyType;
            Depo = depoStart;
        }

        #region Properties ===================================================

        public StrategyType StrategyType { get; set; }

        public decimal Depo
        { get => _depo;
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
                PercentProfit = Math.Round(Profit / Depo * 100,2);
                CalcMaxDrawDown();
                ListEquity.Add(ResultDepo);



            }
        }
        decimal _resultDepo;

        public decimal Profit { get; set; }
       
        /// <summary>
        /// Относительный доход в процентах
        /// </summary>
        public decimal PercentProfit { get; set; }

        /// <summary>
        /// Максимальная абсолютная просадка
        /// </summary>
        public decimal MaxDrawDown
        {
            get => _maxDrawDown;
            set
            {
                _maxDrawDown = value;
                CalcPercentMaxDrawDown();
            }
        }
        decimal _maxDrawDown;

        /// <summary>
        /// Максимальная относительная просадка
        /// </summary>
        public decimal PercentDrawDown { get; set; }

        #endregion

        #region Fields ===================================================
       
        List<decimal> ListEquity = new List<decimal>();


        decimal _min = 0;
        decimal _max = 0;

        #endregion

        #region Methods ===================================================

        public List<decimal> GetListEquity()
        {
            return ListEquity;
 
        }

        private void CalcPercentMaxDrawDown()
        {
            
            decimal percent = Math.Round(MaxDrawDown / _max * 100,2);
            if (percent > PercentDrawDown) PercentDrawDown = percent;
        }
        private void CalcMaxDrawDown()
        {
            if (_min == 0 && _max == 0) _min = Depo; _max = Depo;
            if (_max < ResultDepo)
            {
                _max = ResultDepo;
                _min = ResultDepo;
            }
            if (_min > ResultDepo)
            {
                _min = ResultDepo;
                if (MaxDrawDown < _max - _min) MaxDrawDown = _max - _min;
            }
           
        }
        #endregion
    }
}
