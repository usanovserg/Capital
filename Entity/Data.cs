using Capital.Enums;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media;
using static System.Windows.Media.Color;
using System.ComponentModel;       
using System.Runtime.CompilerServices;

namespace Capital.Entity
{
    public class Data : INotifyPropertyChanged
    {
        // 🟡 ДОБАВЛЕНО: событие для уведомления UI
        public event PropertyChangedEventHandler PropertyChanged;

        // 🟡 ДОБАВЛЕНО: вспомогательный метод
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public Data(decimal depoStart, StrategyType strategyType)
        {
            
            StrategyType = strategyType;                     

            _depo = depoStart > 0 ? depoStart : 1;
            _resultDepo = _depo;
            Profit = 0;
            PercentProfit = 0;
            ListEquity = new List<decimal> { _depo };
            _max = _depo;
            _min = _depo;
        }

        #region Properties=================================================


        public StrategyType StrategyType { get; set; }

        public OxyColor Color { get; set; }

        public Brush ColorBrush
        {
            get
            {
                return StrategyType switch
                {
                    StrategyType.FIX => Brushes.Red,
                    StrategyType.CAPITALIZATION => Brushes.Green,
                    StrategyType.PROGRESS => Brushes.Blue,
                    StrategyType.DOWNGRADE => Brushes.Orange,
                    _ => Brushes.Gray
                };
            }
        }

        public decimal Depo
        {
            get => _depo;

            set
            {
                if (value == 0) return; // установил защиту и не трогаем ResultDepo здесь
                _depo = value;
                //ResultDepo = value;
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

                PercentProfit = _depo != 0 ? Math.Round(Profit * 100 / _depo, 2) : 0; // защита от деления на ноль

                ListEquity.Add(ResultDepo);

                CalcDrawDown();

                OnPropertyChanged(); // 👁️ UI получит уведомление!
            }
        }
        decimal _resultDepo;

        private decimal _profit;
        public decimal Profit
        {
            get => _profit;
            set
            {
                if (_profit != value)
                {
                    _profit = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Относительный профит в процентах
        /// </summary>
        private decimal _percentProfit;
        public decimal PercentProfit
        {
            get => _percentProfit;
            set
            {
                if (_percentProfit != value)
                {
                    _percentProfit = value;
                    OnPropertyChanged();
                }
            }
        }
        /// <summary>
        /// Максимальная абсолютная просадка в деньгах 
        /// </summary>
        private decimal _maxDrownDown;
        public decimal MaxDrownDown
        {
            get => _maxDrownDown;
            set
            {
                if (_maxDrownDown != value)
                {
                    _maxDrownDown = value;
                    CalcPercentDrawDown();
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Максимальная относительная просадка в процентах
        /// </summary>
        public decimal PercentDrawnDown { get; set; }


        #endregion

        #region Fields ====================================================

        List<decimal> ListEquity = new List<decimal>();

        private decimal _max = 0;

        private decimal _min = 0;

        #endregion

        #region Methods ===================================================

        public List<decimal> GetListEquity()
        {
            return ListEquity;
        }

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

                if (MaxDrownDown < _max - _min)
                {
                    MaxDrownDown = _max - _min;
                }             

            }
        }

        private void CalcPercentDrawDown()
        {
            decimal percent = MaxDrownDown * 100 / ResultDepo; 
            
            if (percent > PercentDrawnDown) PercentDrawnDown = Math.Round(percent, 2);
        }

        #endregion
    }
}
