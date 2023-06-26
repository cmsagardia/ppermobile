using System;
namespace Aysa.PPEMobile.Model
{
    public class FilterEventData
    {
		public int EventNumber { get; set; }

		public string Title { get; set; }

		public int Status { get; set; }

		public DateTime FromDate { get; set; }

		public DateTime ToDate { get; set; }
    }
}
