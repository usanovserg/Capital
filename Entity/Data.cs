using Capital.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input.Manipulations;

namespace Capital.Entity
{
    public class Data
    {
        public Data(decimal depoStart, StrategyType strategyType)
        {
            StrategyType = strategyType;
            Depo = depoStart;
            MaxDrawDown = 0;
            Top= Bottom = depoStart;
            Direction = true;
        }
        #region Fields ==================================================================
        // поля для вычисления MaxDrawDown, MDD
        public decimal Top;
        public decimal Bottom;
        public Boolean Direction;
        #endregion  ==================================================================
        #region Properties ==================================================================

        public StrategyType StrategyType { get; set; }
        public decimal Depo 
        { 
            get => _depo;
            set
            {
                _depo = value;
                ResultDepo= value;
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
        public decimal PerecentProfit {  get; set; }

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

        public decimal PercentDrawDown { get; set;  }

        #endregion         ==================================================================
    }
}
