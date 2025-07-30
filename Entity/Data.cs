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
            _minDepo = depoStart;
            _maxDepo = depoStart;    
        }

        #region Properties =================================

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
        public decimal MaxDepo
        {
            get => _maxDepo;

            set
            {
                _maxDepo = (_maxDepo >= value) ? _maxDepo : value;
            }
        }
        decimal _maxDepo;
        public decimal MinDepo
        {
            get => _minDepo;

            set
            {
                _minDepo = (_minDepo <= value) ? _minDepo : value;
            }
        }
        decimal _minDepo;

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

                MaxDepo = value;
                MinDepo = value;

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

        List<decimal> ListEquity = new List<decimal>();

        private decimal _max = 0;

        private decimal _min = 0;


        #endregion

        #region Methods ==========================================

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
