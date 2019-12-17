using System.IO;
using System.Text;

namespace UpDiddyLib.Shared
{
    public class UTF8StringWriter : StringWriter
    {
        public override Encoding Encoding
        {
            get
            {
                return Encoding.UTF8;
            }
        }
    }
}
