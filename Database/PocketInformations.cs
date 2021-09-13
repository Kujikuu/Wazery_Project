using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightConquer_Project.Database
{
   public class PocketInformations
    {
    

       public static void LoadPokerTables()
       {
           string[] baseText = System.IO.File.ReadAllLines(Program.ServerConfig.DbLocation + "PokerTables.txt");
           foreach (var bas_line in baseText)
           {
               var line = bas_line.Split(',');
               Game.MsgServer.Poker.Table table = new Game.MsgServer.Poker.Table();
               table.UID = uint.Parse(line[0]);
               table.Map = ushort.Parse(line[1]);
               table.X = ushort.Parse(line[2]);
               table.Y = ushort.Parse(line[3]);
               table.Mesh = uint.Parse(line[4]);
               table.Noumber = uint.Parse(line[5]);
               table.FixedBet = uint.Parse(line[6]);
               table.Type = (Game.MsgServer.Poker.Table.TableType)uint.Parse(line[7]);
               table.MinBet = uint.Parse(line[8]);
               Database.Server.ServerMaps[table.Map].View.EnterMap<Role.IMapObj>(table);
           }
       }

    }
}
