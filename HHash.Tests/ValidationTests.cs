using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace HHash.Tests
{
    [Parallelizable(ParallelScope.All)]
    public class ValidationTests
    {
        [TestCase(100), Category("validation")]
        [TestCase(10_000), Category("validation")]
        [TestCase(100_000), Category("validation")]
        [TestCase(1_000_000), Category("validation")]
        [TestCase(10_000_000, Explicit = true), Category("long running"), Category("validation")]
        [TestCase(20_000_000, Explicit = true), Category("long running"), Category("validation")]
        [TestCase(40_000_000, Explicit = true), Category("long running"), Category("validation")]
        [TestCase(100_000_000, Explicit = true), Category("long running"), Category("validation")]
        public void Created_Id_Should_Be_Unique_With_High_Probability(int limit)
        {
            var dict = new ConcurrentDictionary<string, int>();
            var sut = new Hasher();

            Parallel.For(0, limit + 1, new ParallelOptions{ MaxDegreeOfParallelism = 8 }, i =>
            {
                var key = sut.CreateId("hello", "world");
                
                dict.AddOrUpdate(key, 1, (k, v) => v + 1);
            });

            var foundDuplicate = dict.Any(x => x.Value > 1);

            Assert.That(foundDuplicate, Is.False, 
                () => $"Found {dict.Count(x => x.Value > 1)} duplicate keys\r\n: {string.Join("\r\n\t", getDuplicateKeysOutput(dict))}");
            
            // TODO: dump whole dictionary

            IEnumerable<string> getDuplicateKeysOutput(ConcurrentDictionary<string, int> keys) 
                => keys.Where(x => x.Value > 2).OrderByDescending(x => x.Value).Select(x => $"{x.Key} => {x.Value}");
        }
    }
}