using System;
using System.Collections.Generic;
using System.Linq;
using Capital.Enums;

namespace Capital
{
	//========================================================================
	public abstract class Strategy
	{
		#region Properties ===================================
		public StrategyType StrategyType { get; set; }
		public decimal Depot { get; set; }
		public decimal ResultDepot { get => _resultDepot; set => SetResultDepot(value);
		}
		private decimal _resultDepot;

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
		protected Params P;

		private decimal _localMaximum;
		private decimal _localMinimum;

		private readonly List<decimal> _results;

		#endregion

		protected Strategy(Params p)
		{
			P = p;

			_results = new List<decimal>(P.CountTrades);

			Depot = P.Depot;
			ResultDepot = Depot;

			_localMaximum = Depot;
			_localMinimum = Depot;
		}

		/// <summary>
		/// Фабрика классов стратегий
		/// </summary>
		/// <param name="p">Параметры</param>
		/// <param name="type">Тип стратегии</param>
		/// <returns></returns>
		public static Strategy CreateStrategy(Params p, StrategyType type)
		{
			return type switch
			{
				StrategyType.Fix => new StrategyFix(p),
				StrategyType.Capitalization => new StrategyCapitalization(p),
				StrategyType.Progress => new StrategyProgress(p),
				StrategyType.Downgrade => new StrategyDowngrade(p),
				StrategyType.Count => throw new NotImplementedException(),
				_ => throw new NotImplementedException()
			};
		}

		private void SetResultDepot(decimal resultDepot)
		{
			_resultDepot = resultDepot;

			Profit = resultDepot - Depot;
			PercentProfit = Math.Round(Profit / Depot * 100M, 2);

			if (resultDepot > _localMaximum)
			{
				_localMaximum = resultDepot;
				_localMinimum = resultDepot;
			}

			if (resultDepot < _localMinimum)
			{
				_localMinimum = resultDepot;

				decimal drawDown = _localMaximum - _localMinimum;
				if (drawDown > MaxDrawDown)
					MaxDrawDown = drawDown;

				decimal percentDrawDown = Math.Round(drawDown / _localMaximum * 100M, 2);
				if (percentDrawDown > PercentDrawDown)
					PercentDrawDown = percentDrawDown;
			}

			_results.Add(resultDepot);
		}

		public List<decimal> GetResults() => _results;

		protected static int CalculateWorkingLot(decimal depot, decimal go, decimal percent)
		{
			decimal workingLot = depot / go * (percent > 100 ? 1 : percent / 100);
			return (int)workingLot;
		}

		/// <summary>
		/// Calculate result for current strategy
		/// </summary>
		public void Calculate(List<bool> deals)
		{
			if (deals.Count != 0)
			{
				GoodDealPercent = Math.Round(100M * deals.Count(bGoodDeal => { Deal(bGoodDeal); return bGoodDeal; }) / deals.Count, 2);
			}
		}
		/// <summary>
		/// Add a deal to strategy (to be implemented in derived classes)
		/// </summary>
		/// <param name="bGoodDeal">bool GoodDeal: true | BadDeal: false</param>
		protected abstract void Deal(bool bGoodDeal);
	}
	//========================================================================
	public class StrategyFix : Strategy
	{
		public StrategyFix(Params p) : base(p)
		{
			StrategyType = StrategyType.Fix;
		}

		protected override void Deal(bool bGoodDeal)
		{
			if (bGoodDeal)
			{
				ResultDepot += (P.Take - P.Commission) * P.StartLot;
			}
			else
			{
				ResultDepot -= (P.Stop + P.Commission) * P.StartLot;
			}
		}
	}
	//========================================================================
	public class StrategyCapitalization : Strategy
	{
		private int _workingLot;
		public StrategyCapitalization(Params p) : base(p)
		{
			StrategyType = StrategyType.Capitalization;
			_workingLot = P.StartLot;
		}

		protected override void Deal(bool bGoodDeal)
		{
			if (bGoodDeal)
			{
				ResultDepot += (P.Take - P.Commission) * _workingLot;

				decimal proposedWorkingLot = (ResultDepot / Depot) * P.StartLot;
				if (proposedWorkingLot > _workingLot)
					_workingLot = (int)proposedWorkingLot;
			}
			else
			{
				ResultDepot -= (P.Stop + P.Commission) * _workingLot;

				int maxLot = (int)(ResultDepot / P.Go);
				if (maxLot < _workingLot)
					_workingLot = maxLot;
			}
		}
	}
	//========================================================================
	public class StrategyProgress : Strategy
	{
		private int _workingLot;

		public StrategyProgress(Params p) : base(p)
		{
			StrategyType = StrategyType.Progress;
			_workingLot = CalculateWorkingLot(Depot, P.Go, P.MinStartPercent);
		}

		protected override void Deal(bool bGoodDeal)
		{
			if (bGoodDeal)
			{
				ResultDepot += (P.Take - P.Commission) * _workingLot;
				_workingLot = CalculateWorkingLot(ResultDepot, P.Go, P.MinStartPercent * P.Take / P.Stop);
			}
			else
			{
				ResultDepot -= (P.Stop + P.Commission) * _workingLot;
				_workingLot = CalculateWorkingLot(ResultDepot, P.Go, P.MinStartPercent);
			}
		}
	}
	//========================================================================
	public class StrategyDowngrade : Strategy
	{
		private int _workingLot;

		public StrategyDowngrade(Params p) : base(p)
		{
			StrategyType = StrategyType.Downgrade;
			_workingLot = P.StartLot;
		}

		protected override void Deal(bool bGoodDeal)
		{
			if (bGoodDeal)
			{
				ResultDepot += (P.Take - P.Commission) * _workingLot;
				_workingLot = P.StartLot;
			}
			else
			{
				ResultDepot -= (P.Stop + P.Commission) * _workingLot;
				_workingLot = Math.Max(_workingLot / 2, 1);
			}
		}
	}
}