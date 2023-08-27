using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Layer4;
using MySql.Data.MySqlClient;
using Real_Estate_Management;
using Real_Estate_Management_Wpf_MySql;

namespace Layer3;

public static class DBObject
{

    public static void Init()
    {
        string sql = "";
        DBAccess.OpenDB();

        try
        {
            sql = "Drop DataBase DBRealEstate;";
            DBAccess.ExecuteNonQuery(sql);
        }
        catch (Exception)
        {
        }


        sql = "CREATE DATABASE if NOT exists DBRealEstate;" +
              "USE DBRealEstate;" +
              "CREATE TABLE if NOT exists TRealEstate" +
              "(" +
              "RealEstateId INT NOT NULL AUTO_INCREMENT," +
              "Adress VARCHAR(50)," +
              "PRIMARY KEY(RealEstateId));"
              +
              "CREATE TABLE if NOT exists TFlat" +
              "(" +
              "FlatId INT NOT NULL AUTO_INCREMENT," +
              "RealEstateId INT NOT null," +
              "FlatDesc varchar(50)," +
              "LivingArea DECIMAL(6,2) DEFAULT 0.00," +
              "PRIMARY KEY(FlatId)" +
              ");"
              +
              "CREATE TABLE if NOT EXISTS TCostCenter" +
              "(" +
              "CostCenterId INT NOT NULL AUTO_INCREMENT," +
              "RealEstateId INT," +
              "CostCenterDesc VARCHAR(50)," +
              "Allocation INT," +
              "PRIMARY KEY(CostCenterId)" +
              ");"
              +
              "CREATE TABLE if NOT EXISTS TBooking" +
              "(" +
              "BookingId INT NOT NULL AUTO_INCREMENT," +
              "CostCenterId INT NOT NULL," +
              "FlatId INT," +
              "BookingDate DATE NOT NULL," +
              "Amount DECIMAL(8,2) NOT NULL," +
              "BookingText VARCHAR(50) DEFAULT NULL," +
              "PRIMARY KEY(BookingId)" +
              ");";

        DBAccess.ExecuteNonQuery(sql);

        sql = "INSERT INTO trealestate " +
              "VALUES" +
              "(RealestateId, 'Schlossalle 100, 95676 Wiesau')," +
              "(RealestateId, 'Bahnhofstraße 300, 92637 Weiden');" +
              "" +
              "INSERT INTO tflat " +
              "VALUES" +
              "(FlatId, 1, 'Erdgeschoss', 89)," +
              "(FlatId, 1, '1. Stock', 23.5)," +
              "(FlatId, 2, 'Erdgeschoss', 99.5);" +
              "" +
              "INSERT INTO tcostcenter " +
              "VALUES" +
              "(CostcenterId, 1, 'Müllabführ', 1)," +
              "(CostcenterId, 1, 'Kabelfernsehen', 2)," +
              "(CostcenterId, 1, 'Wasser', 3)," +
              "(CostcenterId, 1, 'Haftpflichtversicherung', 1);";

        DBAccess.ExecuteNonQuery(sql);

        int year = DateTime.Today.Year;

        sql = "INSERT INTO tbooking " +
              "VALUES " +
              $"(BookingId, 1, NULL, '{year}-05-01', 198.24, 'Stadt Wiesau Bauhof')," +
              $"(BookingId, 2, NULL, '{year}-03-01', 389.99, 'Vadafone Kabel AG')," +
              $"(BookingId, 4, 1,    '{year}-05-01', 92.22, 'Stadt Wiesau Stadtwerke GmbH')," +
              $"(BookingId, 4, 2,    '{year}-05-01', 58.88, 'Stadt Wiesau Stadtwerke GmbH')," +
              $"(BookingId, 5, NULL, '{year}-12-31', 179.98, 'Foo Versicherung AG');";

        DBAccess.ExecuteNonQuery(sql);

    }

    public static List<T> ReadAll<T>()
    {
        List<object> list = new();
        string sql = "";
        T? t = Activator.CreateInstance<T>();


        sql = $"Select * from T{t.GetType().Name} ";

        using (MySqlConnection con = DBAccess.OpenDB())
        {
            using (MySqlDataReader r = DBAccess.ExecuteReader(sql))
            {
                while (r.Read())
                {
                    switch (t)
                    {
                        case CoastCenter:
                            list.Add(new CoastCenter(r.GetInt32(0), r.GetString(1), (Allocation)r.GetInt32(2), RealEstate.Get(r.GetInt32(3))));
                            break;
                        case RealEstate:
                            list.Add(new RealEstate(r.GetInt32(0), r.GetString(1)));
                            break;
                    }

                }

            }


        }

        return list.Cast<T>().ToList();

    }
}
