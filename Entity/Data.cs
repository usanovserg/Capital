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

        #region Properties =====================

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

        public decimal ResultDepo
        {
            get => _resultDepo;

            set
            {
                _resultDepo = value;

                Profit = ResultDepo - Depo;             // вычисляем результат в руб

                PercentProfit = Profit * 100 / Depo;    // вычисляем результат в %

                ListEquity.Add(ResultDepo);             // поместили результат в список

                CalcDrawDown();                         // вызов метода, вычисляющего максимальную просадку
            }
        }
        decimal _resultDepo;

        public decimal Profit { get; set; }

        public decimal PercentProfit { get; set; }

        public decimal MaxDrawDown
        {
            get => _maxDrawDown;

            set
            {
                _maxDrawDown = value;

                CalcPercentDrawDown();  // при обновлении макс просадки в руб проверяем макс просадку в %
            }
        }
        decimal _maxDrawDown;

        public decimal PercentDrawDown { get; set; }

        #endregion

        #region Fields =====================

        List<decimal> ListEquity = new List<decimal>(); // список для вычисления относительной просадки

        private decimal _max = 0;       // в этой переменной будем запоминать максимум капитала

        private decimal _min = 0;       // в этой переменной будем запоминать минимум капитала

        #endregion

        #region Methods =====================

        public List<decimal> GetListEquity()    // метод обращается к приватному полю ListEquity
        {
            return ListEquity;
        }

        private void CalcDrawDown()     // метод вычисляет максимальную просадку в руб
        {
            if (_max < ResultDepo)
            {
                _max = ResultDepo;      // запомнили новый максимум
                _min = ResultDepo;      // т.к. новый максимум, то начинаем отслеживать минимум
            }

            if (_min > ResultDepo)
            {
                _min = ResultDepo;      // запомнили новый минимум

                if (MaxDrawDown < _max - _min)
                {
                    MaxDrawDown = _max - _min;  // запоминаем новую максимальную просадку
                }
            }
        }

        private void CalcPercentDrawDown()  // метод вычисляет максимальную просадку в %
        {
            decimal percent = MaxDrawDown * 100 / ResultDepo;   // текущая просадка в %

            if (percent > PercentDrawDown) PercentDrawDown = Math.Round(percent, 2);
        }

        #endregion
    }
}
