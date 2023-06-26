using System;
namespace Aysa.PPEMobile.Model
{
	public enum PhoneType
	{
		Office,
		CellPhone,
		RPV,
		Alternative,
	}

    public class ContactType
    {


		public string NumberValue { get; set; }

        public PhoneType PhoneType { get; set; }


        public ContactType(string numberValue, PhoneType phoneType)
        {
            this.PhoneType = phoneType;
            this.NumberValue = numberValue;
        }
    }
}
