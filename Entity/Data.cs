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

        #region Propertirs =================================

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

                LisrEquity.Add(ResultDepo);

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
        /// Максимальная абсолбтная просадка в деньгах
        /// </summary>
        public decimal MaxDrawDoun
        {
            get => _maxDrawDoun;

            set
            {
                _maxDrawDoun = value;

                CalcPercentDown();
            }
        }
        decimal _maxDrawDoun;

        /// <summary>
        /// Максимальная относительная просадка в процентах
        /// </summary>
        public decimal PercentDrawDown { get; set; }

        #endregion

        #region Fields =========================================

        List<decimal> LisrEquity = new List<decimal>();

        private decimal _max = 0;

        private decimal _min = 0;


        #endregion

        #region Methods ==========================================

        public List<decimal> GetListEquity()
        {
            return LisrEquity;
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

                if(MaxDrawDoun < _max - _min)
                {
                    MaxDrawDoun = _max - _min;
                }
            }
        }

        private void CalcPercentDown()
        {
            if (ResultDepo == 0) ResultDepo = 1;
            decimal percent = MaxDrawDoun * 100 / ResultDepo;

            if (percent > PercentDrawDown) PercentDrawDown = Math.Round(percent, 2);
        }

        #endregion
    }
}
