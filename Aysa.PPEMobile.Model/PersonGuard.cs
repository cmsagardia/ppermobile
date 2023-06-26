using System;
using System.Collections.Generic;

namespace Aysa.PPEMobile.Model
{
    public class PersonGuard
    {
		public string Id { get; set; }

		public string NombreApellido { get; set; }

        public string Legajo { get; set; }

        public string TelefonoAlternativo { get; set; }

        public string TelefonoOficina { get; set; }

        public string Mail { get; set; }

        public string Celular { get; set; }

        public string RPV { get; set; }

        public List<ContactType> ContactTypes { get; set; }

        public List<Section> Sectores { get; set; }

        public void BuildContactTypes()
        {

            ContactTypes = new List<ContactType>();

            if(TelefonoOficina.Length > 0)
            {
                ContactType officeContact = new ContactType(TelefonoOficina, PhoneType.Office);
                ContactTypes.Add(officeContact);
            }

            if (Celular.Length > 0)
			{
                ContactType celularContact = new ContactType(Celular, PhoneType.CellPhone);
                ContactTypes.Add(celularContact);
			}

            if (RPV.Length > 0)
			{
                ContactType rpvContact = new ContactType(RPV, PhoneType.RPV);
                ContactTypes.Add(rpvContact);
			}

            if (TelefonoAlternativo.Length > 0)
			{
                ContactType alternativoContact = new ContactType(TelefonoAlternativo, PhoneType.Alternative);
				ContactTypes.Add(alternativoContact);
			}
        }

    }
}
