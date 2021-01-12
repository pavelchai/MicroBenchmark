using System;
using System.Threading;
using NUnit.Framework;

namespace MicroBenchmark.Tests
{
    public sealed class BenchmarkTests
    {
        private const int sleepInterval = 1;

        private const int sleepIntervalWithDelta = sleepInterval + 1;

        [Test]
        public void Run_ActionNull_ArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Benchmark.Run(null));
        }

        [Test]
        public void Run_NNegative_ArgumentException()
        {
            Assert.Throws<ArgumentException>(() => Benchmark.Run(() => { }, N: -10));
        }

        [Test]
        public void Run_NSkipNegative_ArgumentException()
        {
            Assert.Throws<ArgumentException>(() => Benchmark.Run(() => { }, NSkip: -10));
        }

        [Test]
        public void Run_NLessThanNStart_ArgumentException()
        {
            Assert.Throws<ArgumentException>(() => Benchmark.Run(() => { }, N: 10, NSkip: 20));
        }

        [Test]
        public void Run_UseTestAction_ActionRaised()
        {
            int i = 0;
            Action testAction = () => i++;
            Benchmark.Run(testAction, N: 100, NSkip: 10);

            Assert.AreEqual(90, i);
        }

        [Test]
        public void Run_UseBeforeAction_ActionRaised()
        {
            int i = 0;
            Action beforeAction = () => i++;
            Benchmark.Run(() => { }, beforeAction: beforeAction, N : 100, NSkip: 10);

            Assert.AreEqual(90, i);
        }

        [Test]
        public void Run_UseAfterAction_ActionRaised()
        {
            int i = 0;
            Action afterAction = () => i++;
            Benchmark.Run(() => { }, afterAction: afterAction, N: 100, NSkip: 10);

            Assert.AreEqual(90, i);
        }

        [Test]
        public void Run_BeforeActionNull_NoErrors()
        {
            Benchmark.Run(() => { }, beforeAction: null, N: 100, NSkip: 10);
            Assert.Pass();
        }

        [Test]
        public void Run_AfterActionNull_NoErrors()
        {
            Benchmark.Run(() => { }, afterAction: null, N: 100, NSkip: 10);
            Assert.Pass();
        }

        [Test]
        public void Run_NoFilteringNoOutliers_NormalResult()
        {
            var result = Benchmark.Run(() => { Thread.Sleep(sleepInterval); }, N: 10, NSkip: 0, filterOutliers: false, k: 1.5);
            Assert.True(result.Mean >= sleepInterval * 1E-3);
            Assert.True(result.Mean < sleepIntervalWithDelta * 1E-3);
        }

        [Test]
        public void Run_NoFilteringWithOutliers_InvalidResult()
        {
            int i = 0;
            var result = Benchmark.Run(() => { Thread.Sleep(i++ == 0 ? 10 * sleepInterval : sleepInterval); }, N: 10, NSkip: 0, filterOutliers: false, k: 1.5);
            Assert.True(result.Mean >= sleepInterval * 1E-3);
            Assert.False(result.Mean < sleepIntervalWithDelta * 1E-3);
        }

        [Test]
        public void Run_WithFilteringNoOutliers_NormalResult()
        {
            var result = Benchmark.Run(() => { Thread.Sleep(sleepInterval); }, N: 10, NSkip: 0, filterOutliers: true);
            Assert.True(result.Mean >= sleepInterval * 1E-3);
            Assert.True(result.Mean < sleepIntervalWithDelta * 1E-3);
        }

        [Test]
        public void Run_WithFilteringWithOutliers_NormalResult()
        {
            int i = 0;
            var result = Benchmark.Run(() => { Thread.Sleep(i++ == 0 ? 10 * sleepInterval : sleepInterval); }, N: 10, NSkip: 0, filterOutliers: true, k: 1.5);
            Assert.True(result.Mean >= sleepInterval * 1E-3);
            Assert.True(result.Mean < sleepIntervalWithDelta * 1E-3);
        }

        [Test]
        public void Run_WithFilteringWithOutliersWithBigK_InvalidResult()
        {
            int i = 0;
            var result = Benchmark.Run(() => { Thread.Sleep(i++ == 0 ? 10 * sleepInterval : sleepInterval); }, N: 10, NSkip: 0, filterOutliers: true, k: 100);
            Assert.True(result.Mean >= sleepInterval * 1E-3);
            Assert.False(result.Mean < sleepIntervalWithDelta * 1E-3);
        }
    }
}