using System;
using System.Collections.Generic;

namespace Aysa.PPEMobile.Droid
{
    public class AysaConstants
    {
        // Define general format for Dates
        public static readonly string FormatDate = "dd/MM/yyyy";
        public static readonly string FormatDateTime = "dd/MM/yyyy h:mm tt";
        public static readonly string FormatDateToSendEvent = "yyyy-mm-dd hh:ss";
        public static readonly string ExceptionGenericMessage = $"No se puede conectar al servidor. Compruebe la conexión y vuelva a intentar";
        public static readonly string ExceptionNetworkMessage = $"No se puede conectar al servidor. Compruebe la conexión y vuelva a intentar";
    }
}
