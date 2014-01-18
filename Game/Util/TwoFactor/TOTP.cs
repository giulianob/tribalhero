using System;

namespace Game.Util.TwoFactor
{
    /**
        * TOTP - One time password generator 
        * 
        * The TOTP class allow for the generation 
        * and verification of one-time password using 
        * the TOTP specified algorithm.
        *
        * This class is meant to be compatible with 
        * Google Authenticator
        *
        * This class was originally ported from the rotp
        * ruby library available at https://github.com/mdp/rotp
        */
    public class Totp : Otp
    {

        /**
         * The interval in seconds for a one-time password timeframe
         * Defaults to 30
         * @var integer
         */
        public double Interval;


        public Totp(string secret)
            : base(secret, 6, HashAlgorithm.Sha1)
        {
            this.Interval = 30;
        }

        public Totp(string secret, double interval)
            : base(secret, 6, HashAlgorithm.Sha1)
        {
            this.Interval = interval;
        }

        public Totp(string secret, double interval, int digits)
            : base(secret, digits, HashAlgorithm.Sha1)
        {
            this.Interval = interval;
        }

        public Totp(string secret, double interval, int digits, HashAlgorithm algo)
            : base(secret, digits, algo)
        {
            this.Interval = interval;
        }

        /**
         *  Get the password for a specific timestamp value 
         *
         *  @param integer $timestamp the timestamp which is timecoded and 
         *  used to seed the hmac hash function.
         *  @return integer the One Time Password
         */
        public int At(double timestamp)
        {
            return this.GenerateOtp(this.Timecode(timestamp));
        }

        /**
         *  Get the password for the current timestamp value 
         *
         *  @return integer the current One Time Password
         */
        public int Now()
        {
            return this.At(new Unixtime().ToTimeStamp());
        }

        /**
         * Verify if a password is valid for a specific counter value
         *
         * @param integer $otp the one-time password 
         * @param integer $timestamp the timestamp for the a given time, defaults to current time.
         * @return  bool true if the counter is valid, false otherwise
         */
        public bool Verify(int otp, double timestamp)
        {
            return (otp == this.At(timestamp));
        }

        public bool Verify(int otp)
        {
            for (var i = -1; i <= 1; i++)
            {
                if (this.Verify(otp, new Unixtime().ToTimeStamp() + (i * Interval)))
                {
                    return true;
                }
            }

            return false;
        }

        /**
         * Returns the uri for a specific secret for totp method.
         * Can be encoded as a image for simple configuration in 
         * Google Authenticator.
         *
         * @param string $name the name of the account / profile
         * @return string the uri for the hmac secret
         */
        public string ProvisitioningUri(string name)
        {
            return "otpauth://totp/" + name + "?secret=" + this.Secret;
        }

        /**
         * Transform a timestamp in a counter based on specified internal
         *
         * @param integer $timestamp
         * @return integer the timecode
         */
        public Int64 Timecode(double timestamp)
        {
            return (Int64)(((((timestamp * 1000)) / (this.Interval * 1000))));
        }
    }
}
