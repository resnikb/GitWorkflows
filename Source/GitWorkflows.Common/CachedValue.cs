using System;

namespace GitWorkflows.Common
{
    /// <summary>
    /// Represents a value that can be recalculated when needed.
    /// </summary>
    /// 
    /// <typeparam name="T">Type of the value.</typeparam>
    /// 
    /// <remarks>
    ///     <para>When an instance of CachedValue is created, the client supplies a delegate that is
    ///     used to calculate the cached value. The value is calculated when it is first accessed,
    ///     and is cached until <see cref="Invalidate"/> method is invoked. After that, the value is
    ///     recalculated using the hydration function when it is next accessed.
    ///     </para>
    /// 
    ///     <para>All methods are thread-safe.</para>
    /// </remarks>
    public sealed class CachedValue<T>
    {
        private readonly Func<T> _hydrate;
        private volatile bool _isValid;
        private T _cachedValue;

        /// <summary>
        /// Gets the cached value.
        /// </summary>
        /// 
        /// <value>The value.</value>
        /// 
        /// <remarks>
        ///     <para>Accessing this property will cause the value to be re hydrated if needed.</para>
        /// </remarks>
        public T Value
        {
            get
            {
                // Double checked locking pattern
                if (!_isValid)
                {
                    lock (_hydrate)
                    {
                        if (!_isValid)
                        {
                            _cachedValue = _hydrate();
                            _isValid = true;
                        }
                    }
                }

                return _cachedValue;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CachedValue&lt;T&gt;"/> class.
        /// </summary>
        /// 
        /// <param name="hydrate">The hydration delegate. This delegate will be used to calculate
        /// the value when needed</param>
        /// 
        /// <exception cref="ArgumentNullException"><paramref name="hydrate"/> is <c>null</c>.</exception>
        public CachedValue(Func<T> hydrate)
        {
            Arguments.EnsureNotNull(new{ hydrate });
            _hydrate = hydrate;
        }

        /// <summary>
        /// Invalidates the cached value.
        /// </summary>
        /// 
        /// <remarks>
        ///     <para>Invoking this method marks the cached value as invalid, and will cause the
        ///     value to be recalculated when it is accessed the next time. Invoking this method
        ///     multiple times is safe and has the same effect as calling it once.</para>
        /// </remarks>
        public void Invalidate()
        { _isValid = false; }
    }
}