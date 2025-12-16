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

        public decimal ResultDepo
        {
            get => _resultDepo;

            set
            {
                _resultDepo = value;
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
            get { return _maxDrownDown; }

            set { _maxDrownDown = value; }
        }
        decimal _maxDrownDown;

        /// <summary>
        /// Максимальная относительная просадка в процентах
        /// </summary>
        public decimal PercentDrawnDown { get; set; }


        #endregion

        #region Fields ====================================================

        #endregion

        #region Methods ===================================================

        #endregion
    }
}
