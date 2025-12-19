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

        #region Properties #############################################

        public StrategyType StrategyType { get; set; }

        /// <summary>
        /// До сделки
        /// </summary>
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
        /// осле сделки
        /// </summary>
        public decimal ResultDepo 
        {
            get => _resultDepo ;

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
        public decimal MaxDrawDown
        {
            get => _maxDrawDown; 

            set
            {
                _maxDrawDown = value;
            }
        }
        private decimal _maxDrawDown;

        /// <summary>
        /// Максимальная просадка в процентах
        /// </summary>
       public decimal PercentDrawDown { get; set; }

        #endregion ######################################################


    }
}
