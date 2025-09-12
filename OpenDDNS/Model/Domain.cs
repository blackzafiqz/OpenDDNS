﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenDDNS.Model
{
    public class Domain
    {
        public required string Name { get; set; }
        public required List<string> SubDomains { get; set; }
    }
}
