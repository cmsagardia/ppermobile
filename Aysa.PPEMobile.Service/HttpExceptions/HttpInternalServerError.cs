using System;
namespace Aysa.PPEMobile.Service.HttpExceptions
{
    public class HttpInternalServerError : HttpException
    {
		public HttpInternalServerError() : base("")
        {
		}

		public HttpInternalServerError(string message) : base(message)
		{
		}
    }
}
