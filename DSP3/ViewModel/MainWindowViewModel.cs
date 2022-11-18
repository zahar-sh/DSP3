using DSP3.Model;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
        private IEnumerable<double> _signals;
        private IEnumerable<double> _sineSpectrums;
        private IEnumerable<double> _cosineSpectrums;
        private IEnumerable<double> _amplitudeSpectrums;
        private IEnumerable<double> _phaseSpectrums;
        private IEnumerable<double> _restoredSignals;

        public MainWindowViewModel()
        {
            PropertyChanged += OnPropertyChanged;
        }

        public int SignalsCount { get => _signalsCount; set => SetProperty(ref _signalsCount, value, nameof(SignalsCount)); }

        public int HarmonicsCount { get => _harmonicsCount; set => SetProperty(ref _harmonicsCount, value, nameof(HarmonicsCount)); }

        public double Amplitude { get => _amplitude; set => SetProperty(ref _amplitude, value, nameof(Amplitude)); }

        public double Frequency { get => _frequency; set => SetProperty(ref _frequency, value, nameof(Frequency)); }

        public double Phase { get => _phase; set => SetProperty(ref _phase, value, nameof(Phase)); }

        public IEnumerable<double> SignalValues { get => _signals; set => SetProperty(ref _signals, value, nameof(SignalValues)); }

        public IEnumerable<double> SineSpectrums { get => _sineSpectrums; set => SetProperty(ref _sineSpectrums, value, nameof(SineSpectrums)); }

        public IEnumerable<double> CosineSpectrums { get => _cosineSpectrums; set => SetProperty(ref _cosineSpectrums, value, nameof(CosineSpectrums)); }

        public IEnumerable<double> AmplitudeSpectrums { get => _amplitudeSpectrums; set => SetProperty(ref _amplitudeSpectrums, value, nameof(AmplitudeSpectrums)); }

        public IEnumerable<double> PhaseSpectrums { get => _phaseSpectrums; set => SetProperty(ref _phaseSpectrums, value, nameof(PhaseSpectrums)); }

        public IEnumerable<double> RestoredSignals { get => _restoredSignals; set => SetProperty(ref _restoredSignals, value, nameof(RestoredSignals)); }

        public ICommand RepaintCommand { get; }

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
            SignalValues = Signals.CalcHarmonicSignals(SignalsCount, Amplitude, Frequency, Phase).ToArray();
            SineSpectrums = Signals.CalcSineSpectrums(HarmonicsCount, SignalValues).ToArray();
            CosineSpectrums = Signals.CalcCosineSpectrums(HarmonicsCount, SignalValues).ToArray();
            AmplitudeSpectrums = Signals.CalcAmplitudeSpectrums(SineSpectrums, CosineSpectrums).ToArray();
            PhaseSpectrums = Signals.CalcPhaseSpectrums(SineSpectrums, CosineSpectrums).ToArray();
            RestoredSignals = Signals.RestoreSignals(SignalsCount, AmplitudeSpectrums.Zip(PhaseSpectrums, (amplitude, phase) => (amplitude, phase))).ToArray();
        }
    }
}