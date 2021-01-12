using NUnit.Framework;

namespace MicroBenchmark.Tests
{
    public sealed class BenchmarkResultTests
    {
        [Test]
        public void Create()
        {
            var result = new BenchmarkResult(1, 1, 1, 1, 1, 1);
            Assert.Pass();
        }

        [Test]
        public void GetString()
        {
            var result = new BenchmarkResult(1, 1, 1, 1, 1, 1);
            Assert.NotNull(result.ToString());
        }
    }
}