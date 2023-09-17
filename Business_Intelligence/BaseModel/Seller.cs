using System;
using System.Collections.Generic;
using System.Linq;
using System.htmltext;
using System.Threading.Tasks;

namespace Business_Intelligence
{
    class Seller
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Revenue { get; set; }

        public Seller(int id, string name, double r)
        {
            Id = id;
            Name = name;
            Revenue = r;
        }

        public static List<Seller> GetTestSeller()
        {
            return new List<Seller>() { new Seller(0, "Tony Test", 3000.0), new Seller(1, "Mark Markise", 2200), new(2, "Fred Feuerstein", 1000)};
        }
    }
}
