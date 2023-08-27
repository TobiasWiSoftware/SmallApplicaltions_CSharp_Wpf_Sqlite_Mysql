using Real_Estate_Management_Wpf_MySql;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Real_Estate_Management
{
    public class ViewModel : BaseModel
    {
        public ObservableCollection<RealEstate>? LRealEstates { get; set; }
        public ObservableCollection<Flat>? LstFlats { get; set; }
        public List<Tuple<string,decimal, string, decimal>>? LGridLine { get; set; } //using als help because value of one row data grid must beg calc. in loops with ifs as the allocation type
        public RealEstate? SelectedRealEstate { get => _realEstate; set { _realEstate = value; OnPropertyChanged(nameof(SelectedRealEstate)); } }
        public Flat? SelectedFlat { get; set; }
        public decimal TotalSum { get; set; }

        private RealEstate? _realEstate;

        public ViewModel()
        {
          
        }
        public void FillGridLines(RealEstate real, Flat flat, int year)
        {

        }

        public void Init()
        {
            LRealEstates = new(RealEstate.GetAll());
            SelectedRealEstate = LRealEstates[0];
            OnPropertyChanged(nameof(LRealEstates));
            //FillGridLines();
        }
       
    }
}
