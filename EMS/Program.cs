using System;
using System.Threading;
using System.Threading.Tasks;
using AlfenNG9xx;

namespace EMS
{    class Program
    {
        static void Main(string[] args)
        {
            var alfen = new AlfenNG9xx.AlfenNG9xx();
            alfen.Start();
            Thread.Sleep(30000);
            alfen.Stop();
        }        
    }    
}
