using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace LightConquer_Project.Database
{
   public static class DBEffects
    {

       public static Extensions.Counter Counter = new Extensions.Counter(1);

       public static Dictionary<uint, string> Effecte = new Dictionary<uint, string>();


       public static void Load()
       {
#if TEST
           string[] baseText = File.ReadAllLines(Program.ServerConfig.DbLocation + "3DEffect.ini");
           foreach (string aline in baseText)
           {
               if (aline.StartsWith("["))
               {
                   string[] ln = aline.Split('[');

                   Effecte.Add(Counter.Next, ln[1].Split(']')[0].ToString());
               }

           }
#endif
       }
    }
}
