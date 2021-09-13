using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace LightConquer_Project.Database
{
    public class TaskRewards
    {
        public static Dictionary<uint, List<uint>> Rewards = new Dictionary<uint, List<uint>>();
        public static void Load()
        {
            string[] baseText = File.ReadAllLines(Program.ServerConfig.DbLocation + "task_reward_type.ini");
            foreach (var bas_line in baseText)
            {
                string[] line = bas_line.Split(new string[] { "@@" }, StringSplitOptions.RemoveEmptyEntries);
                uint UID = uint.Parse(line[0]);

                Rewards.Add(UID, new List<uint>());
                for (int x = 0; x < 8; x++)
                    Rewards[UID].Add(uint.Parse(line[14 + x]));
            }
        }
    }
}
