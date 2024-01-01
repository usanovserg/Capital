using Capital.Enams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capital.Entity
{
    public class Data // класс с данными
    {
        public Data(decimal depoStart, StrategyType strategyType)
        {
            StrategyType = strategyType;

            Depo = depoStart;
        }

        #region====================Properties======================

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

        // <summary>
        /// Результат эквити (депо)
        /// </summary> 
        public decimal ResultDepo 
        {
            get => _resultDepo; 

            set
            {
                _resultDepo = value; 

                Profit = ResultDepo - Depo; 

                PercentProfit = Profit * 100 / Depo; 

                ListEquity.Add(ResultDepo); //  В ListEquity добавляем новое значение депо

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
        decimal _maxDrawDown; 

        /// <summary>
        /// Относительный профит в процентах
        /// </summary> 
        public decimal PercentDrawDown { get; set; } 

        #endregion

        #region===============================Fields=======================================

        List<decimal> ListEquity = new List<decimal>(); //   лист для фиксиции нового значение депо

        private decimal _min = 0;

        private decimal _max = 0;



        #endregion

        #region===============================Methods=======================================

        public List<decimal> GetListEquity() 
        {
            return ListEquity; // возвращаем ListEquity через этот метод, так как поле List<decimal> ListEquity приватное
        }
        private void CalcDrawDown()
        {
            if (_max < ResultDepo) // если _max меньше ResultDepo
            {
                _max = ResultDepo; // то в _max присваиваем ResultDepo
                _min = ResultDepo; // минимум иакже обновляем
            }

            if (_min > ResultDepo) // если _min стал больше нового ResultDepo
            {
                _min = ResultDepo; // в минимум присваиваем ResultDepo

                if (MaxDrawDown < _max - _min)
                {
                    MaxDrawDown = _max - _min;
                }
            }
        }

        private void CalcPercentDrawDown() // метод для расчета относительной просадки
        {
            decimal percent = MaxDrawDown * 100 / ResultDepo;

            if (percent > PercentDrawDown) PercentDrawDown = Math.Round(percent, 2);
        }

        #endregion
    }
}
