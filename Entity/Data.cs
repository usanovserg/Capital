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

        #region Properties=================================================

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

        /// <summary>
        /// Результат эквити (депо)
        /// </summary>
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
        /// Максимальная абсолютная просадка в деньгах 
        /// </summary>
        public decimal MaxDrownDown
        {
            get => _maxDrownDown; 

            set 
            { 
                _maxDrownDown = value; 
                CalcPercentDrawDown(); 
            }
        }
        decimal _maxDrownDown;

        /// <summary>
        /// Максимальная относительная просадка в процентах
        /// </summary>
        public decimal PercentDrawnDown { get; set; }


        #endregion

        #region Fields ====================================================

        List<decimal> ListEquity = new List<decimal>();

        private decimal _max = 0;

        private decimal _min = 0;

        #endregion

        #region Methods ===================================================

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

                if (MaxDrownDown < _max - _min)
                {
                    MaxDrownDown = _max - _min;
                }             

            }
        }

        private void CalcPercentDrawDown()
        {
            decimal percent = MaxDrownDown * 100 / ResultDepo; 
            
            if (percent > PercentDrawnDown) PercentDrawnDown = Math.Round(percent, 2);
        }

        #endregion
    }
}
