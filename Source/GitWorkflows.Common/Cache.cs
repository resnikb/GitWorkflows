using System;

namespace GitWorkflows.Common
{
    public sealed class Cache<T>
    {
        private readonly Func<T> _hydrate;
        private volatile bool _isValid;
        private T _cachedValue;

        public T Value
        {
            get
            {
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
        
        public Cache(Func<T> hydrate)
        {
            Arguments.EnsureNotNull(new{ hydrate });
            _hydrate = hydrate;
        }

        public void Invalidate()
        { _isValid = false; }
    }
}