using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ConfigEditor.ViewModels
{
    public class RequestConfigPathViewModel
    {
        private const string ConfigFileExt = "config";

        public string ConfigFileExtension
        {
            get
            {
                return ConfigFileExt;
            }
        }

        public bool IsPathValid
        {
            get
            {
                return File.Exists(this.ConfigFilePath);
            }
        }

        public string ConfigFilePath { get; set; }
    }
}
