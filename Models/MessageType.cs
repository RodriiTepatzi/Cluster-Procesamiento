using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cluster_Procesamiento.Models
{
    public enum MessageType
    {
        User,
        Processor,
        Data,
        ProcessedData,
        Status,
        Turn,
    }
}
