using System;
using System.Text;

namespace SizePhotos.ResultWriters;

static class SqlHelper
{
    public static string SqlString(object val)
    {
        if (val == null)
        {
            return "NULL";
        }

        return SqlString(val.ToString());
    }

    public static string SqlString(string val)
    {
        if (string.IsNullOrWhiteSpace(val))
        {
            return "NULL";
        }
        else
        {
            val = val.Replace("'", "''");

            return $"'{val}'";
        }
    }

    public static string SqlNumber(object num)
    {
        if (num == null)
        {
            return "NULL";
        }

        return num.ToString();
    }

    public static string SqlTimestamp(DateTime? dt)
    {
        if (dt == null)
        {
            return "NULL";
        }


        return SqlString(((DateTime)dt).ToString("yyyy-MM-dd HH:mm:sszzz"));
    }

    public static string SqlCreateLookup(string table, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        var val = SqlString(value);

        sb.AppendLine($"    IF NOT EXISTS (SELECT 1 FROM {table} WHERE name = {val}) THEN")
          .AppendLine($"        INSERT INTO {table} (name) VALUES ({val});")
          .AppendLine($"    END IF;");

        return sb.ToString();
    }

    public static string SqlLookupId(string table, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "NULL";
        }

        return $"(SELECT id FROM {table} WHERE name = {SqlString(value)})";
    }
}
