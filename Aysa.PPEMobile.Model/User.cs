using System;
using System.Collections.Generic;

namespace Aysa.PPEMobile.Model
{
    public class User
    {
		public string Id { get; set; }

		public string UserName { get; set; }

		public string NombreApellido { get; set; }

        public string Mail { get; set; }

        public List<Roles> Roles { get; set; }
    }
}
