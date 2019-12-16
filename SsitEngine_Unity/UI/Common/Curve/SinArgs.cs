using System;

namespace SsitEngine.Unity.UI.Common.Curve
{
    /// <summary>Args of sin.</summary>
    [Serializable]
    public struct SinArgs
    {
        /// <summary>Amplitude of sin.</summary>
        public float amplitude;

        /// <summary>Angular of sin.</summary>
        public float angular;

        /// <summary>Initial phase of sin.</summary>
        public float phase;

        /// <summary>Setover of sin.</summary>
        public float setover;

        /// <summary>Constructor.</summary>
        /// <param name="amplitude">Amplitude of sin.</param>
        /// <param name="angular">Angular of sin.</param>
        /// <param name="phase">Initial phase of sin.</param>
        /// <param name="setover">Setover of sin.</param>
        public SinArgs( float amplitude, float angular, float phase, float setover )
        {
            this.amplitude = amplitude;
            this.angular = angular;
            this.phase = phase;
            this.setover = setover;
        }
    }
}