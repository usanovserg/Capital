using Capital.Enums;

using System;
using System.Collections.Generic;

namespace Capital.Entity
{
    public class Data
    {
        public Data(decimal depoStart, StrategyType strategyType)
        {
            StrategyType = strategyType;

            Depo = depoStart;
        }

        #region Propertirs =================================

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
        /// Результат эквити (депо)
        /// </summary>
        public decimal ResultDepo
        {
            get => _resultDepo;

            set
            {
                _resultDepo = value;

                Profit = ResultDepo - Depo;
                //Profit = _resultDepo - _depo; // так тоже верно
                //Profit = _resultDepo - value; // так тоже верно

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
        public decimal MaxDrawDoun
        {
            get => _maxDrawDoun;

            set
            {
                _maxDrawDoun = value;

                CalcPercentDown();
            }
        }
        decimal _maxDrawDoun;

        /// <summary>
        /// Максимальная относительная просадка в процентах
        /// </summary>
        public decimal PercentDrawDown { get; set; }

        #endregion

        #region Fields =========================================

        // Результирующее депо после совершения каждой сделки - Кривая доходности
        List<decimal> ListEquity = new();
        // Запоминаем максимум на кривой доходности
        private decimal _max = 0;
        // Запоминаем минимум на кривой доходности
        private decimal _min = 0;

        #endregion

        #region Methods ==========================================

        public List<decimal> GetListEquity()
        {
            return ListEquity;
        }

        private void CalcDrawDown()
        {
            // Обновился максимум
            if (_max < ResultDepo)
            {
                _max = ResultDepo;
                _min = ResultDepo;
            }

            // Обновился минимум
            if (_min > ResultDepo)
            {
                _min = ResultDepo;

                if (MaxDrawDoun < _max - _min)
                {
                    MaxDrawDoun = _max - _min;
                }
            }
        }

        // Расчёт просадок в процентах
        private void CalcPercentDown()
        {
            if (ResultDepo == 0) ResultDepo = 1;

            decimal percent = MaxDrawDoun * 100 / ResultDepo;

            if (percent > PercentDrawDown) PercentDrawDown = Math.Round(percent, 2);
        }

        #endregion
    }
}
