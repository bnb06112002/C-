using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UkrPochta.Data.Entity
{
   public class ChooseAddr
    {
        private string _region;
        private string _district;
        private string _city;
        private string _street;

        public string ChooseRegion
        {
            get { return _region; }
            set { _region = value; _district = null; _city = null;_street = null; }
        }
        public string ChooseDistrict
        {
            get { return _district; }
            set { _district = value; _city = null; _street = null; }
        }
        public string ChooseCity
        {
            get { return _city; }
            set { _city = value; _street = null; }
        }
        public string ChooseStreet
        {
            get { return _street; }
            set { _street = value; }
        }

        public string ChooseAdrr
        {
            get
            {
                string s="Україна";
                if (!string.IsNullOrEmpty(_region))
                    s = _region+" область, "+s;
                if (!string.IsNullOrEmpty(_district))
                    s =  _district+" район, "+s ;
                if (!string.IsNullOrEmpty(_city))
                    s =   "м. "+_city+ ", " +s;
                if (!string.IsNullOrEmpty(_street))
                    s =   _street+ ", " +s;
                return s;
            }
        }
    }
}
