#region

using System;
using System.IO;
using System.Text.RegularExpressions;

#endregion

namespace Common
{
    public class CsvReader : IDisposable
    {
        private readonly StreamReader sr;

        private string[] columns;

        private bool initialized;

        public CsvReader(StreamReader sr, bool hasHeader = true)
        {
            this.sr = sr;

            HasHeader = hasHeader;
        }

        public bool HasHeader { get; set; }

        public string[] Columns
        {
            get
            {
                Initialize();

                return columns;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            sr.Close();
        }

        #endregion

        public string[] ReadRow()
        {
            Initialize();

            string line;
            while (true)
            {
                line = sr.ReadLine();
                if (line == null)
                {
                    return null;
                }

                line = line.Trim();

                if (line != string.Empty && !line.StartsWith("//"))
                {
                    break;
                }
            }

            return RegexTokenizeCsvLine(line);
        }

        private void Initialize()
        {
            if (initialized)
            {
                return;
            }

            if (HasHeader)
            {
                initialized = true;
                columns = TokenizeCsvLine(sr.ReadLine());
            }
        }

        private static string[] RegexTokenizeCsvLine(string line)
        {
            const RegexOptions options =
                    ((RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline) | RegexOptions.IgnoreCase);
            var reg = new Regex("(?:^|,)(\\\"(?:[^\\\"]+|\\\"\\\")*\\\"|[^,]*)", options);
            MatchCollection coll = reg.Matches(line);
            var items = new string[coll.Count];
            int i = 0;
            foreach (Match m in coll)
            {
                items[i++] = m.Groups[0].Value.Trim('"').Trim(',').Trim('"').Replace("\"\"", "\"").Trim();
            }

            return items;
        }

        private static string[] TokenizeCsvLine(string line)
        {
            string[] cells = line.Split(',');

            var result = new string[cells.Length];
            for (int i = 0; i < cells.Length; i++)
            {
                string cell = cells[i].Trim();
                if (cell.StartsWith("\""))
                {
                    result[i] = cell.Replace("\"\"", "\"").Trim('"');
                }
                else
                {
                    result[i] = cell;
                }
            }

            return result;
        }
    }
}