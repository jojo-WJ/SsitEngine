namespace SsitEngine.Unity.Sound
{
    public partial class SoundManager
    {
        /// <summary>
        ///     ��Ч�Ĳ��ŷ�ʽö��
        /// </summary>
        public enum PlayMethod
        {
            /// <summary>
            ///     ��������
            /// </summary>
            ContinuousPlayThrough,
            ContinuousPlayThroughWithDelay,
            ContinuousPlayThroughWithRandomDelayInRange,

            /// <summary>
            ///     ���β���
            /// </summary>
            OncePlayThrough,
            OncePlayThroughWithDelay,
            OncePlayThroughWithRandomDelayInRange,

            /// <summary>
            ///     �������
            /// </summary>
            ShufflePlayThrough,
            ShufflePlayThroughWithDelay,
            ShufflePlayThroughWithRandomDelayInRange
        }
    }
}