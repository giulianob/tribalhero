using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Game.Util.TwoFactor
{

    // Please see LICENSE.txt for information on what license this is released under
    public enum HashAlgorithm
    {
        Sha1
    }

    // This class was ported from this PHP library https://github.com/lelag/otphp which was ported from a Ruby library
    /**
     * One Time Password Generator 
     * 
     * The OTP class allow the generation of one-time
     * password that is described in rfc 4xxx.
     * 
     * This is class is meant to be compatible with 
     * Google Authenticator.
     *
     * This class was originally ported from the rotp
     * ruby library available at https://github.com/mdp/rotp
     */

    public class Otp
    {
        /**
         * The base32 encoded secret key
         * @var string
         */

        public string Secret;

        /**
         * The algorithm used for the hmac hash function
         * @var string
         */

        public HashAlgorithm Digest;

        /**
         * The number of digits in the one-time password
         * @var integer
         */

        public int Digits;

        /**
         * Constructor for the OTP class
         * @param string $secret the secret key
         * @param array $opt options array can contain the
         * following keys :
         *   @param integer digits : the number of digits in the one time password
         *   Currently Google Authenticator only support 6. Defaults to 6.
         *   @param string digest : the algorithm used for the hmac hash function
         *   Google Authenticator only support sha1. Defaults to sha1
         *
         * @return new OTP class.
         */

        public Otp(string secret)
        {
            this.Secret = secret;
            this.Digits = 6;
            this.Digest = HashAlgorithm.Sha1;
        }

        public Otp(string secret, int digits)
        {
            this.Secret = secret;
            this.Digits = digits;
            this.Digest = HashAlgorithm.Sha1;
        }

        public Otp(string secret, int digits, HashAlgorithm digest)
        {
            this.Secret = secret;
            this.Digits = digits;
            this.Digest = digest;
        }

        /**
         * Generate a one-time password
         *
         * @param integer $input : number used to seed the hmac hash function.
         * This number is usually a counter (HOTP) or calculated based on the current
         * timestamp (see TOTP class).
         * @return integer the one-time password 
         */

        public int GenerateOtp(Int64 input)
        {

            HMAC hashgenerator;
            switch (Digest)
            {
                case HashAlgorithm.Sha1:
                    hashgenerator = new HMACSHA1(Base32Encoding.ToBytes(Secret));
                    break;
                default:
                    throw new Exception("Unknown hash");
            }


            hashgenerator.ComputeHash(IntToByteString(input));
            string hash = "";

            foreach (byte b in hashgenerator.Hash)
            {
                hash += b.ToString("x2");
            }

            List<int> hmac = new List<int>();
            foreach (string s in hash.Split(2))
            {
                hmac.Add(Int32.Parse(s, System.Globalization.NumberStyles.HexNumber));
            }

            int offset = hmac[19] & 0xf;
            int code = (hmac[offset + 0] & 0x7F) << 24 |
                       (hmac[offset + 1] & 0xFF) << 16 |
                       (hmac[offset + 2] & 0xFF) << 8 |
                       (hmac[offset + 3] & 0xFF);

            return code % (int)Math.Pow(10, this.Digits);

        }

        public byte[] IntToByteString(Int64 i)
        {
            List<byte> res = new List<byte>();

            while (i != 0)
            {
                res.Add((byte)(i & 0xFF));
                i >>= 8;
            }
            int rcount = res.Count;
            for (int z = 0; z < 8 - rcount; z++)
                res.Add(0);
            res.Reverse();

            return res.ToArray();
        }


    }

}
