﻿using System;
using System.Collections.Generic;
using System.Text;

namespace VerdeNFC.Models
{
    public class RoastProfile
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Data { get; set; }

        public bool isManualChoiceAllowed { get; set; }

        public override string ToString()
        {
            return String.Format("{0:D3} - {1}", this.Id, this.Name);
        }

    }
}
