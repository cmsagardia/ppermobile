using System;
namespace Aysa.PPEMobile.Service.HttpExceptions
{
    public class NetworkConnectionException : HttpException
    {
		public NetworkConnectionException() : base("")
		{
		}

		public NetworkConnectionException(string message) : base(message)
		{
		}
    }
}
