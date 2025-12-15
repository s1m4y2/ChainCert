using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CertificateIssuer.Models;
public class CertMetadata
{
    public string name { get; set; } = "";
    public string description { get; set; } = "";
    public string documentHash { get; set; } = "";
    public string documentURI { get; set; } = "";
}

