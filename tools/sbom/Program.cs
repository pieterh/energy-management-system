// See https://aka.ms/new-console-template for more information
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Xml.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

Console.WriteLine("Hello, World!");

using var reader = new FileStream("sbom-backend-ems.json", FileMode.Open);

var dom = await CycloneDX.Json.Serializer.DeserializeAsync(reader);

dom.Components.Sort(
    (x, y) =>
    {
        /*var lic1 = x.Licenses?.First().License.Id ?? "";
        var lic2 = y.Licenses?.First().License.Id ?? "";
        return lic1.CompareTo(lic2);*/
        return x.Publisher.CompareTo(y.Publisher);
    });

// return 

foreach (var item in dom.Components)
{
    Console.WriteLine($"{item.Name} - {item.Version} - {item.Publisher} - {item.Licenses?.First().License.Id}");
}

