using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyCapital.Enums;

namespace MyCapital.Entity
{
    internal class Data
    {
        public Data(decimal DepoStart, StrategyType strategyType)
        {
            StrategyType = strategyType;

            Depo = DepoStart;
        }

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

        private decimal _depo;

        
        /// <summary>
        /// Результат Эквити (Депо)
        /// </summary>
        public decimal ResultDepo
        {
            get => _resultDepo;
            set
            {
                _resultDepo = value;
                Profit = ResultDepo - Depo;
                PercentProfit = Profit * 100 / Depo;
                ListEquity.Add(ResultDepo);
                CalcDrawDown();
            }
        }

        public decimal Profit { get; set; }

        private decimal _resultDepo { get; set; }

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
                CalcPercentDrawDawn();
            }
            
        }

        public decimal _maxDrawDown;
        /// <summary>
        /// Максимальная относительная просадка в процентах
        /// </summary>
        public decimal PercentDrawDown { get; set; }
        
        private List<decimal> ListEquity = [];

        public List<decimal> GetListEquity()
        {
            return ListEquity;
        }

        private decimal _max = 0;

        private decimal _min = 0;

        private void CalcDrawDown()
        {
            if (ResultDepo > _max)
            {
                _max = ResultDepo;
                _min = ResultDepo;
            }

            if (ResultDepo < _min)
            {
                _min = ResultDepo;

                if (MaxDrawDown < _max - _min)
                {
                    MaxDrawDown = _max - _min;
                }
            }
        }

        private void CalcPercentDrawDawn()
        {
            if (ResultDepo == 0) return;
            decimal percent = MaxDrawDown * 100 /ResultDepo;
            if (percent > PercentDrawDown) PercentDrawDown = Math.Round(percent, 2);
        }
    }
}
