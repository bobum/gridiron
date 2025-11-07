using DomainObjects.Helpers;

namespace UnitTestProject1.Helpers
{
    public class TestSeedableRandom : ISeedableRandom
    {
        private int _intIndex = 0;
        private int _doubleIndex = 0;
        public double[] __NextDouble { get; set; } = new double[99];
        public int[] __NextInt { get; set; } = new int[99];
        public void GetBytes(byte[] buffer)
        {
            throw new System.NotImplementedException();
        }

        public double NextDouble()
        {
            var returnVal = __NextDouble[_doubleIndex];
            _doubleIndex++;
            return returnVal;
        }

        public int Next(int minValue, int maxValue)
        {
            // Return value clamped to the specified range
            var value = __NextInt[_intIndex];
            _intIndex++;
            return Math.Max(minValue, Math.Min(maxValue - 1, value));
        }

        public int Next()
        {
            var returnVal = __NextInt[_intIndex];
            _intIndex++;
            return returnVal;
        }

        public int Next(int maxValue)
        {
            return Next();
        }

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }

        public void GetNonZeroBytes(byte[] data)
        {
            throw new System.NotImplementedException();
        }
    }
}



//namespace DomainObjects.Helpers
//{
//    public interface ICryptoRandom
//    {
//        ///<summary>
//        /// Fills the elements of a specified array of bytes with random numbers.
//        ///</summary>
//        ///<param name="buffer">An array of bytes to contain random numbers.</param>
//        void GetBytes(byte[] buffer);

//        ///<summary>
//        /// Returns a random number between 0.0 and 1.0.
//        ///</summary>
//        double NextDouble();

//        ///<summary>
//        /// Returns a random number within the specified range.
//        ///</summary>
//        ///<param name="minValue">The inclusive lower bound of the random number returned.</param>
//        ///<param name="maxValue">The exclusive upper bound of the random number returned. maxValue must be greater than or equal to minValue.</param>
//        int Next(int minValue, int maxValue);

//        ///<summary>
//        /// Returns a nonnegative random number.
//        ///</summary>
//        int Next();

//        ///<summary>
//        /// Returns a nonnegative random number less than the specified maximum
//        ///</summary>
//        ///<param name="maxValue">The inclusive upper bound of the random number returned. maxValue must be greater than or equal 0</param>
//        int Next(int maxValue);

//        void Dispose();
//        void GetNonZeroBytes(byte[] data);
//    }

//    ///<summary>
//    /// Represents a pseudo-random number generator, a device that produces random data.
//    ///</summary>
//    public class CryptoRandom : RandomNumberGenerator, ICryptoRandom
//    {
//        private static RandomNumberGenerator _r;

//        ///<summary>
//        /// Creates an instance of the default implementation of a cryptographic random number generator that can be used to generate random data.
//        ///</summary>
//        public CryptoRandom()
//        {
//            _r = Create();
//        }

//        ///<summary>
//        /// Fills the elements of a specified array of bytes with random numbers.
//        ///</summary>
//        ///<param name="buffer">An array of bytes to contain random numbers.</param>
//        public override void GetBytes(byte[] buffer)
//        {
//            _r.GetBytes(buffer);
//        }

//        ///<summary>
//        /// Returns a random number between 0.0 and 1.0.
//        ///</summary>
//        public double NextDouble()
//        {
//            byte[] b = new byte[4];
//            _r.GetBytes(b);
//            return BitConverter.ToUInt32(b, 0) / ((double)uint.MaxValue + 1);
//        }

//        ///<summary>
//        /// Returns a random number within the specified range.
//        ///</summary>
//        ///<param name="minValue">The inclusive lower bound of the random number returned.</param>
//        ///<param name="maxValue">The exclusive upper bound of the random number returned. maxValue must be greater than or equal to minValue.</param>
//        public int Next(int minValue, int maxValue)
//        {
//            long range = (long)maxValue - minValue;
//            return (int)((long)Math.Floor(NextDouble() * range) + minValue);
//        }

//        ///<summary>
//        /// Returns a nonnegative random number.
//        ///</summary>
//        public int Next()
//        {
//            return Next(0, int.MaxValue);
//        }

//        ///<summary>
//        /// Returns a nonnegative random number less than the specified maximum
//        ///</summary>
//        ///<param name="maxValue">The inclusive upper bound of the random number returned. maxValue must be greater than or equal 0</param>
//        public int Next(int maxValue)
//        {
//            return Next(0, maxValue);
//        }
//    }
//}