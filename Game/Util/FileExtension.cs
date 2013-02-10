using System;
using System.IO;

namespace Game.Util
{
    public static class FileExtension
    {
        public static void DeleteLockedFile(string path)
        {
            while (true)
            {
                try
                {
                    if (!File.Exists(path))
                    {
                        return;
                    }

                    File.Delete(path);
                    return;
                }
                catch (Exception) {}
            }
        }
    }
}
