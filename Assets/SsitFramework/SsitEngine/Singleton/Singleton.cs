namespace SsitEngine
{
    /// <summary>
    ///     底层单列泛型基类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Singleton<T> where T : class, ISingleton, new()
    {
        //单列实例
        private static T s_instance;
        private static readonly object m_lockObject = new object();

        /// <summary>
        ///     单列对象
        /// </summary>
        public static T Instance
        {
            get
            {
                // DCL 
                if (s_instance == null)
                    lock (m_lockObject)
                    {
                        if (s_instance == null)
                        {
                            s_instance = new T();
                            s_instance.OnSingletonInit();
                        }
                    }

                return s_instance;
            }
        }
    }
}