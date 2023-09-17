using Real_Estate_Management;
using System;
using System.Collections.Generic;
using System.Linq;
using System.htmltext;
using System.Threading.Tasks;

namespace Real_Estate_Management_Wpf_MySql
{
    public enum Allocation
    {
        Area = 1, // Area per Flat
        Count = 2, // Refer to Count of Flats
        Custom = 3 // Differend to every living group
    }
    public class CoastCenter
    {
        public int Id { get; set; }
        public string? Description { get; set; }
        public Allocation Allocation { get; set; }
        public RealEstate RealEstate { get; set; }

        public CoastCenter()
        {

        }

        public CoastCenter(int id, string? description, Allocation allocation, RealEstate realEstate)
        {
            Id = id;
            Description = description;
            Allocation = allocation;
            RealEstate = realEstate;
        }

        public CoastCenter(int id)
        {

        }

        public static List<CoastCenter> GetAll(RealEstate realestate)
        {
            throw new NotImplementedException();
        }

    }
}
