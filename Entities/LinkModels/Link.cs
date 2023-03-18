using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.LinkModels
{
    public class Link
    {
        //Link bilgisini nereye vereceksek ona işaret eder.
        public string? HRef { get; set; }
        //Linki tanımlayan ifade;silme,güncelleme vs..
        public string? Rel { get; set; }
        public string? Method { get; set; }

        public Link()
        {
            
        }
        public Link(string? hRef, string? rel, string? method)
        {
            HRef = hRef;
            Rel = rel;
            Method = method;
        }
    }

}
