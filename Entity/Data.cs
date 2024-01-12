using Capital.Enams;
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

        #region Fields ==================================================================

        List<decimal> ListEquiy = new List<decimal>();

        private decimal _max = 0;

        private decimal _min = 0;

        #endregion

        #region Properties ==================================================================

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

                ListEquiy.Add(ResultDepo);

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
        public decimal MaxDrawDown
        {
            get => _maxDrawDown;

            set
            {
                _maxDrawDown = value;

                CalcPrecentDrawDown();
            }
        }
        decimal _maxDrawDown;

        /// <summary>
        /// Максимальная относительная просадка в процентах
        /// </summary>
        public decimal PercentDrawDown { get; set; }

        #endregion

        #region Methods ==================================================================

        public List<decimal> GetListEquity()
        {
            return ListEquiy;
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

        private void CalcPrecentDrawDown()
        {
            decimal percent = MaxDrawDown * 100 / ResultDepo;

            if (percent > PercentDrawDown) PercentDrawDown = Math.Round(percent, 2);
        }
        #endregion

    }
}
