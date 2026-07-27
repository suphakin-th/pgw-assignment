using System.Text;

namespace Reconciliation.Csv;

public static class CsvRdr
{
    public static IEnumerable<string[]> Read(TextReader rdr)
    {
        var fields = new List<string>();
        var sb = new StringBuilder();
        var inQuote = false;
        var pending = false;

        int raw;
        while ((raw = rdr.Read()) != -1)
        {
            var c = (char)raw;

            if (inQuote)
            {
                if (c == '"')
                {
                    if (rdr.Peek() == '"')
                    {
                        rdr.Read();
                        sb.Append('"');
                    }
                    else
                    {
                        inQuote = false;
                    }
                }
                else
                {
                    sb.Append(c);
                }
                continue;
            }

            switch (c)
            {
                case '"':
                    inQuote = true;
                    pending = true;
                    break;
                case ',':
                    fields.Add(sb.ToString());
                    sb.Clear();
                    pending = true;
                    break;
                case '\r':
                    break;
                case '\n':
                    fields.Add(sb.ToString());
                    sb.Clear();
                    yield return fields.ToArray();
                    fields.Clear();
                    pending = false;
                    break;
                default:
                    sb.Append(c);
                    pending = true;
                    break;
            }
        }

        if (pending || sb.Length > 0 || fields.Count > 0)
        {
            fields.Add(sb.ToString());
            yield return fields.ToArray();
        }
    }

    public static string Field(string val)
    {
        if (val.Contains(',') || val.Contains('"') || val.Contains('\n') || val.Contains('\r'))
        {
            return "\"" + val.Replace("\"", "\"\"") + "\"";
        }
        return val;
    }

    public static string Line(IEnumerable<string> vals)
        => string.Join(',', vals.Select(Field));
}
