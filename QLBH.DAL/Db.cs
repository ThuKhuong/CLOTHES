using System.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;

namespace QLBH.DAL
{
    public static class Db
    {
    private static string ConnStr
    {
        get
        {
            var cs = ConfigurationManager.ConnectionStrings["CLOTHES"]?.ConnectionString;
            if (string.IsNullOrWhiteSpace(cs))
                throw new InvalidOperationException("Missing connection string 'CLOTHES' in App.config.");
            return cs;
        }
    }

        public static SqlConnection OpenConnection()
        {
            var conn = new SqlConnection(ConnStr);
            conn.Open();
            return conn;
        }

        public static object? Scalar(string sql, params SqlParameter[] parameters)
        {
            using var conn = OpenConnection();
            using var cmd = new SqlCommand(sql, conn);
            if (parameters != null && parameters.Length > 0)
                cmd.Parameters.AddRange(parameters);

            return cmd.ExecuteScalar();
        }

        public static DataTable Query(string sql, params SqlParameter[] parameters)
        {
            using var conn = OpenConnection();
            using var cmd = new SqlCommand(sql, conn);
            if (parameters != null && parameters.Length > 0)
                cmd.Parameters.AddRange(parameters);

            using var da = new SqlDataAdapter(cmd);
            var dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public static int Execute(string sql, params SqlParameter[] parameters)
        {
            using var conn = OpenConnection();
            using var cmd = new SqlCommand(sql, conn);
            if (parameters != null && parameters.Length > 0)
                cmd.Parameters.AddRange(parameters);

            return cmd.ExecuteNonQuery();
        }
    }
}
