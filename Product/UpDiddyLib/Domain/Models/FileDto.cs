namespace UpDiddyLib.Domain.Models
{
    public class FileDto
    {
        public string FileUrl { get; set; }
        public string FileName { get; set; }
        public string MimeType { get; set; }
        public string Base64EncodedData { get; set; }
        public byte[] ByteArrayData { get; set; }

    }
}