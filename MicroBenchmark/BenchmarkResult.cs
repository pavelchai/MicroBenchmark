/*
 * Licensed under MIT.
 * Copyright © 2021 Pavel Chaimardanov. All rights reserved.
 */

namespace MicroBenchmark
{
    /// <summary>
    /// Represents the results from <see cref="Benchmark.Run"></see> method.
    /// </summary>
    public sealed class BenchmarkResult
    {
        /// <summary>
        /// First quartile (in s).
        /// </summary>
        public readonly double Q1;

        /// <summary>
        /// Second quartile (in s).
        /// </summary>
        public readonly double Q2;

        /// <summary>
        /// Third quartile (in s).
        /// </summary>
        public readonly double Q3;

        /// <summary>
        /// Mean (in s).
        /// </summary>
        public readonly double Mean;

        /// <summary>
        /// Standard deviation (in s).
        /// </summary>
        public readonly double StdDev;

        /// <summary>
        /// Resolution of the measurement (in s).
        /// </summary>
        public readonly double Resolution;

        /// <summary>
        /// Creates a new results from <see cref="Benchmark.Run"></see> method.
        /// </summary>
        /// <param name="Q1"> First quartile (in s). </param>
        /// <param name="Q2"> Second quartile (in s). </param>
        /// <param name="Q3"> Third quartile (in s). </param>
        /// <param name="mean"> Mean (in s). </param>
        /// <param name="stdDev"> Standard deviation (in s). </param>
        /// <param name="resolution"> Resolution of the measurement (in s). </param>
        public BenchmarkResult(double Q1, double Q2, double Q3, double mean, double stdDev, double resolution)
        {
            this.Q1 = Q1;
            this.Q2 = Q2;
            this.Q3 = Q3;
            this.Mean = mean;
            this.StdDev = stdDev;
            this.Resolution = resolution;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format("Mean = {0:E3} s; Std.Dev = {1:E3} s; Q1 = {2:E3} s; Q2 = {3:E3} s; Q3 = {4:E3} s; Resolution: {5:E3} s", this.Mean, this.StdDev, this.Q1, this.Q2, this.Q3, this.Resolution).ToString();
        }
    }
}