using Newtonsoft.Json;

namespace UpDiddyLib.Domain.Models.G2.CrossChq
{
    public class CrossChqConfigurationParameters
    {
        public int Managers { get; set; }

        public int Employees { get; set; }

        public int Peers { get; set; }

        public int Business { get; set; }

        public int Social { get; set; }
    }
}
