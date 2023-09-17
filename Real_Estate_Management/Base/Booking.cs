using Real_Estate_Management_Wpf_MySql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.htmltext;
using System.Threading.Tasks;

namespace Real_Estate_Management
{

    public class Booking
    {
        public int Id { get; set; }
        public string? htmltext { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public CoastCenter CoastCenter { get; set; }
        public Flat? Flat { get; set; } // Is null when Allocation Custom
        
        public Booking()
        {

        }

        public static List<Booking> GetAll(CoastCenter c, DateTime begin, DateTime end, DateTime until)
        {
            throw new NotImplementedException();
        }


    }
}
