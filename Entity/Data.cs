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

        public ColorsMy StrategyColor { get; set; }


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



        /// <summary>
        /// Результирующее депо (или Equity)
        /// </summary>
        public decimal ResultDepo
        {
            get { return _resultDepo; }
            set
            {
                _resultDepo = value;                    // Результирующее депо (или Equity)
                Profit = ResultDepo - Depo;             // Абсолютный профит 
                PercentProfit = Profit / Depo * 100;    // Относительный профит в процентах
                ListEquity.Add(ResultDepo);             // в список запишем после каждой сделки новое результирующее депо (сколько сделок, столько значений)
                CalcDrawDown();                         // расчёт просадки DrawDown                       
            }
        }
        private decimal _resultDepo;



        public decimal Profit { get; set; }



        /// <summary>
        /// Относительный профит в процентах
        /// </summary>
        public decimal PercentProfit { get; set; }



        /// <summary>
        /// Максимальная абсолютная просадка в деньгах. И здесь же запустим метод для относительной в процентах. В нём будет значение для относительной
        /// </summary>
        public decimal MaxDrawDown      
        {
            get => _maxDrawDown;
            set
            {
                _maxDrawDown = value;   // это просадка MaxDrawDown, которая считается в методе CalcDrawDown, который запускается в свойстве ResultDepo
                CalcPercentDrawDown();  // это метод для относительной просадки PercentDrawDown. В нём будет получено значение для неё
            }
        }
        decimal _maxDrawDown;



        /// <summary>
        /// Максимальная относительная просадка в процентах
        /// </summary>
        public decimal PercentDrawDown { get; set; }    // это считается в методе CalcPercentDrawDown, который запускается в свойстве MaxDrawDown


        public decimal Color { get; set; }

        #endregion Properties



        #region Fields ========================================================

        private List<decimal> ListEquity = new List<decimal>();     // на каждой сделке записываем значение текущего депо в список. Потом мы по нему можем пробежаться
        private decimal _max = 0;
        private decimal _min = 0;

        #endregion Fields


        #region Methods ========================================================



        // публичный метод, чтобы был доступен приватный List<decimal> ListEquity для вызова (получить список где-то в коде). Что-то типа get
        public List<decimal> GetListEquity()
        {
            return ListEquity;
        }



        // расчёт абсолютной просадки
        private void CalcDrawDown()
        {
            // если обновился max
            if (_max < ResultDepo)
            {
                _max = ResultDepo;
                _min = ResultDepo;
            }

            // если обновился min
            if (_min > ResultDepo)
            {
                _min = ResultDepo;
                if ( MaxDrawDown < (_max - _min) )
                {
                    MaxDrawDown = (_max - _min);
                }                
            }
        }



        // расчёт относительной просадки
        private void CalcPercentDrawDown()
        {
            decimal percent = MaxDrawDown / ResultDepo * 100;
            if (percent > PercentDrawDown)
            {
                PercentDrawDown =  Math.Round(percent, 2);
            }
        }

        #endregion Methods



    }
}
