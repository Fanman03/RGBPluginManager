﻿using System;


namespace PluginManager
{
    public class Settings
    {
        public string IndexURL { get; set; } = "https://rgbsync.com/api/pluginmanager/index.json";

        public string MainExe { get; set; } = "RGBSync+";

        public string MarketplaceName { get; set; } = "RGB.NET Plugin Manager";
    }
}
