using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aysa.PPEMobile.Model
{
    public class Feature
    {

        public string Id { get; set; }

        public string Detail { get; set; }

        public DateTime Date { get; set; }

        public User Usuario { get; set; }

        public Section Sector { get; set; }

        public List<AttachmentFile> Archivos { get; set; }

        public Boolean CanEdit { get; set; }

        public Boolean CanDelete { get; set; }

    }
}
