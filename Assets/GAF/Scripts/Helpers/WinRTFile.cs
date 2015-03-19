
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
#if NETFX_CORE
using Windows.Storage;
using System.Threading.Tasks;
using Windows.Storage.Streams;
#endif


namespace LegacySystem.IO
{
    public static class WinRTFile
    {
        public static bool Exists(string path)
        {
#if NETFX_CORE
            path = FixPath(path);
            var thread = ExistsAsync(path);
            try
            {
                thread.Wait();

                if (thread.IsCompleted)
                    return thread.Result;
                else
                    return false;
            }
            catch
            {
                return false;
            }
#else
            return System.IO.File.Exists(path);
#endif
        }

#if NETFX_CORE

        private static async Task<bool> ExistsAsync(string path)
        {
            var file = await StorageFile.GetFileFromPathAsync(path);
            return file != null;
        }

        private static string FixPath(string path)
        {
            return path.Replace('/', '\\');
        }
#endif

    }
}
