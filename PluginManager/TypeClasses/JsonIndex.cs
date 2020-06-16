using System.Collections.Generic;

namespace PluginManager
{
    public class JsonIndex
    {
        public List<Plugin> Packages { get; set; }

        public List<string> AdditionalPackageURLs { get; set; }

        public double Version { get; set; } = 1.0;

        public string MarketplaceName { get; set; } 
    }
}
