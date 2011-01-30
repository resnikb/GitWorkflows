using System;

namespace GitWorkflows.Package.Common
{
    public class Cache<T>
    {
        private readonly Func<T> _hydrate;
        private readonly TimeSpan? _validityTime;
        private DateTime _expirationTime = DateTime.Now;
        private T _cachedValue;

        public T Value
        {
            get
            {
                if (_expirationTime <= DateTime.Now)
                {
                    _cachedValue = _hydrate();
                    _expirationTime = _validityTime.HasValue ? DateTime.Now.Add(_validityTime.Value) : DateTime.MaxValue;
                }

                return _cachedValue;
            }
        }
        
        public Cache(Func<T> hydrate, TimeSpan? validityTime = null)
        {
            _hydrate = hydrate;
            _validityTime = validityTime;
        }

        public void Invalidate()
        { _expirationTime = DateTime.Now; }
    }
}