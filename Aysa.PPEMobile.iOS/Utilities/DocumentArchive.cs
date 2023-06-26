using System;
using Foundation;

namespace Aysa.PPEMobile.iOS.Utilities
{
    public class DocumentArchive : NSObject 
    {
        
		public DocumentArchive()
		{
		}

		public string Name { get; set; }

		public string ServerRelativeUrl { get; set; }

		public byte[] BytesArray { get; set; }

		[Export("initWithCoder:")]
		public DocumentArchive(NSCoder coder)
		{
            Name = (NSString)coder.DecodeObject(@"name");
            ServerRelativeUrl = (NSString)coder.DecodeObject(@"serverRelativeUrl");
            BytesArray = (byte[])coder.DecodeBytes(@"bytes");
			
		}

		[Export("encodeWithCoder:")]
		public void EncodeTo(NSCoder coder)
		{
            coder.Encode((NSString)Name, "name");
            coder.Encode((NSString)ServerRelativeUrl, "serverRelativeUrl");
            coder.Encode((byte[])BytesArray, "bytes");
		}
	
    }
}
