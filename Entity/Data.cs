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
        public Data(StrategyType strategyType, decimal startDepo)
        {
            StrategyType = strategyType;
            Depo = startDepo;
            ResultDepo = startDepo;
        }
        
        #region Fields

        #endregion

        #region Properties
        public StrategyType StrategyType { get; set; }
        public decimal Depo 
        {
            get => _depo;
            set => _depo = value;
        } decimal _depo=0;
        public decimal ResultDepo
        {
            get => _resultdepo;
            set => _resultdepo = value;
        }decimal _resultdepo = 0;
        

        public decimal Profit
        {
            get => _profit;
            set => _profit = value;
        }decimal _profit = 0;

        /// <summary>
        /// Относительный профит в процентах
        /// </summary>
        public decimal PercentProfit
        {
            get => _percentprofit;
            set => _percentprofit = value;
        }decimal _percentprofit = 0;
        /// <summary>
        /// Максимальная просадка в деньгах (абсолютная)
        /// </summary>
        public decimal MaxDrawDown
        {
            get => _maxdrawdown;
            set => _maxdrawdown = value;
        }decimal _maxdrawdown = 0;
        /// <summary>
        /// Просадка в процентах
        /// </summary>
        public decimal PercentDrawDown
        {
            get => _percentdrawdown;
            set => _percentdrawdown = value;
        }decimal _percentdrawdown = 0;


        #endregion End_Properties

        #region Methods

        #endregion
    }
}
