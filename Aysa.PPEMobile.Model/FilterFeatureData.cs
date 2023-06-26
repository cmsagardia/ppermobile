using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aysa.PPEMobile.Model
{
    public class FilterFeatureData
    {

        public string Username { get; set; }

        public string Detail { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public Section Sector { get; set; }
    }
}
