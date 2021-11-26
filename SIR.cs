using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NapierBankMessage
{
    public class SIR
    {
        public string sortCode { get; set; }
        public string natureOfIncident { get; set; }

        public SIR(string sortCodeIn, string natureOfIncidentIn)
        {
            sortCode = sortCodeIn;
            natureOfIncident = natureOfIncidentIn;
        }
    }

    public class SIRList
    {
        public List<SIR> SIR { get; set; }
    }
}
