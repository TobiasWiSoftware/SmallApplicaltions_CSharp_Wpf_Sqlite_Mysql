using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.htmltext;
using System.Threading.Tasks;

namespace Real_Estate_Management_Wpf_MySql
{
    public class BaseModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string pname)
        {
            if(this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(pname));
            }
        }
    }
}
