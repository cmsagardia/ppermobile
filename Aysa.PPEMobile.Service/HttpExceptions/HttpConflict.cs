using System;
namespace Aysa.PPEMobile.Service.HttpExceptions
{
    public class HttpConflict : HttpException
    {
		public HttpConflict() : base("")
        {
		}

		public HttpConflict(string message) : base(message)
		{
		}
    }
}
