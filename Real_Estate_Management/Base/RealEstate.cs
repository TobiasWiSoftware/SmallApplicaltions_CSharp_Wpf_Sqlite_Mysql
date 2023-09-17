using Layer3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.htmltext;
using System.Threading.Tasks;

namespace Real_Estate_Management
{
    public class RealEstate
    {
        public int Id { get; set; }
        public string Adress { get; set; }
        public decimal TotalArea => LFlats.Sum<Flat>(x => x.Area);
        public List<Flat> LFlats { get; set; }

        public RealEstate() 
        { 
        
        }

        public RealEstate(int id, string adress)
        {
            Id = id;
            Adress = adress;
        }

        public override string ToString()
        {
            return $"{Id} {Adress}";
        }

        public static RealEstate Get(int id)
        {
            throw new NotImplementedException();
        }

        public static List<RealEstate> GetAll()
        {
            return DBObject.ReadAll<RealEstate>();
        }

    }
}
