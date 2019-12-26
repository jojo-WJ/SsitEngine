namespace SsitEngine.Unity
{
    /// <summary>
    ///     继承Mono的单列
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SingletonMono<T> : SingletonMonoBase, ISingleton where T : SingletonMono<T>
    {
        protected static T _mInstance;
        private static readonly object s_lock = new object();

        public static T Instance
        {
            get
            {
                lock (s_lock)
                {
                    if (!_mInstance)
                    {
                        _mInstance = CreateMonoSingleton<T>();
                    }
                }
                return _mInstance;
            }
            set => _mInstance = value;
        }

        /// <summary>
        ///     初始化方法
        /// </summary>
        public virtual void OnSingletonInit()
        {
        }
    }
}