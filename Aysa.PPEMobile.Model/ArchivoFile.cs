using System;
namespace Aysa.PPEMobile.Model
{
    public class ArchivoFile
    {
        public ArchivoFile()
        {
        }

        public Guid Id { get; set; }

        public byte[] File { get; set; }

        public byte[] RowVersion { get; set; }


    }
}
