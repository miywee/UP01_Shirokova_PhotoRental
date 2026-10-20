using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace UP01_Shirokova_PhotoRental
{
    internal static class PasswordHelper
    {
        public static bool VerifyPassword(
            string password, string storedHash)
        {
            if (string.IsNullOrEmpty(storedHash))
                return false;

            try
            {
                string[] parts = storedHash.Split('$');

                if (parts.Length != 4 ||
                    parts[0] != "PBKDF2")
                    return false;

                int iterations;

                if (!int.TryParse(parts[1], out iterations)
                    || iterations < 10000)
                    return false;

                byte[] salt =
                    Convert.FromBase64String(parts[2]);

                byte[] expected =
                    Convert.FromBase64String(parts[3]);

                if (salt.Length < 16 ||
                    expected.Length != 32)
                    return false;

                using (var derive =
                    new Rfc2898DeriveBytes(
                        password,
                        salt,
                        iterations,
                        HashAlgorithmName.SHA256))
                {
                    byte[] actual =
                        derive.GetBytes(expected.Length);

                    int difference = 0;

                    for (int i = 0; i < actual.Length; i++)
                    {
                        difference |=
                            actual[i] ^ expected[i];
                    }

                    return difference == 0;
                }
            }
            catch (FormatException)
            {
                return false;
            }
            catch (CryptographicException)
            {
                return false;
            }


        }

        public static string HashPassword(string password)
        {
            const int iterations = 100000;

            byte[] salt = new byte[16];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            using (var derive = new Rfc2898DeriveBytes(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256))
            {
                byte[] hash = derive.GetBytes(32);

                return "PBKDF2$" + iterations + "$" +
                       Convert.ToBase64String(salt) + "$" +
                       Convert.ToBase64String(hash);
            }
        }

    }
}
