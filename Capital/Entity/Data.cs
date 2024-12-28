using Capital.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capital.Entity
{
    internal class Data
    {
        public Data (decimal depoStart, StrategyType strategyType)
        {
            StrategyType = strategyType;
            Depo = depoStart;
            _min = depoStart;
            _max = depoStart;
        }


        #region Properties ==================================================================
        #endregion
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
                Profit = value - Depo;
                PercentProfit = Profit / Depo * 100;
                CalculateDrawDown();
                _ListEqity.Add(value);
            }
        }
        decimal _resultDepo;

        public decimal Profit { get; set; }

        /// <summary>
        /// Относительный профит в %
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
                CalculatePercentDrawDown();
            }
        }
        decimal _maxDrawDown;

        /// <summary>
        /// Максимальная относительная просадка в %
        /// </summary>
        public decimal PercentDrawDown { get; set; }
        #region Fields =======================================================================

        List<decimal> _ListEqity = new List<decimal>();
        decimal _min = 0;
        decimal _max = 0;
        #endregion

        #region Methods ======================================================================

        public List<decimal> GetListEqity ()
        {
            return _ListEqity;
        }

        void CalculateDrawDown ()
        {

            if (ResultDepo > _max )
            {
                _max = ResultDepo;
                _min = ResultDepo;
            }

            if (ResultDepo < _min)
            {
                _min = ResultDepo;
                
                if (_max - _min > MaxDrawDown)
                {
                    MaxDrawDown = _max - _min;
                }
            }
        }

        void CalculatePercentDrawDown ()
        {
            decimal percent = MaxDrawDown / ResultDepo * 100;
            if (percent > PercentDrawDown) PercentDrawDown = Math.Round(percent, 2);
        }
        #endregion
    }
}
