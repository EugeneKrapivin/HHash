using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HHash;
using NUnit.Framework;
using HHasher;
using NUnit.Framework.Interfaces;

namespace HHasher.Tests
{
    public class PropertyTests
    {
        public static IEnumerable<string[]> CreationTestCases
        {
            get
            {
                yield return default;
                yield return Array.Empty<string>();
                yield return new [] { "test" };
                yield return new [] { "test", "test" };
                yield return new [] { "test", "test1" };
            }
        }

        public static IEnumerable<string[]> PathCaseSensitivityCases
        {
            get
            {
                yield return new[] { "Test" };
                yield return new[] { "tEst" };
                yield return new[] { "Test" };
                yield return new[] { "teSt" };
                yield return new[] { "tesT" };
                yield return new[] { "TEST" };
            }
        }

        [TestCaseSource(nameof(CreationTestCases))]
        public void Created_Id_Should_Be_Valid_Base64(string[] path)
        {
            var sut = new Hasher();

            var id = sut.CreateId(path);

            var decodedId = Convert.FromBase64String(id);

            Assert.That(decodedId, Is.Not.Null);
            Assert.That(decodedId, Is.Not.Empty);
        }

        [TestCaseSource(nameof(CreationTestCases))]
        public void Created_Id_Should_Be_Verifiable_For_Same_Path(string[] path)
        {
            var sut = new Hasher();

            var id = sut.CreateId(path);

            Assert.That(sut.ValidateId(id, path), Is.True);
        }

        [TestCaseSource(nameof(PathCaseSensitivityCases))]
        public void Created_Id_Should_Be_Case_Sensitive(string[] path)
        {
            var sut = new Hasher();

            var id = sut.CreateId("path");

            Assert.That(sut.ValidateId(id, path), Is.False);
        }

        [TestCaseSource(nameof(CreationTestCases))]
        public void Created_Id_Should_Be_Not_Be_Verifiable_For_Different_Path(string[] path)
        {
            var sut = new Hasher();

            var id = sut.CreateId("path");

            Assert.That(sut.ValidateId(id, path), Is.False);
        }

        [TestCase(100)]
        [TestCase(10_000)]
        [TestCase(100_000)]
        [TestCase(1_000_000)]
        [TestCase(10_000_000), Category("long running")]
        [TestCase(20_000_000), Category("long running")]
        [TestCase(40_000_000), Category("long running")]
        [TestCase(100_000_000, Explicit = true), Category("long running")]
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