﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfBasics.ADO.Entities
{
    public class Department
    {
        public Guid    Id   { get; set; }
        public String? Name { get; set; }

        public override string ToString()
        {
            return Id.ToString()[0..3] + "...  " + Name;
        }
    }
}
