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
            Depo = depoStart;

            StrategyType = strategyType;
        }


        #region Properties ======================

        public StrategyType StrategyType { get; set; }




        public decimal Profit { get; set; }

        public decimal PercentProfit { get; set; }

        public decimal PercentDrawDown { get; set; }

        public static bool CountButtonClicks = false;


        public decimal Depo
        {
            //get
            //{
            //    return _depo;
            //}

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

                Profit = ResultDepo - Depo;

                PercentProfit = Profit * 100 / Depo;

                listEquity.Add(ResultDepo);

                CalcDrawDown();

                CountButtonClicks = true;



            }
        }
        decimal _resultDepo;


        //public decimal Profit
        //{
        //    get => _profit;

        //    set
        //    {
        //        _profit = value;
        //    }
        //}
        //decimal _profit;

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





        #endregion


        #region Fields =======================

        List<decimal> listEquity = new List<decimal>();

        private decimal _max = 0;

        private decimal _min = 0;



        #endregion


        #region Methods ======================

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
            if (percent > PercentDrawDown) PercentDrawDown = Math.Round(percent, 2);

        }


        public List<decimal> GetListEquity()
        {
            return listEquity;
        }
        #endregion
    }
}
