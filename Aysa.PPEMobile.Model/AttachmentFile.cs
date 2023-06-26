using System;


namespace Aysa.PPEMobile.Model
{
    public class AttachmentFile
    {
        public AttachmentFile()
        {
        }

        public AttachmentFile(string filename)
        {
            FileName = filename;
        }

        public string Id { get; set; }

		public string FileName { get; set; }

        public Boolean Private { get; set; }

        public byte[] BytesArray { get; set; }

        public ArchivoFile ArchivoFile { get; set; }


    }
}
