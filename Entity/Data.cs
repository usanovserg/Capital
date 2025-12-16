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
            Depo = depoStart;
            StrategyType = strategyType;
        }




        #region Properties ========================================================

        public StrategyType StrategyType { get; set; }



        public decimal Depo
        {
            get { return _depo; }
            set
            {
                _depo = value;
                ResultDepo = value;
            }
        }
        private decimal _depo;



        public decimal ResultDepo
        {
            get { return _resultDepo; }
            set
            {
                _resultDepo = value;
            }
        }
        private decimal _resultDepo;



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
            }
        }
        decimal _maxDrawDown;



        /// <summary>
        /// Максимальная относительная просадка в процентах
        /// </summary>
        public decimal PercentDrawDown { get; set; }


        #endregion Properties



        #region Fields ========================================================

        #endregion Fields


        #region Methods ========================================================

        #endregion Methods



    }
}
