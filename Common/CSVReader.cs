using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RestArea.Common
{
    public static class CSVReader
    {
        public static IEnumerable<IList<string>> FromFile(string fileName)
        {
            foreach (IList<string> item in FromFile(fileName, ignoreFirstLineDefault)) yield return item;
        }

        public static IEnumerable<IList<string>> FromFile(string fileName, bool ignoreFirstLine)
        {
            using (StreamReader rdr = new StreamReader(fileName))
            {
                foreach (IList<string> item in FromReader(rdr, ignoreFirstLine)) yield return item;
            }
        }

        public static IEnumerable<IList<string>> FromStream(Stream csv)
        {
            foreach (IList<string> item in FromStream(csv, ignoreFirstLineDefault)) yield return item;
        }

        public static IEnumerable<IList<string>> FromStream(Stream csv, bool ignoreFirstLine)
        {
            using (var rdr = new StreamReader(csv))
            {
                foreach (IList<string> item in FromReader(rdr, ignoreFirstLine)) yield return item;
            }
        }

        public static IEnumerable<IList<string>> FromReader(TextReader csv)
        {
            foreach (IList<string> item in FromReader(csv, ignoreFirstLineDefault)) yield return item;
        }

        public static IEnumerable<IList<string>> FromReader(TextReader csv, bool ignoreFirstLine)
        {
            if (ignoreFirstLine) csv.ReadLine();

            IList<string> result = new List<string>();

            StringBuilder curValue = new StringBuilder();
            char c;
            c = (char)csv.Read();
            while (csv.Peek() != -1)
            {
                switch (c)
                {
                    case ',':
                        result.Add("");
                        c = (char)csv.Read();
                        break;
                    case '"':
                    case '\'':
                        char q = c;
                        c = (char)csv.Read();
                        bool inQuotes = true;
                        while (inQuotes && csv.Peek() != -1)
                        {
                            if (c == q)
                            {
                                c = (char)csv.Read();
                                if (c != q)
                                    inQuotes = false;
                            }

                            if (inQuotes)
                            {
                                curValue.Append(c);
                                c = (char)csv.Read();
                            }
                        }
                        result.Add(curValue.ToString());
                        curValue = new StringBuilder();
                        if (c == ',') c = (char)csv.Read();
                        break;
                    case '\n':
                    case '\r':
                        if (result.Count > 0)
                        {
                            yield return result;
                            result = new List<string>();
                        }
                        c = (char)csv.Read();
                        break;
                    default:
                        while (c != ',' && c != '\r' && c != '\n' && csv.Peek() != -1)
                        {
                            curValue.Append(c);
                            c = (char)csv.Read();
                        }
                        result.Add(curValue.ToString());
                        curValue = new StringBuilder();
                        if (c == ',') c = (char)csv.Read();
                        break;
                }

            }
            if (curValue.Length > 0)
                result.Add(curValue.ToString());
            if (result.Count > 0)
                yield return result;

        }
        private static bool ignoreFirstLineDefault = false;
    }
}