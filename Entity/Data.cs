using Capital.Enums;
using Capital.Entity;
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

        #region Properties ======================================
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
                Profit = ResultDepo - Depo; // расчет Профита
                PercentProfit = Profit * 100 / Depo;// расчет профита в прцентах
                ListEquity.Add(ResultDepo);// добавляем в Лист каждый раз раз новое значение при новой сделки
                CalcDrawDown();// запускаем вычисление макс просадки
            }

        }
        decimal _resultDepo;

        public decimal Profit { get; set; }

        ///<summary>
        ///Относительный профит в процентах
        /// </summary>

        public decimal PercentProfit { get; set; }
        ///<summary>
        ///Максимальная абсолютная просадка в деньгах
        /// </summary>
        public decimal MaxDrawDown
        {
            get => _maxDrawDown;
            set
            {
                _maxDrawDown = value;
                CalcPercentDrawDown();// вызываем расчет относительной просадки, когда будем обновлять максимальную абсолютную просадку
            }
        }
        decimal _maxDrawDown;

        ///<summary>
        ///Максимальная относительная просадка в процентах
        /// </summary>

        public decimal PercentDrawDown { get; set; }
        #endregion

        #region ================== Fields ==============
        // список, фиксирующий точки(значения), в том числе и  _мах и _мин, (хранит значения действующего депо для каждой сделки)
        List<decimal> ListEquity = new List<decimal>();

        decimal _min = 0;
        decimal _max = 0;

        #endregion
        #region ================== Methods =============
        public List<decimal> GetListEquity()
        {
            return ListEquity;
        }

        private void CalcDrawDown()//метод для вычисления просадки
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
        private void CalcPercentDrawDown() //расчет относительной просадки
        {
            decimal percent = MaxDrawDown * 100 / ResultDepo;// расчет процента просадки
            if (percent > PercentDrawDown) PercentDrawDown = Math.Round(percent, 2);

        }


        #endregion
    }




}
