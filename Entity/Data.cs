using MyCapital.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MyCapital.Entity
{
    public class Data

    {
        public Data(decimal depoStart, StrategyType strategyType)
        {
            StrategyType = strategyType;

            Depo = depoStart;
        }
                     
        #region Properties ==========================================

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
        /// результат эквити
        /// </summary>
        public decimal ResultDepo
        {
            get => _resultDepo;

            set
            {
                _resultDepo = value;

                Profit = ResultDepo - Depo;

                PercentProfit = Math.Round(Profit * 100 / Depo, 2);
                
                ListEquity.Add(ResultDepo);

                CalcDrawDown();
            }
        }

        decimal _resultDepo;
        

        public decimal Profit
        {
            get => _profit;

            set => _profit = value;

        }

        decimal _profit;

        /// <summary>
        /// Относительный профит в процентах 
        /// </summary>
        public decimal PercentProfit { get; set; }
        /// <summary>
        /// Максимальная абсолютная просадка в деньгах
        /// </summary>
        public decimal MaxDrawDown
        {
            get => _maxDrowDown;
            set
            {
                _maxDrowDown = value;
                CalcPercentDrawDown();
            }
        }            
        decimal _maxDrowDown;

        /// <summary>
        /// Максимальная относительная просадка в процентах
        /// </summary>
        public decimal PercentDrawDown 
        {
            get; set;
        }
        
        #endregion

        #region Fields ==============================================
        
        List<decimal> ListEquity = new List<decimal>();

        private decimal _max = 0;
        private decimal _min = 0;


        #endregion

        #region Methods =============================================

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
            decimal percent = MaxDrawDown * 100 / ResultDepo;
            if (percent > PercentDrawDown)
            {
                PercentDrawDown = Math.Round(percent, 2);
            }
        }   
            
            
            
            
            #endregion
        
    }
}
