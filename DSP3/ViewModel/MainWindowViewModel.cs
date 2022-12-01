using DSP3.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace DSP3.ViewModel
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private SignalType _signalType;
        private FourierTransformationType _fourierTransformationType;
        private int _signalsCount;
        private int _harmonicsCount;
        private double _amplitude;
        private double _frequency;
        private double _phase;
        private int _polyharmonicsCount;
        private int _filterFactor;
        private IEnumerable<Vector> _signals;
        private IEnumerable<Vector> _amplitudeSpectrums;
        private IEnumerable<Vector> _phaseSpectrums;
        private IEnumerable<Vector> _restoredSignals;
        private IEnumerable<Vector> _restoredNonPhasedSignals;
        private IEnumerable<Vector> _restoredLow;
        private IEnumerable<Vector> _restoredHigh;
        private TimeSpan _elapsedTime;

        public MainWindowViewModel()
        {
            _signalType = SignalType.Harmonic;
            _signalsCount = 512;
            _harmonicsCount = 16;
            _polyharmonicsCount = 32;
            _amplitude = 8;
            _frequency = 4;
            _phase = 60;
            _filterFactor = 16;
            PropertyChanged += OnPropertyChanged;
            Update();
        }

        public int SignalsCount { get => _signalsCount; set => SetProperty(ref _signalsCount, value, nameof(SignalsCount)); }

        public SignalType SignalType { get => _signalType; set => SetProperty(ref _signalType, value, nameof(SignalType)); }

        public FourierTransformationType FourierTransformationType { get => _fourierTransformationType; set => SetProperty(ref _fourierTransformationType, value, nameof(FourierTransformationType)); }

        public int HarmonicsCount { get => _harmonicsCount; set => SetProperty(ref _harmonicsCount, value, nameof(HarmonicsCount)); }

        public double Amplitude { get => _amplitude; set => SetProperty(ref _amplitude, value, nameof(Amplitude)); }

        public double Frequency { get => _frequency; set => SetProperty(ref _frequency, value, nameof(Frequency)); }

        public double Phase { get => _phase; set => SetProperty(ref _phase, value, nameof(Phase)); }

        public int PolyharmonicsCount { get => _polyharmonicsCount; set => SetProperty(ref _polyharmonicsCount, value, nameof(PolyharmonicsCount)); }

        public int FilterFactor { get => _filterFactor; set => SetProperty(ref _filterFactor, value, nameof(FilterFactor)); }

        public TimeSpan ElapsedTime { get => _elapsedTime; set => SetProperty(ref _elapsedTime, value, nameof(ElapsedTime)); }

        public IEnumerable<Vector> Signals { get => _signals; set => SetProperty(ref _signals, value, nameof(Signals)); }

        public IEnumerable<Vector> AmplitudeSpectrums { get => _amplitudeSpectrums; set => SetProperty(ref _amplitudeSpectrums, value, nameof(AmplitudeSpectrums)); }

        public IEnumerable<Vector> PhaseSpectrums { get => _phaseSpectrums; set => SetProperty(ref _phaseSpectrums, value, nameof(PhaseSpectrums)); }

        public IEnumerable<Vector> RestoredSignals { get => _restoredSignals; set => SetProperty(ref _restoredSignals, value, nameof(RestoredSignals)); }

        public IEnumerable<Vector> RestoredNonPhasedSignals { get => _restoredNonPhasedSignals; set => SetProperty(ref _restoredNonPhasedSignals, value, nameof(RestoredNonPhasedSignals)); }

        public IEnumerable<Vector> RestoredLow{ get => _restoredLow; set => SetProperty(ref _restoredLow, value, nameof(RestoredLow)); }

        public IEnumerable<Vector> RestoredHigh{ get => _restoredHigh; set => SetProperty(ref _restoredHigh, value, nameof(RestoredHigh)); }


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
                case nameof(FourierTransformationType):
                case nameof(HarmonicsCount):
                case nameof(Amplitude):
                case nameof(Frequency):
                case nameof(Phase):
                case nameof(PolyharmonicsCount):
                case nameof(FilterFactor):
                    Update();
                    break;
            }
        }

        private void Update()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var signals =
                SignalType == SignalType.Harmonic
                ? Signal.CalcHarmonicSignals(SignalsCount, Amplitude, Frequency, Signal.ToRadians(Phase)).ToList()
                : Signal.CalcPolyharmonicSignals(SignalsCount, CreateRandomPolyharmonic()).ToList();

            var (amplitudeSpectrums, phaseSpectrums) =
                FourierTransformationType == FourierTransformationType.Simple ?
                CreateFourierTransformation(signals) :
                Signal.CreateFastFourierTransformation(signals);;

            var restoredSignalsTask = Task.Run(() => Signal.RestoreSignals(SignalsCount, amplitudeSpectrums, phaseSpectrums).ToList());
            var restoredNonPhasedSignalsTask = Task.Run(() => Signal.RestoreNonPhasedSignals(SignalsCount, amplitudeSpectrums).ToList());
            var restoredLowTask = Task.Run(() => Signal.RestoreSignals(SignalsCount, Filter(amplitudeSpectrums, phaseSpectrums, x => x < FilterFactor)).ToList());
            var restoredHighTask = Task.Run(() => Signal.RestoreSignals(SignalsCount, Filter(amplitudeSpectrums, phaseSpectrums, x => x > FilterFactor)).ToList());

            var restoredNonPhasedSignals = restoredNonPhasedSignalsTask.GetAwaiter().GetResult();
            var restoredSignals = restoredSignalsTask.GetAwaiter().GetResult();
            var restoredLow = restoredLowTask.GetAwaiter().GetResult();
            var restoredHigh = restoredHighTask.GetAwaiter().GetResult();

            stopwatch.Stop();

            ElapsedTime = stopwatch.Elapsed;
            Signals = signals.AsPoints();
            RestoredSignals = restoredSignals.AsPoints();
            RestoredNonPhasedSignals = restoredNonPhasedSignals.AsPoints();
            RestoredLow = restoredLow.AsPoints();
            RestoredHigh = restoredHigh.AsPoints(); 
            AmplitudeSpectrums = amplitudeSpectrums.AsPoints();
            PhaseSpectrums = phaseSpectrums.AsPoints();
        }

        private IEnumerable<(double Amplitude, double Phase)> Filter(IList<double> amplitudeSpectrums, IList<double> phaseSpectrums, Func<int, bool> predicate)
        {
            var zero = (0.0d, 0.0d);
            var i = 0;
            return amplitudeSpectrums.Zip(phaseSpectrums, (a, p) => predicate(i++) ? (a, p) : zero);
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

        private (IList<double>, IList<double>) CreateFourierTransformation(IList<double> signals)
        {
            var sineSpectrumsTask = Task.Run(() => Signal.CalcSineSpectrums(HarmonicsCount, signals).ToList());
            var cosineSpectrumsTask = Task.Run(() => Signal.CalcCosineSpectrums(HarmonicsCount, signals).ToList());
            var sineSpectrums = sineSpectrumsTask.GetAwaiter().GetResult();
            var cosineSpectrums = cosineSpectrumsTask.GetAwaiter().GetResult();

            var amplitudeSpectrumsTask = Task.Run(() => Signal.CalcAmplitudeSpectrums(sineSpectrums, cosineSpectrums).ToList());
            var phaseSpectrumsTask = Task.Run(() => Signal.CalcPhaseSpectrums(sineSpectrums, cosineSpectrums).ToList());

            var amplitudeSpectrums = amplitudeSpectrumsTask.GetAwaiter().GetResult();
            var phaseSpectrums = phaseSpectrumsTask.GetAwaiter().GetResult();
            return (amplitudeSpectrums, phaseSpectrums);
        }
    }
}