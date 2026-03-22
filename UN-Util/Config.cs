using System;
using System.Collections.Generic;
using System.Text;
using Exiled.API.Interfaces;

namespace UN_Util
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
    }
}


