using DSP3.Model;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace DSP3.ViewModel
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private int _signalsCount;
        private int _harmonicsCount;
        private double _amplitude;
        private double _frequency;
        private double _phase;
        private IEnumerable<Vector> _signals;
        private IEnumerable<Vector> _amplitudeSpectrums;
        private IEnumerable<Vector> _phaseSpectrums;
        private IEnumerable<Vector> _restoredSignals;
        private IEnumerable<Vector> _restoredNonPhasedSignals;

        public MainWindowViewModel()
        {
            _signalsCount = 512;
            _harmonicsCount = 1;
            _amplitude = 5;
            _frequency = 5;
            _phase = 0;
            PropertyChanged += OnPropertyChanged;
            Update();
        }

        public int SignalsCount { get => _signalsCount; set => SetProperty(ref _signalsCount, value, nameof(SignalsCount)); }

        public int HarmonicsCount { get => _harmonicsCount; set => SetProperty(ref _harmonicsCount, value, nameof(HarmonicsCount)); }

        public double Amplitude { get => _amplitude; set => SetProperty(ref _amplitude, value, nameof(Amplitude)); }

        public double Frequency { get => _frequency; set => SetProperty(ref _frequency, value, nameof(Frequency)); }

        public double Phase { get => _phase; set => SetProperty(ref _phase, value, nameof(Phase)); }

        public IEnumerable<Vector> Signals { get => _signals; set => SetProperty(ref _signals, value, nameof(Signals)); }

        public IEnumerable<Vector> AmplitudeSpectrums { get => _amplitudeSpectrums; set => SetProperty(ref _amplitudeSpectrums, value, nameof(AmplitudeSpectrums)); }

        public IEnumerable<Vector> PhaseSpectrums { get => _phaseSpectrums; set => SetProperty(ref _phaseSpectrums, value, nameof(PhaseSpectrums)); }

        public IEnumerable<Vector> RestoredSignals { get => _restoredSignals; set => SetProperty(ref _restoredSignals, value, nameof(RestoredSignals)); }

        public IEnumerable<Vector> RestoredNonPhasedSignals { get => _restoredNonPhasedSignals; set => SetProperty(ref _restoredNonPhasedSignals, value, nameof(RestoredNonPhasedSignals)); }

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T field, T newValue, string propertyName)
        {
            if (Equals(field, newValue))
            {
                return false;
            }
            field = newValue;
            NotifyPropertyChanged(propertyName);
            return true;
        }

        protected void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(SignalsCount):
                case nameof(Amplitude):
                case nameof(Frequency):
                case nameof(Phase):
                    Update();
                    break;
            }
        }

        private void Update()
        {
            var signals = Signal.CalcHarmonicSignals(SignalsCount, Amplitude, Frequency, Phase).ToList();
            var sineSpectrums = Signal.CalcSineSpectrums(HarmonicsCount, signals).ToList();
            var cosineSpectrums = Signal.CalcCosineSpectrums(HarmonicsCount, signals).ToList();
            var amplitudeSpectrums = Signal.CalcAmplitudeSpectrums(sineSpectrums, cosineSpectrums).ToList();
            var phaseSpectrums = Signal.CalcPhaseSpectrums(sineSpectrums, cosineSpectrums).ToList();
            var restoredSignals = Signal.RestoreSignals(SignalsCount, amplitudeSpectrums.Zip(phaseSpectrums, (amplitude, phase) => (amplitude, phase))).ToList();
            var restoredNonPhasedSignals = Signal.RestoreSignals(SignalsCount, amplitudeSpectrums).ToList();

            Signals = signals.AsPoints();
            AmplitudeSpectrums = amplitudeSpectrums.AsPoints();
            PhaseSpectrums = phaseSpectrums.AsPoints();
            RestoredSignals = restoredSignals.AsPoints();
            RestoredNonPhasedSignals = restoredNonPhasedSignals.AsPoints();
        }
    }
}