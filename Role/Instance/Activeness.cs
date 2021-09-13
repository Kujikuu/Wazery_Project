using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LightConquer_Project.Game.MsgServer;

namespace LightConquer_Project.Role.Instance
{
    public class Activeness
    {

        //not add
        //Daily Quest[5]
        //Champion`s Arena[8]
        //Horse Racing [9]
        //Treasure in the Blue[10]
        //1 Daily Quest[17]
        //Champion`s Arena[20]
        //Horse Racing[21]
        //Treasure in the Blue[22]
        //1 Daily Quest[29]
        //Champion`s Arena[32]
        //Horse Racing[33]
        //Treasure in the Blue[34]

        public class Task
        {
            public uint ID;
            public byte Progres;
            public byte Completed;
        }
        public Client.GameClient user;
        public byte ClaimRewards = 0;
        public Extensions.SafeDictionary<uint, Task> Tasks;
        public uint ActivityPoints = 0;


        public unsafe void UpdateActivityPoints(ServerSockets.Packet stream)
        {
          
            user.Send(stream.ActivityRewardsInfoCreate(MsgActivityRewardsInfo.Action.InitializeList, ActivityPoints));
        }

        public Activeness(Client.GameClient client)
        {
            user = client;
            Tasks = new Extensions.SafeDictionary<uint, Task>();
        }

        public void IncreaseTask(uint UID)
        {
            Database.ActivityTask.Task DBTask;
            if (Database.Server.ActivityTasks.TryGetValue(UID, out DBTask))
            {
                if (Tasks.ContainsKey(UID))
                {
                    UpdateTask(UID, DBTask);
                }
                else
                {
                    if (DBTask.CheckOpen(user.Player.Level, user.Player.Reborn))
                    {
                        Tasks.Add(UID, new Task() { ID = UID });
                        UpdateTask(UID, DBTask);
                    }
                }
            }
        }
        private void UpdateTask(uint ID, Database.ActivityTask.Task DBTask)
        {
            var mytask = Tasks[ID];
            if (mytask.Completed == 0)
            {
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();

                    if (mytask.Progres < DBTask.NeedProgress)
                        mytask.Progres++;

                    if (mytask.Progres == DBTask.NeedProgress)
                    {
                        mytask.Completed = 1;
                        ActivityPoints += DBTask.ActiveValue;
                        UpdateActivityPoints(stream);
                    }
                    UpdateSingleTask(mytask, stream);
                }
            }
        }
        public unsafe void UpdateSingleTask(Task task,ServerSockets.Packet stream)
        {
            user.Send(stream.ActivityTasksCreate(MsgActivityTasks.Action.InitializeList, new Task[1] { task }));
        }
        public void CheckTasks(ServerSockets.Packet stream)
        {
            foreach (var task in Database.Server.ActivityTasks.GetValues())
            {
                if (task.CheckOpen(user.Player.Level, user.Player.Reborn))
                {
                    if (!Tasks.ContainsKey(task.ID))
                        Tasks.Add(task.ID, new Task() { ID= task.ID });
                }
            }
            UpdateTasksList(stream);
        }
        public unsafe void UpdateTasksList(ServerSockets.Packet stream)
        {
        
            user.Send(stream.ActivityTasksCreate(MsgActivityTasks.Action.InitializeList, Tasks.GetValues()));
        }
        public unsafe void UpdateClaimRewards(ServerSockets.Packet stream)
        {
            user.Send(stream.ActivityClaimsCreate(ClaimRewards));
        }

        public override string ToString()
        {
            Database.DBActions.WriteLine writer = new Database.DBActions.WriteLine('/');
            writer.Add(ClaimRewards);
            writer.Add(ActivityPoints);
            writer.Add(Tasks.Count);

            var array = Tasks.GetValues();
            for (int x = 0; x < array.Length; x++)
            {
                var task = array[x];
                writer.Add(task.ID).Add(task.Progres).Add(task.Completed);
            }
            return writer.Close();
        }
        public void Load(string Line)
        {
            Database.DBActions.ReadLine reader = new Database.DBActions.ReadLine(Line, '/');
            ClaimRewards = reader.Read((byte)0);
            ActivityPoints = reader.Read((byte)0);
            int counttasks = reader.Read((int)0);
            for (int x = 0; x < counttasks; x++)
            {
                Task task = new Task();
                task.ID = reader.Read((uint)0);
                task.Progres = reader.Read((byte)0);
                task.Completed = reader.Read((byte)0);

                if (!Tasks.ContainsKey(task.ID))
                    Tasks.Add(task.ID, task);
            }
        }
    }
}
