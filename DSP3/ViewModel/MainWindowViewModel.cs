using DSP3.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace DSP3.ViewModel
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private SignalType _signalType;
        private int _signalsCount;
        private int _harmonicsCount;
        private double _amplitude;
        private double _frequency;
        private double _phase;
        private int _polyharmonicsCount;
        private IEnumerable<Vector> _signals;
        private IEnumerable<Vector> _amplitudeSpectrums;
        private IEnumerable<Vector> _phaseSpectrums;
        private IEnumerable<Vector> _restoredSignals;
        private IEnumerable<Vector> _restoredNonPhasedSignals;

        public MainWindowViewModel()
        {
            _signalType = SignalType.Harmonic;
            _signalsCount = 512;
            _harmonicsCount = 8;
            _polyharmonicsCount = 32;
            _amplitude = 8;
            _frequency = 4;
            _phase = 60;
            PropertyChanged += OnPropertyChanged;
            Update();
        }

        public int SignalsCount { get => _signalsCount; set => SetProperty(ref _signalsCount, value, nameof(SignalsCount)); }

        public SignalType SignalType { get => _signalType; set => SetProperty(ref _signalType, value, nameof(SignalType)); }

        public int HarmonicsCount { get => _harmonicsCount; set => SetProperty(ref _harmonicsCount, value, nameof(HarmonicsCount)); }

        public double Amplitude { get => _amplitude; set => SetProperty(ref _amplitude, value, nameof(Amplitude)); }

        public double Frequency { get => _frequency; set => SetProperty(ref _frequency, value, nameof(Frequency)); }

        public double Phase { get => _phase; set => SetProperty(ref _phase, value, nameof(Phase)); }

        public int PolyharmonicsCount { get => _polyharmonicsCount; set => SetProperty(ref _polyharmonicsCount, value, nameof(PolyharmonicsCount)); }

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
                case nameof(SignalType):
                case nameof(HarmonicsCount):
                case nameof(Amplitude):
                case nameof(Frequency):
                case nameof(Phase):
                case nameof(PolyharmonicsCount):
                    Update();
                    break;
            }
        }

        private void Update()
        {
            var signals = CreateSignals().ToList();
            var sineSpectrumsTask = Task.Run(() => Signal.CalcSineSpectrums(HarmonicsCount, signals).ToList());
            var cosineSpectrumsTask = Task.Run(() => Signal.CalcCosineSpectrums(HarmonicsCount, signals).ToList());
            var sineSpectrums = sineSpectrumsTask.GetAwaiter().GetResult();
            var cosineSpectrums = cosineSpectrumsTask.GetAwaiter().GetResult();

            var amplitudeSpectrumsTask = Task.Run(() => Signal.CalcAmplitudeSpectrums(sineSpectrums, cosineSpectrums).ToList());
            var phaseSpectrumsTask = Task.Run(() => Signal.CalcPhaseSpectrums(sineSpectrums, cosineSpectrums).ToList());

            var amplitudeSpectrums = amplitudeSpectrumsTask.GetAwaiter().GetResult();
            var restoredNonPhasedSignalsTask = Task.Run(() => Signal.RestoreNonPhasedSignals(SignalsCount, amplitudeSpectrums).ToList());

            var phaseSpectrums = phaseSpectrumsTask.GetAwaiter().GetResult();
            var restoredSignalsTask = Task.Run(() => Signal.RestoreSignals(SignalsCount, amplitudeSpectrums, phaseSpectrums).ToList());

            var restoredNonPhasedSignals = restoredNonPhasedSignalsTask.GetAwaiter().GetResult();
            var restoredSignals = restoredSignalsTask.GetAwaiter().GetResult();

            Signals = signals.AsPoints();
            AmplitudeSpectrums = amplitudeSpectrums.AsPoints();
            PhaseSpectrums = phaseSpectrums.AsPoints();
            RestoredSignals = restoredSignals.AsPoints();
            RestoredNonPhasedSignals = restoredNonPhasedSignals.AsPoints();
        }

        private IEnumerable<double> CreateSignals()
        {
            return SignalType == SignalType.Harmonic
                ? Signal.CalcHarmonicSignals(SignalsCount, Amplitude, Frequency, Signal.ToRadians(Phase))
                : Signal.CalcPolyharmonicSignals(SignalsCount, CreateRandomPolyharmonic());
        }

        private IEnumerable<(double Amplitude, double Frequency, double Phase)> CreateRandomPolyharmonic()
        {
            var amplitudes = new double[] { 2, 3, 5, 9, 10, 12, 15 };
            var phases = new double[] { Math.PI / 6, Math.PI / 4, Math.PI / 3, Math.PI / 2, 3 * Math.PI / 4, Math.PI };
            var random = new Random();
            double GetRandomElement(double[] array) => array[random.Next(array.Length)];
            return Enumerable.Range(0, PolyharmonicsCount)
                .Select(i => (GetRandomElement(amplitudes), 1.0d, GetRandomElement(phases)));
        }
    }
}