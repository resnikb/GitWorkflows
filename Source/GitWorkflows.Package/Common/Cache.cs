using System;

namespace GitWorkflows.Package.Common
{
    sealed class Cache<T>
    {
        private readonly object _syncRoot = new object();
        private readonly Func<T> _hydrate;
        private readonly TimeSpan? _validityTime;
        private DateTime _expirationTime;
        private T _cachedValue;

        public T Value
        {
            get
            {
                lock (_syncRoot)
                {
                    if (_expirationTime <= DateTime.Now)
                    {
                        _cachedValue = _hydrate();
                        _expirationTime = _validityTime.HasValue ? DateTime.Now.Add(_validityTime.Value) : DateTime.MaxValue;
                    }
                }

                return _cachedValue;
            }
        }
        
        public Cache(Func<T> hydrate, TimeSpan? validityTime = null)
        {
            _hydrate = hydrate;
            _validityTime = validityTime;
            _expirationTime = DateTime.MinValue;
        }

        public void Invalidate()
        {
            lock (_syncRoot)
                _expirationTime = DateTime.Now;
        }
    }
}