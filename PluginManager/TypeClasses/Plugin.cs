using System.Collections.Generic;

namespace PluginManager
{
    public class Plugin
    {
        public string Name { get; set; }

        public string Image { get; set; }

        public string Description { get; set; }

        public bool Warning { get; set; }

        public string WarningText { get; set; }

        public int TotalFiles { get; set; }

        public bool IsInstalled { get; set; }

        public bool IsUpToDate { get; set; }

        public bool IsNotInstalled
        {
            get
            {
                return !IsInstalled;
            }
        }

        public string Status
        {
            get
            {
                if (IsInstalled)
                {
                    return "Installed";
     
                } else
                {
                    return "Not Installed";
                }
            }
        }

        public List<ResourceFile> DPFiles { get; set; }

        public List<ResourceFile> x86Files { get; set; }

        public List<ResourceFile> x64Files { get; set; }
        public List<ResourceFile> RootFiles { get; set; }

    }

}
