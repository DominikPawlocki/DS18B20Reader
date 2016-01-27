using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Ds18B20Reader;

namespace TemperatureReader
{
    class Program
    {
        private static void Main(string[] args)
        {
            List<Ds18B20> devices = new List<Ds18B20>();

            using (ComPort com1 = new ComPort("COM1"))
            {
                SearchSlaves ss = new SearchSlaves();
                //should be await usage, later
                bool result = ss.Search(com1).Result;

                foreach (byte[] id in ss.IdList)
                {
                    devices.Add(new Ds18B20(id, com1));
                }

                while (true)
                {
                    foreach (Ds18B20 device in devices)
                    {
                        //should be await usage, later
                        float temper = device.ReadTemperature().Result;
                        Console.WriteLine(device.IdString + " " + temper);
                        Thread.Sleep(200 + device.Id.Sum(b => (int)b));
                    }
                }
            }
        }
    }
}
