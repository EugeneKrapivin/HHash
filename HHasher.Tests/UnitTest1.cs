using System;
using System.Collections.Generic;
using HHash;
using NUnit.Framework;
using HHasher;
using NUnit.Framework.Interfaces;

namespace HHasher.Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

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
        public void Created_Id_Should_Be_Verifiable_ForSamePath(string[] path)
        {
            var sut = new Hasher();

            var id = sut.CreateId(path);

            Assert.That(sut.ValidateId(id, path), Is.True);
        }
    }
}