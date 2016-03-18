namespace SizePhotos.ResultWriters
{
    static class SqlHelper
    {
        public static string SqlString(object val)
        {
            if(val == null)
            {
                return "NULL";
            }
            
            return SqlString(val.ToString());
        }
        
        
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
