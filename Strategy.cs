using Capital.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Capital.Strategy_FIX;

namespace Capital
{
    //========================================================================
    public abstract class Strategy
    {
        #region Properties ===================================
        public StrategyType StrategyType { get; set; }
        public decimal Depot { get; set; }
        public decimal ResultDepot { get; set; }
        /// <summary>
        /// Профит, в деньгах
        /// </summary>
        public decimal Profit { get; set; }
        /// <summary>
        /// Профит, %
        /// </summary>
        public decimal PercentProfit { get; set; }
        /// <summary>
        /// Максимальная просадка, в деньгах
        /// </summary>
        public decimal MaxDrawDown { get; set; }
        /// <summary>
        /// Максимальная просадка, %
        /// </summary>
        public decimal PercentDrawDown { get; set; }
        /// <summary>
        /// Процент прибыльных сделок
        /// </summary>
        public decimal GoodDealPercent { get; set; }
        #endregion

        #region Fields ===================================
        protected Params _p;
        #endregion

        public Strategy(Params p)
        {
            _p = p;

            Depot = _p.Depot;
            ResultDepot = _p.Depot;
        }

        /// <summary>
        /// Фабрика классов стратегий
        /// </summary>
        /// <param name="p">Параметры</param>
        /// <param name="type">Тип стратегии</param>
        /// <returns></returns>
        public static Strategy CreateStrategy(Params p, StrategyType type)
        {
            switch (type)
            {
                case StrategyType.FIX:
                    return new Strategy_FIX(p);

                case StrategyType.CAPITALIZATION:
                    return new Strategy_CAPITALIZATION(p);

                case StrategyType.PROGRESS:
                    return new Strategy_PROGRESS(p);

                case StrategyType.DOWNGRADE:
                    return new Strategy_DOWNGRADE(p);

                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Calculate result for current strategy (to be implemented in derived classes)
        /// </summary>
        public void Calculate(List<bool> deals)
        {
            if (deals.Count != 0)
                GoodDealPercent = Math.Round(100M * deals.Count(bGoodDeal => { Deal(bGoodDeal); return bGoodDeal; }) / deals.Count, 2);
        }

        public int CalculateWorkingLot(decimal depot, decimal go, decimal percent)
        {
            decimal WorkingLot = depot / go * (percent > 100 ? 1 : percent / 100);
            return (int)WorkingLot;
        }

        public abstract void Deal(bool bGoodDeal);
    }
    //========================================================================
    public class Strategy_FIX : Strategy
    {
        public Strategy_FIX(Params p) : base(p)
        {
            StrategyType = StrategyType.FIX;
        }

        public override void Deal(bool bGoodDeal)
        {
            if (bGoodDeal)
            {
                ResultDepot += (_p.Take - _p.Comiss) * _p.StartLot;
            }
            else
            {
                ResultDepot -= (_p.Stop + _p.Comiss) * _p.StartLot;
            }
        }
    }
    //========================================================================
    public class Strategy_CAPITALIZATION : Strategy
    {
        private int _workingLot;
        public Strategy_CAPITALIZATION(Params p) : base(p)
        {
            StrategyType = StrategyType.CAPITALIZATION;
            _workingLot = _p.StartLot;
        }

        public override void Deal(bool bGoodDeal)
        {
            if (bGoodDeal)
            {
                ResultDepot += (_p.Take - _p.Comiss) * _workingLot;

                int proposedWorkingLot = (int)(ResultDepot / Depot) * _p.StartLot;
                if (proposedWorkingLot > _workingLot)
                    _workingLot = proposedWorkingLot;
            }
            else
            {
                ResultDepot -= (_p.Stop + _p.Comiss) * _workingLot;

                int maxLot = (int)(ResultDepot / _p.Go);
                if (maxLot < _workingLot)
                    _workingLot = maxLot;
            }
        }
    }
    //========================================================================
    public class Strategy_PROGRESS : Strategy
    {
        private int _workingLot;

        public Strategy_PROGRESS(Params p) : base(p)
        {
            StrategyType = StrategyType.PROGRESS;
            _workingLot = CalculateWorkingLot(Depot, _p.Go, _p.MinStartPercent);
        }

        public override void Deal(bool bGoodDeal)
        {
            if (bGoodDeal)
            {
                ResultDepot += (_p.Take - _p.Comiss) * _workingLot;
                _workingLot = CalculateWorkingLot(ResultDepot, _p.Go, _p.MinStartPercent * _p.Take / _p.Stop);
            }
            else
            {
                ResultDepot -= (_p.Stop + _p.Comiss) * _workingLot;
                _workingLot = CalculateWorkingLot(ResultDepot, _p.Go, _p.MinStartPercent);
            }
        }
    }
    //========================================================================
    public class Strategy_DOWNGRADE : Strategy
    {
        private int _workingLot;

        public Strategy_DOWNGRADE(Params p) : base(p)
        {
            StrategyType = StrategyType.DOWNGRADE;
            _workingLot = _p.StartLot;
        }

        public override void Deal(bool bGoodDeal)
        {
            if (bGoodDeal)
            {
                ResultDepot += (_p.Take - _p.Comiss) * _workingLot;
                _workingLot = _p.StartLot;
            }
            else
            {
                ResultDepot -= (_p.Stop + _p.Comiss) * _workingLot;
                _workingLot = Math.Max(_workingLot / 2, 1);
            }
        }
    }
}