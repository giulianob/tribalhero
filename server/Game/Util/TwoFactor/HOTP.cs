namespace Game.Util.TwoFactor
{
    /**
         * HOTP - One time password generator 
         * 
         * The HOTP class allow for the generation 
         * and verification of one-time password using 
         * the HOTP specified algorithm.
         *
         * This class is meant to be compatible with 
         * Google Authenticator
         *
         * This class was originally ported from the rotp
         * ruby library available at https://github.com/mdp/rotp
         */
    public class Hotp : Otp
    {

        public Hotp(string secret)
            : base(secret, 6, HashAlgorithm.Sha1)
        {

        }

        /**
         *  Get the password for a specific counter value
         *  @param integer $count the counter which is used to
         *  seed the hmac hash function.
         *  @return integer the One Time Password
         */
        public int At(int count)
        {
            return this.GenerateOtp(count);
        }

        /**
         * Verify if a password is valid for a specific counter value
         *
         * @param integer $otp the one-time password 
         * @param integer $counter the counter value
         * @return  bool true if the counter is valid, false otherwise
         */
        public bool Verify(int otp, int counter)
        {
            return (otp == this.At(counter));
        }

        public string ProvisioningUri(string name, int initialCount)
        {
            return "otpauth://hotp/" + name + "?secret=" + this.Secret + " &counter=" + initialCount;
        }
    }
}
