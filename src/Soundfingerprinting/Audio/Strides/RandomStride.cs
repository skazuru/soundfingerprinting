﻿namespace Soundfingerprinting.Audio.Strides
{
    using System;

    /// <summary>
    ///   Random stride object. [min - max Samples stride]
    /// </summary>
    [Serializable]
    public class RandomStride : IStride
    {
        private static readonly Random Random = new Random(unchecked((int)DateTime.Now.Ticks));

        private readonly int firstStride;

        /// <summary>
        ///   Max stride
        /// </summary>
        private readonly int maxStride;

        /// <summary>
        ///   Min stride
        /// </summary>
        private readonly int minStride;

        /// <summary>
        /// Initializes a new instance of the <see cref="RandomStride"/> class. 
        ///   Constructor for a random stride object
        /// </summary>
        /// <param name="minStride">
        /// Minimum stride size, generated by this object
        /// </param>
        /// <param name="maxStride">
        /// Maximum stride size, generated by this object
        /// </param>
        public RandomStride(int minStride, int maxStride)
        {
            if (minStride < 0 || maxStride < 0 || minStride > maxStride)
            {
                throw new ArgumentException("Bad arguments. Please review the documentation");
            }

            this.minStride = minStride;
            this.maxStride = maxStride;
            firstStride = Random.Next(minStride, maxStride);
        }

        public RandomStride(int minStride, int maxStride, int firstStride) : this(minStride, maxStride)
        {
            this.firstStride = firstStride;
        }

        /// <summary>
        ///   Gets min stride size
        /// </summary>
        public int Min
        {
            get
            {
                return minStride;
            }
        }

        /// <summary>
        ///   Gets max stride size
        /// </summary>
        public int Max
        {
            get
            {
                return maxStride;
            }
        }

        #region IStride Members

        /// <summary>
        ///   Get's stride size in terms of number of samples, which are needed to be skipped
        /// </summary>
        /// <returns>Bit samples to skip, between 2 consecutive overlapping fingerprints</returns>
        public int StrideSize
        {
            get
            {
                return Random.Next(minStride, maxStride);
            }
        }

        public int FirstStrideSize
        {
            get
            {
                return firstStride;
            }
        }

        #endregion
    }
}