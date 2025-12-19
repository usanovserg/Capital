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
        /// После сделки. Рассчитываем конечное депо, профит, процент профита и эквити.
        /// </summary>
        public decimal ResultDepo 
        {
            get => _resultDepo ;

            set
            {
                _resultDepo = value;

                Profit = value; //Можно так
                Profit = _resultDepo; //И так
                Profit = ResultDepo - Depo;

                PercentProfit = Profit * 100 / Depo;

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
        /// Максимальная абсолютная просадка в деньгах
        /// </summary>
        public decimal MaxDrawDown
        {
            get => _maxDrawDown; 

            set
            {
                _maxDrawDown = value;

                CalcPercentDrawDown();
            }
        } 
        private decimal _maxDrawDown;

        /// <summary>
        /// Максимальная просадка в процентах
        /// </summary>
       public decimal PercentDrawDown { get; set; }

        #endregion
        #region Fields #####################################################

       List<Decimal> ListEquity = new List<decimal>();

        private decimal _max = 0;

        private decimal _min =0;
        #endregion

        #region Methods #####################################################
        
        public List<Decimal> GetListEquity()  //Получаем приватное поле.
        {
            return ListEquity;

        }
        private void CalcDrawDown() //Рассчет максимальной просадки.
        {
            if (_max < ResultDepo)
            {
                _max = ResultDepo;
                _min = ResultDepo;
            }

            if(_min > ResultDepo)
            {
                _min = ResultDepo;

                if (MaxDrawDown < _max - _min)
                {
                    MaxDrawDown = _max - _min;
                } 
            }
        }

        private void CalcPercentDrawDown() // Рассчет ОТНОСИТЕЛЬНОЙ просадки
        {
            decimal percent = MaxDrawDown * 100 / ResultDepo;

            if (percent > PercentDrawDown) PercentDrawDown = Math.Round(percent, 2);
                
        }
        #endregion

    }
}
