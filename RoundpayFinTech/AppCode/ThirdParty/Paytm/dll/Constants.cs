namespace paytm.util
{
    internal class Constants
    {
        public const string VALUE_SEPARATOR_TOKEN = "|";

        public const short SALT_LENGTH = 4;

        public static bool USE_UNICODE_ENCODING = false;

        public static byte[] CRYPTO_INIT_VECTOR = new byte[]
        {
            64,
            64,
            64,
            64,
            38,
            38,
            38,
            38,
            35,
            35,
            35,
            35,
            36,
            36,
            36,
            36
        };
    }
}
