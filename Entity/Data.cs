using Capital.Enams;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

            _depo = depoStart;

            ResultDepo = depoStart;
        }

        #region Properties ============================================================================================

        [DisplayName("Стратегия")]
        public StrategyType StrategyType { get; set; }

        private readonly decimal _depo;

        /// <summary>
        /// Результат эквити (депо)
        /// </summary>
        [DisplayName("Результат")]
        public decimal ResultDepo
        {
            get => _resultDepo;

            set
            {
                _resultDepo = value;

                ListEquity.Add(ResultDepo);

                CalcDrawDown();                  
            }
        }
        decimal _resultDepo;

        [DisplayName("Профит")]
        public decimal Profit { get => ResultDepo - _depo; }


        /// <summary>
        /// Относительный профит в процентах
        /// </summary>
        [DisplayName("Профит в процентах")]
        public decimal PercentProfit { get => Profit * 100 / _depo; }

        /// <summary>
        /// Максимальная абсолютная просадка в деньгах
        /// </summary>
        [DisplayName("Просадка в деньгах")]
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

        /// <summary>
        /// Максимальная относительная просадка в процентах
        /// </summary>
        [DisplayName("Просадка в процентах")]
        public decimal PercentDrawDown { get; set; }

        #endregion

        #region Fields ================================================================================================

        private readonly List<decimal> ListEquity = [];

        private decimal _max = 0;

        private decimal _min = 0;

        #endregion

        #region Methods ===============================================================================================

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

        private void CalcPercentDrawDown()
        {
            if (ResultDepo == 0) ResultDepo = 1;
            decimal percent = MaxDrawDown * 100 / ResultDepo;

            if (percent > PercentDrawDown) PercentDrawDown = Math.Round(percent, 2);

        }
            
        #endregion
    }
}
