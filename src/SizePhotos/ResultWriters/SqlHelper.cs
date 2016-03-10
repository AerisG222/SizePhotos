namespace SizePhotos.ResultWriters
{
    static class SqlHelper
    {
        public static string SqlString(string val)
        {
            if(string.IsNullOrWhiteSpace(val))
            {
                return "NULL";
            }
            else
            {
                return $"'{val.Replace("'", "''")}'";
            }
        }
    }
}
