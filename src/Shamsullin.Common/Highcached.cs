using System;
using System.Runtime.Caching;
using Shamsullin.Common.Extensions;

namespace Shamsullin.Common
{
    /// <summary>
    /// Cache result of execution method. Can be used to cache query result. Preloading is included.
    /// </summary>
    public class Highcached
    {
        public delegate T Geter<out T>();

        private readonly bool _usePreload;
        private readonly double? _minutesForCache;

        public Highcached(double? minutesForCache, bool usePreload = true)
        {
            _minutesForCache = minutesForCache;
            _usePreload = usePreload;
        }

        public static void Clear()
        {
            foreach (var element in MemoryCache.Default)
            {
                MemoryCache.Default.Remove(element.Key);
            }
        }

        private void SetCache(string key, object value, DateTime expiry)
        {
            if (!_minutesForCache.HasValue) return;
            MemoryCache.Default.Set(key, value ?? DBNull.Value, new CacheItemPolicy {AbsoluteExpiration = expiry});
        }

        private object GetCache(string key)
        {
            var result = MemoryCache.Default.Get(key);
            return result;
        }

        public T Get<T>(Geter<T> invocation, string key)
        {
            if (!_minutesForCache.HasValue) return invocation();
            var keyRefreshMe = string.Concat("!", key);
            var result = GetCache(key);
            if (result == null)
            {
                lock (key.SyncRoot())
                {
                    result = GetCache(key);
                    if (result == null)
                    {
                        Log.Instance.DebugFormat("Highcached {1} min miss {0}", key, _minutesForCache.Value);
                        result = invocation();
                        SetCache(key, result, DateTime.Now.AddMinutes(_minutesForCache.Value));
                        SetCache(keyRefreshMe, 1, DateTime.Now.AddMinutes(_minutesForCache.Value/2));
                    }
                }
            }
            else if (_usePreload && GetCache(keyRefreshMe) == null)
            {
                // It's time to preload data for future requests.
                new AsyncManager(delegate
                {
                    lock (key.SyncRoot())
                    {
                        if (GetCache(keyRefreshMe) == null)
                        {
                            var newCacheResult = invocation();
                            SetCache(key, newCacheResult, DateTime.Now.AddMinutes(_minutesForCache.Value));
                            SetCache(keyRefreshMe, 1, DateTime.Now.AddMinutes(_minutesForCache.Value/2));
                        }
                    }
                }).ExecuteAsync();
            }

            if (result == DBNull.Value) return default(T);
            if (result != null && !(result is T))
                Log.Instance.WarnFormat("Highcached returned wrong object for key {0}", key);
            return (T) result;
        }

        public T GetAndRefresh<T>(Geter<T> invocation, string key)
        {
            if (!_minutesForCache.HasValue) return invocation();
            var keyRefreshMe = string.Concat("!", key);
            lock (key.SyncRoot())
            {
                var result = invocation();
                SetCache(key, result, DateTime.Now.AddMinutes(_minutesForCache.Value));
                SetCache(keyRefreshMe, 1, DateTime.Now.AddMinutes(_minutesForCache.Value/2));
                return result;
            }
        }
    }
}