using System.IO;
using Common;

namespace Game.Util
{
    public static class FileExtension
    {
        private static ILogger logger = LoggerFactory.Current.GetLogger<Engine>();

        public static FileStream OverwriteLockedFile(string path)
        {
            var startTime = SystemClock.Now;
            var hasWarned = false;

            while (true)
            {
                try
                {
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }

                    return File.Open(path, FileMode.Create);
                }
                catch(IOException)
                {
                    if (!hasWarned)
                    {
                        logger.Warn("Another process is holding onto a file we are trying to open, we'll wait until the file is unlocked. File is {0}", path);
                        hasWarned = true;
                    }

                    if (SystemClock.Now.Subtract(startTime).TotalMinutes > 3)
                    {
                        throw;
                    }
                }
            }
        }
    }
}
