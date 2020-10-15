using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace HHash
{
	public class Hasher
	{
		private static readonly RNGCryptoServiceProvider Rng = new RNGCryptoServiceProvider();
        
        [ThreadStatic] // https://medium.com/@wanderingdeveloper/c-cautionary-tail-the-dangers-of-sha256-reuse-2b5bb9c6fde9
		private static SHA256 _sha256;

        private static SHA256 Sha256
        {
            get
            {
                if (_sha256 is null)
                {
                    _sha256 = SHA256.Create();
                }

                return _sha256;
            }
        }

        public string CreateId(params string[] parts)
        {
            var bytes = new byte[8];
			Rng.GetBytes(bytes);

			return Convert.ToBase64String(Calculate(bytes, parts ?? Array.Empty<string>()));
		}

        public bool ValidateId(string id, params string[] parts)
		{
			var idBytes = Convert.FromBase64String(id);

			var hashFromId = new byte[8];
			var bytesFromId = new byte[8];
			Array.Copy(idBytes, hashFromId, 8);
			Array.Copy(idBytes, 8, bytesFromId, 0, 8);

			var calculatedHash = Calculate(bytesFromId, parts ?? Array.Empty<string>());

			return idBytes.SequenceEqual(calculatedHash);
		}

        private static byte[] Calculate(byte[] random, IEnumerable<string> parts)
        {
            return parts
                .Select(Encoding.UTF8.GetBytes)
                .Select(Sha256.ComputeHash)
                .Aggregate(
                    Sha256.ComputeHash(random),
                    (acc, part) =>
                    {
                        var concat = new byte[acc.Length + part.Length];
                        Array.Copy(acc, concat, acc.Length);
                        Array.Copy(part, 0, concat, acc.Length - 1, part.Length);

                        return Sha256.ComputeHash(concat);
                    },
                    hash =>
                    {
                        var concat = new byte[16];
                        Array.Copy(hash, concat, 8);
                        Array.Copy(random, 0, concat, 8, 8);

                        return concat;
                    });
        }
    }
}
