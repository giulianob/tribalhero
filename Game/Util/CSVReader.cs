#region

using System;
using System.IO;

#endregion

namespace Game.Util
{
    public class CsvReader : IDisposable
    {
        private readonly string[] columns;
        private readonly StreamReader sr;

        public CsvReader(StreamReader sr)
        {
            this.sr = sr;

            columns = TokenizeCSVLine(sr.ReadLine());
        }

        public string[] Columns
        {
            get
            {
                return columns;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (sr != null)
                sr.Close();
        }

        #endregion

        public string[] ReadRow()
        {
            string line;
            while (true)
            {
                line = sr.ReadLine();

                if (line == null)
                    return null;

                line.Trim();

                if (line != string.Empty && !line.StartsWith("//"))
                    break;
            }

            return TokenizeCSVLine(line);
        }

        private string[] TokenizeCSVLine(string line)
        {
            if(line==null) return new string[0];
            string[] cells = line.Split(',');

            var result = new string[cells.Length];
            for (int i = 0; i < cells.Length; i++)
            {
                string cell = cells[i].Trim();
                if (cell.StartsWith("\""))
                    result[i] = cell.Replace("\"\"", "\"").Trim('"');
                else
                    result[i] = cell;
            }

            return result;
        }
    }
}