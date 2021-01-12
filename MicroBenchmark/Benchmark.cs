/*
 * Licensed under MIT.
 * Copyright © 2021 Pavel Chaimardanov. All rights reserved.
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MicroBenchmark
{
    /// <summary>
    /// Represents a class for the benchmarking.
    /// </summary>
    public static class Benchmark
    {
        /// <summary>
        /// Resolution of the measurement (in s).
        /// </summary>
        private static double resolution = 0;

        /// <summary>
        /// Initializes the benchmark.
        /// </summary>
        static Benchmark()
        {
            resolution = 1.0 / Stopwatch.Frequency; 
        }

        /// <summary>
        /// Runs the benchmarking.
        /// </summary>
        /// <param name="action"> Action that performance will be tested. </param>
        /// <param name="beforeAction">
        /// Action that may run before the action.
        /// If null - not specified.
        /// </param>
        /// <param name="afterAction">
        /// Action that may run after the action.
        /// If null - not specified. </param>
        /// <param name="N"> Number of the total iterations. </param>
        /// <param name="NSkip">
        /// Number of the iterations that should be skipped before run benchmarking.
        /// Recommended value is more than zero - if action will be raised first times
        /// - JIT compilation may involves on result of the benchmarking.
        /// </param>
        /// <param name="filterOutliers">
        /// Indicates whether outliers should be filtered (Tukey's fences outlier detection method will be used).
        /// Recommended value is true - any random events may involves on result of the benchmarking.
        /// Filtering may reduse that influence.
        /// </param>
        /// <param name="k">
        /// k coefficient for Tukey's fences outlier detection method (if outliers exclusion is used).
        /// Default value is 1.5.
        /// </param>
        /// <returns> Result of the benchmarking. </returns>
        /// <exception cref="ArgumentException">
        /// The excection that throws if action is null or N is less than or equals NStart.
        /// </exception>
        public static BenchmarkResult Run(Action action, Action beforeAction = null, Action afterAction = null, int N = 100, int NSkip = 5, bool filterOutliers = true, double k = 1.5)
        {
            // Validation  

            if (action == null)
            {
                throw new ArgumentNullException("Action is null");
            }

            if (N < 0)
            {
                throw new ArgumentException($"N={N} is negative");
            }

            if (NSkip < 0)
            {
                throw new ArgumentException($"NSkip={NSkip} is negative");
            }

            int length = N - NSkip;
            if (length <= 0)
            {
                throw new ArgumentException($"N={N} is less than or equals NSkip={NSkip}");
            }

            // Runs the benchmarking

            beforeAction = beforeAction ?? (() => { });
            afterAction = afterAction ?? (() => { });

            List<double> samples = new List<double>(length);
            double time = 0;

            if (action != null)
            {
                Stopwatch stopwatch = new Stopwatch();
                for (int i = 0; i < length; i++)
                {
                    beforeAction();

                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
                    GC.WaitForPendingFinalizers();
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);

                    if (i >= NSkip)
                    {
                        stopwatch.Restart();
                        action();
                        time = stopwatch.ElapsedTicks;

                        samples.Add(time);
                    }
                    else
                    {
                        action();
                    }

                    afterAction();
                }
            }

            // Sorts the samples
            samples.Sort();

            // Filters the samples
            IList<double> filteredSamples;
            if (filterOutliers)
            {
                filteredSamples = samples.TukeyFences(k);
                length = filteredSamples.Count;
            }
            else
            {
                filteredSamples = samples;
            }

            // Gets an average time
            double mean = 0;
            for (int i = 0; i < length; i++)
            {
                mean += filteredSamples[i];
            }
            mean /= length;

            // Gets a standard deviation
            double stdDev = 0;
            for (int i = 0; i < length; i++)
            {
                stdDev += Math.Pow((filteredSamples[i] - mean), 2);
            }
            stdDev = Math.Sqrt(stdDev / length);

            // Gets the quartiles
            double[] quartiles = filteredSamples.GetQuartiles();

            // Returns the result
            return new BenchmarkResult(
                quartiles[0] * resolution,
                quartiles[1] * resolution,
                quartiles[2] * resolution,
                mean * resolution,
                stdDev * resolution,
                resolution);
        }

        /// <summary>
        /// Excludes the outliers from the input sequence
        /// (Tukey's fences outlier detection method is used).
        /// </summary>
        /// <param name="samples"> List of the input samples (sorted in ascending order). </param>
        /// <param name="k">
        /// k coefficient for Tukey's fences outlier detection method (if outliers exclusion is used).
        /// Default value is 1.5.
        /// </param>
        /// <returns> List of the samples without outliers. </returns>
        private static IList<double> TukeyFences(this IList<double> samples, double k)
        {
            double[] quartiles = samples.GetQuartiles();

            int N = samples.Count;

            double Q1 = quartiles[0];
            double Q3 = quartiles[2];
            double dQ = Q3 - Q1;
            double Q1N = Q1 - k * dQ;
            double Q3N = Q3 + k * dQ;

            List<double> newSamples = new List<double>(N);
            double sample;

            for (int i = 0; i < N; i++)
            {
                sample = samples[i];
                if (sample >= Q1N && sample <= Q3N)
                {
                    newSamples.Add(sample);
                }
            }

            return newSamples;
        }

        /// <summary>
        /// Gets a quartiles from the samples.
        /// </summary>
        /// <param name="samples"> List of the samples (sorted in ascending order). </param>
        /// <returns> Quartiles (Q1,Q2,Q3). </returns>
        private static double[] GetQuartiles(this IList<double> samples)
        {
            int iSize = samples.Count;
            int iMid = iSize / 2;

            double fQ1 = 0;
            double fQ2 = 0;
            double fQ3 = 0;

            if (iSize % 2 == 0)
            {
                //================ EVEN NUMBER OF POINTS: =====================
                // even between low and high point
                fQ2 = (samples[iMid - 1] + samples[iMid]) / 2;

                int iMidMid = iMid / 2;

                //easy split 
                if (iMid % 2 == 0)
                {
                    fQ1 = (samples[iMidMid - 1] + samples[iMidMid]) / 2;
                    fQ3 = (samples[iMid + iMidMid - 1] + samples[iMid + iMidMid]) / 2;
                }
                else
                {
                    fQ1 = samples[iMidMid];
                    fQ3 = samples[iMidMid + iMid];
                }
            }
            else if (iSize == 1)
            {
                //================= SPECIAL CASE ================
                fQ1 = samples[0];
                fQ2 = samples[0];
                fQ3 = samples[0];
            }
            else
            {
                // Odd number so the median is just the midpoint in the array.
                fQ2 = samples[iMid];

                if ((iSize - 1) % 4 == 0)
                {
                    //====================== (4n-1) POINTS =========================
                    int n = (iSize - 1) / 4;
                    fQ1 = (samples[n - 1] * 0.25) + (samples[n] * 0.75);
                    fQ3 = (samples[3 * n] * 0.75) + (samples[3 * n + 1] * 0.25);
                }
                else if ((iSize - 3) % 4 == 0)
                {
                    //====================== (4n-3) POINTS =========================
                    int n = (iSize - 3) / 4;

                    fQ1 = (samples[n] * 0.75) + (samples[n + 1] * 0.25);
                    fQ3 = (samples[3 * n + 1] * 0.25) + (samples[3 * n + 2] * 0.75);
                }
            }

            return new[] { fQ1, fQ2, fQ3 };
        }
    }
}