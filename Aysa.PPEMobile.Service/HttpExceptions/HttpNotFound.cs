using System;
namespace Aysa.PPEMobile.Service.HttpExceptions
{
    public class HttpNotFound : HttpException
    {
		public HttpNotFound() : base("")
        {
		}

		public HttpNotFound(string message) : base(message)
		{
		}
    }
}
