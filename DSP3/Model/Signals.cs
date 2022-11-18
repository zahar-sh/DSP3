using System;
using System.Collections.Generic;
using System.Linq;

namespace DSP3.Model
{
    public static class Signal
    {
        private static readonly double PI2 = Math.PI * 2;

        public static double CalcSignal(double amplitude, double frequency, double phase, int i, int n)
        {
            return amplitude * Math.Cos(PI2 * frequency * i / n + phase);
        }

        public static IEnumerable<double> CalcHarmonicSignals(int n, double amplitude, double frequency, double phase)
        {
            return Enumerable.Range(0, n)
                .Select(i => CalcSignal(amplitude, frequency, phase, i, n));
        }

        public static IEnumerable<double> CalcPolyharmonicSignals(int n, IEnumerable<(double Amplitude, double Frequency, double Phase)> harmonics)
        {
            return Enumerable.Range(0, n)
                .Select(i => harmonics
                    .Select(harmonic => CalcSignal(harmonic.Amplitude, harmonic.Frequency, harmonic.Phase, i, n))
                    .Sum());
        }

        public static IEnumerable<double> CalcSineSpectrums(int harmonicsCount, IEnumerable<double> signals)
        {
            var n = signals.Count();
            var num = 2.0d / n;
            return Enumerable.Range(0, harmonicsCount)
                .Select(j => signals
                    .SelectWithIndex((i, v) => v * Math.Sin(PI2 * i * j / n))
                    .Sum())
                .Select(v => v * num);
        }

        public static IEnumerable<double> CalcCosineSpectrums(int harmonicsCount, IEnumerable<double> signals)
        {
            var n = signals.Count();
            var num = 2.0d / n;
            return Enumerable.Range(0, harmonicsCount)
                .Select(j => signals
                    .SelectWithIndex((i, v) => v * Math.Cos(PI2 * i * j / n))
                    .Sum())
                .Select(v => v * num);
        }

        public static IEnumerable<double> CalcAmplitudeSpectrums(IEnumerable<double> sineSpectrums, IEnumerable<double> cosineSpectrums)
        {
            return sineSpectrums.Zip(cosineSpectrums, (sin, cos) => Math.Sqrt(sin * sin + cos * cos));
        }

        public static IEnumerable<double> CalcPhaseSpectrums(IEnumerable<double> sineSpectrums, IEnumerable<double> cosineSpectrums)
        {
            return sineSpectrums.Zip(cosineSpectrums, (sin, cos) => Math.Atan(sin / cos));
        }

        public static IEnumerable<double> RestoreSignals(int n, IEnumerable<(double Amplitude, double Phase)> spectrums)
        {
            return Enumerable.Range(0, n)
                .Select(i => spectrums
                    .SelectWithIndex((j, t) => t.Amplitude * Math.Cos(PI2 * i * j / n - t.Phase))
                    .Sum());
        }

        public static IEnumerable<double> RestoreSignals(int n, IEnumerable<double> amplitudeSpectrums)
        {
            return Enumerable.Range(0, n)
                .Select(i => amplitudeSpectrums
                    .SelectWithIndex((j, amplitude) => amplitude * Math.Cos(PI2 * i * j / n))
                    .Sum());
        }
    }
}
