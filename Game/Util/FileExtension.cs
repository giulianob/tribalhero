using System;
using System.IO;

namespace Game.Util
{
    public static class FileExtension
    {
        public static FileStream OverwriteLockedFile(string path)
        {
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
                catch (Exception) {}
            }
        }
    }
}
