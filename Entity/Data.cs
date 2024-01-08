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
        List<decimal> ListEquity = new List<decimal>();

        private decimal _max = 0;
        private decimal _min = 0;

        #endregion

        #region Properties
        public StrategyType StrategyType { get; set; }
        public decimal Depo 
        {
            get => _depo;
            set => _depo = value;
        } decimal _depo=0;

        /// <summary>
        /// Результирующее ДЕПО
        /// </summary>
        public decimal ResultDepo
        {
            get => _resultdepo;
            set 
            { 
                _resultdepo = value;
                Profit = ResultDepo - Depo;
                PercentProfit = Math.Round(Profit / Depo * 100,2);
                ListEquity.Add(ResultDepo);
                CalcDrawDown();
            }
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
            set { _maxdrawdown = value; CalcPercentDrawDown(); }
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

        public List<decimal> GetListOfEquity()
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

                if(MaxDrawDown < _max - _min) MaxDrawDown = _max - _min;
            }
        }

        private void CalcPercentDrawDown()
        {
            decimal percent = MaxDrawDown / ResultDepo * 100;
            if (percent > PercentDrawDown) PercentDrawDown = Math.Round(percent, 2);
        }
        #endregion
    }
}
