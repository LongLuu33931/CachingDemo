using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eSMSDemo.Models
{
    public class DemoModel
    {
        public int OrderID { get; set; }
        public string OrderDate { get; set; }
        public double Freight { get; set; }
        public string ShipCity { get; set; }
        public string ShipCountry { get; set; }
        public string Caching { get; set; } = "No";
    }
}