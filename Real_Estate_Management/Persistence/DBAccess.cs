using System;
using System.Collections.Generic;
using System.Linq;
using System.htmltext;
using System.Threading.Tasks;
using System.Threading;
using System.Data;
using MySql.Data.MySqlClient;
using System.Diagnostics;

namespace Layer4;

public static class DBAccess
{
    private static MySqlConnection con = new();

    public static MySqlConnection OpenDB()
    {
        int t = 0;

        if (con.State != ConnectionState.Open)
        {

            for (int i = 0; i < 100; i++)
            {
                try
                {
                    con = new(GetConnectionString().ToString());
                    if (con.State != ConnectionState.Open)
                        con.Open();
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(10);
                    t++;
                }

            }

            Trace.WriteLine($"Connectionstate {(con != null ? con.State.ToString() : "null") + " " + t + " attemps"}");
        }


        return con != null ? con : throw new Exception();
    }

    public static MySqlDataReader ExecuteReader(string sql)
    {
        MySqlCommand cmd = new(sql, con);

        return cmd.ExecuteReader();
    }

    public static void ExecuteNonQuery(string sql)
    {
        MySqlCommand cmd = new(sql, con);
        cmd.ExecuteNonQuery();
    }


    public static MySqlConnectionStringBuilder GetConnectionString()
    {
        MySqlConnectionStringBuilder b = new()
        {
            Database = "",
            Password = "",
            Server = "localhost",
            UserID = "root"
        };

        return b;
    }
}
