using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Windows.Documents;

namespace DSP3.Model
{
    public static class Signal
    {
        private static readonly double PI2 = Math.PI * 2;
        private static readonly double ToRadiansValue = Math.PI / 180;

        public static double ToRadians(double angle) => angle * ToRadiansValue;

        public static double ToDegree(double radians) => radians / ToRadiansValue;

        public static double CalcSignal(double amplitude, double frequency, double phase)
        {
            return amplitude * Math.Cos(PI2 * frequency + phase);
        }

        public static IEnumerable<double> CalcHarmonicSignals(int n, double amplitude, double frequency, double phase)
        {
            return Enumerable.Range(0, n)
                .Select(i => CalcSignal(amplitude, frequency * i / n, phase));
        }

        public static IEnumerable<double> CalcPolyharmonicSignals(int n, IEnumerable<(double Amplitude, double Frequency, double Phase)> harmonics)
        {
            return Enumerable.Range(0, n)
                .Select(i => harmonics
                    .Select(harmonic => CalcSignal(harmonic.Amplitude, harmonic.Frequency * i / n, harmonic.Phase))
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
            return sineSpectrums.Zip(cosineSpectrums, (sin, cos) => Math.Atan2(sin, cos));
        }

        public static IEnumerable<double> RestoreSignals(int n, IEnumerable<(double Amplitude, double Phase)> spectrums)
        {
            return Enumerable.Range(0, n)
                .Select(i => spectrums
                    .SelectWithIndex((j, t) => t.Amplitude * Math.Cos(PI2 * i * j / n - t.Phase))
                    .Sum());
        }

        public static IEnumerable<double> RestoreSignals(int n, IEnumerable<double> amplitudeSpectrums, IEnumerable<double> phaseSpectrums)
        {
            return RestoreSignals(n, amplitudeSpectrums.Zip(phaseSpectrums, (amplitude, phase) => (amplitude, phase)));
        }

        public static IEnumerable<double> RestoreNonPhasedSignals(int n, IEnumerable<double> amplitudeSpectrums)
        {
            return Enumerable.Range(0, n)
                .Select(i => amplitudeSpectrums
                    .SelectWithIndex((j, amplitude) => amplitude * Math.Cos(PI2 * i * j / n))
                    .Sum());
        }

        private static Complex CalcFactor(int m, int n)
        {
            return Complex.Exp(new Complex(0, -2) * new Complex(Math.PI, 0) * new Complex(m, 0) / new Complex(n, 0));
        }

        private static void CreateFastFourierTransformation(Complex[] signals)
        {
            var n = signals.Length;
            if (n < 2)
                return;

            var num = n / 2;
            var evenElements = new Complex[num];
            var oddElements = new Complex[num];
            for (int i = 0; i < num; i++)
            {
                evenElements[i] = signals[i * 2];
                oddElements[i] = signals[i * 2 + 1];
            }
            CreateFastFourierTransformation(evenElements);
            CreateFastFourierTransformation(oddElements);

            for (int m = 0; m < num; m++)
            {
                var e = evenElements[m];
                var o = oddElements[m];

                var f1 = CalcFactor(m, n);
                signals[m] = e + (f1 * o);

                var f2 = CalcFactor(m + num, n);
                signals[m + num] = e + (f2 * o);
            }
        }

        public static (IList<double> Amplitudes, IList<double> Phases) CreateFastFourierTransformation(IEnumerable<double> signals)
        {
            var preparedSignals = signals
                .Select(v => new Complex(v, 0))
                .ToArray();

            CreateFastFourierTransformation(preparedSignals);

            var n = preparedSignals.Length;
            var amplitudeSpectrums = preparedSignals
                .Select(v => v.Magnitude / n)
                .ToList();
            var phases = preparedSignals
                .Select(v => -v.Phase)
                .ToList();

            return (Amplitudes: amplitudeSpectrums, Phases: phases);
        }
    }
}
