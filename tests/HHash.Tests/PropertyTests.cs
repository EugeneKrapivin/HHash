using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

using MoreLinq;

namespace HHash.Tests
{
    [Parallelizable(ParallelScope.All)]
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

        public static IEnumerable<TestCaseData> PathOrderingSensitivityCases
        {
            get
            {
                var basePath = new[] {"this", "is", "a", "long", "path"};

                var permutations = basePath.Permutations().Where(x => !x.SequenceEqual(basePath));

                foreach (var permutation in permutations)
                {
                    yield return new TestCaseData(basePath, permutation);
                }
            }
        }

        [TestCaseSource(nameof(CreationTestCases)), Category("property")]
        public void Created_Id_Should_Be_Valid_Base64(string[] path)
        {
            var sut = new Hasher();

            var id = sut.CreateId(path);

            var decodedId = Convert.FromBase64String(id);

            Assert.That(decodedId, Is.Not.Null);
            Assert.That(decodedId, Is.Not.Empty);
        }

        [TestCaseSource(nameof(CreationTestCases)), Category("property")]
        public void Created_Id_Should_Be_Verifiable_For_Same_Path(string[] path)
        {
            var sut = new Hasher();

            var id = sut.CreateId(path);

            Assert.That(sut.ValidateId(id, path), Is.True);
        }

        [TestCaseSource(nameof(PathCaseSensitivityCases)), Category("property")]
        public void Created_Id_Should_Be_Case_Sensitive(string[] path)
        {
            var sut = new Hasher();

            var id = sut.CreateId("path");

            Assert.That(sut.ValidateId(id, path), Is.False);
        }

        [TestCaseSource(nameof(CreationTestCases)), Category("property")]
        public void Created_Id_Should_Not_Be_Verifiable_For_Different_Path(string[] path)
        {
            var sut = new Hasher();

            var id = sut.CreateId("path");
            Console.WriteLine($"this key should be valid for path `[\"hello'\", \"world\"]` {sut.ValidateId(id, "hello", "world")}");
            Assert.That(sut.ValidateId(id, path), Is.False);
        }

        [TestCaseSource(nameof(PathOrderingSensitivityCases)), Category("property")]
        public void Created_Id_Should_Be_Sensitive_To_Path_Order(string [] originalPath, string[] permutation)
        {
            var sut = new Hasher();
            var id = sut.CreateId(originalPath);

            Assert.That(sut.ValidateId(id, permutation), Is.False, () => $"Failed Id validation for path {originalPath} against {permutation}");
        }
    }
}