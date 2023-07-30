using Microsoft.AspNetCore.Identity;
using System;
using System.Security.Cryptography;

namespace HotsLogsApi.Auth;

public class MyPasswordHasher2 : IPasswordHasher<ApplicationUser>
{
    private const int Hashiterations = 1500;

    public string HashPassword(ApplicationUser user, string password)
    {
        return HashPassword(password);
    }

    public PasswordVerificationResult VerifyHashedPassword(
        ApplicationUser user,
        string hashedPassword,
        string providedPassword)
    {
        return VerifyHashedPassword(hashedPassword, providedPassword);
    }

    private string HashPassword(string password)
    {
        // use Crypto-Safe PseudoRandom Number Generator (CSPRNG) to make salt array
        var salt = RandomNumberGenerator.GetBytes(16);
        //byte[] salt;
        //new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);


        // hash and salt using PBKDF2 (purposely 'slow' hash, runs through HASHITERATIONS times)
        var hashedSaltedPw = new Rfc2898DeriveBytes(password, salt, Hashiterations, HashAlgorithmName.SHA1);

        // change to byte array
        var hash = hashedSaltedPw.GetBytes(20);

        // new byte array to store concatenated salt and hash (16 for salt, 20 for hash)
        var ccHashBytes = new byte[36];

        // concatenate them in the correct order
        Array.Copy(salt, 0, ccHashBytes, 0, 16);
        Array.Copy(hash, 0, ccHashBytes, 16, 20);

        // convert byte array to string for storage in DB
        return Convert.ToBase64String(ccHashBytes);
    }

    private PasswordVerificationResult VerifyHashedPassword(string hashedPassword, string providedPassword)
    {
        // convert to bytes from string
        var pwBytes = Convert.FromBase64String(hashedPassword);

        // pull out the salt - it's the first 16 bytes of the hashed pw
        var salt = new byte[16];
        Array.Copy(pwBytes, 0, salt, 0, 16);

        // hash the given password with the saved salt
        var givenHashed = new Rfc2898DeriveBytes(providedPassword, salt, Hashiterations, HashAlgorithmName.SHA1);
        // array it out so can compare byte-by-byte
        var hash = givenHashed.GetBytes(20);

        // start comparing - we start at the 16th (0 ordinal) byte because that's where the pw starts
        for (var i = 0; i < 20; i++)
        {
            // if you hit any differences, no match. end function
            if (pwBytes[i + 16] != hash[i])
            {
                return PasswordVerificationResult.Failed;
            }
        }

        // made it here? passwords match!
        return PasswordVerificationResult.Success;
    }
}
