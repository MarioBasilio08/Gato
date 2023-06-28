using Gato.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gato.Host
{
    public class Program
    {
        public static void Main(string[] args)
        {

            #region ManagerComplejo
            PlayerMove accountServer = new PlayerMove();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Manager initialize...");
            accountServer.Initialize();
            Console.WriteLine("Press any key for stop..");
            Console.ReadLine();
            accountServer.Uninitialize();
            #endregion
        }
    }
}
