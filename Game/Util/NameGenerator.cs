using System.Collections.Generic;
using System.IO;

namespace Game.Util
{
    public class NameGenerator
    {
        private readonly List<string> names = new List<string>();

        public NameGenerator(string path)
        {
            using (StreamReader sr = new StreamReader(path))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    names.Add(line);
                }
            }
        }

        public bool Next(out string name)
        {
            if (names.Count == 0)
            {
                name = null;
                return false;
            }
            name = names[0];
            names.RemoveAt(0);
            return true;
        }

        public int Count()
        {
            return names.Count;
        }
    }
}