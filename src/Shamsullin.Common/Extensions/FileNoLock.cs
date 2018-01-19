using System.IO;

namespace Shamsullin.Common.Extensions
{
    /// <summary>
    /// No locking file methods.
    /// </summary>
    public class FileNoLock
    {
        /// <summary>
        /// Reads all text of the file without locking it.
        /// </summary>
        /// <param name="filename">The file to read.</param>
        public static string ReadAllText(string filename)
        {
            var fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using (var sr = new StreamReader(fs))
            {
                var result = sr.ReadToEnd();
                return result;
            }
        }
    }
}