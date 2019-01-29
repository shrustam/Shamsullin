using System;
using System.Collections;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Web.Services;
using Shamsullin.Common;
using Shamsullin.Wcf;

namespace Shamsullin.WcfCache
{
    [ServiceContract]
    [WcfRestErrorHandler]
    public class WcfCacheService : WebService
    {
        private static readonly Hashtable Hashtable = new Hashtable();

        public class Record
        {
            public readonly DateTime Timestamp = DateTime.Now;

            public string Key;

            public byte[] Value;

            public TimeSpan? Expiry;
        }

        [WebInvoke(Method = "POST")]
        public byte[] Get(string key)
        {
            var sw = Stopwatch.StartNew();
            var result = Hashtable[key] as Record;
            if (result?.Expiry != null && result.Timestamp+result.Expiry > DateTime.Now)
            {
                Hashtable[key] = null;
                return null;
            }

            Log.Instance.Debug($"Get processed in {sw.ElapsedMilliseconds}ms");
            return result?.Value;
        }

        [WebInvoke(Method = "POST")]
        public void Set(Record record)
        {
            var sw = Stopwatch.StartNew();
            Hashtable[record.Key] = record;
            Log.Instance.Debug($"Set processed in {sw.ElapsedMilliseconds}ms");
        }
    }
}