using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHParser.Models
{
    public struct Metadata
    {
        public MetadataType type;
        public string value;
        public int index;

        public Metadata(MetadataType type, string value, int index)
        {
            this.type = type;
            this.value = value;
            this.index = index;
        }
    }
}
