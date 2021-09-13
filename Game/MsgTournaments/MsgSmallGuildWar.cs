using Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace OdysseyServer_Project.Game.MsgTournaments
{
    public class MsgSmallGuildWar
    {
        public static SafeDictionary<uint, Winner> ScoreList = new SafeDictionary<uint, Winner>();
        public Dictionary<Role.SobNpc.StaticMesh, Role.SobNpc> Furnitures;
        public Winner RoundOwner;
        public static ushort MapID = 10550;
        public ProcesType Proces;
        public class Winner
        {
            public uint GuildID;
            public uint Score;
            public string GuildName;
        }
        public MsgSmallGuildWar()
        {
            Proces = ProcesType.Dead;
            Furnitures = new Dictionary<Role.SobNpc.StaticMesh, Role.SobNpc>();
        }

        public void Load()
        {
            Furnitures.Add(Role.SobNpc.StaticMesh.Pole, Database.Server.ServerMaps[MapID].View.GetMapObject<Role.SobNpc>(Role.MapObjectType.SobNpc, 10550));
        }

      //  public void CheckUP()
     //   {
          //  DateTime Now = DateTime.Now;
          //  if (Proces == ProcesType.Dead && Now.Hour == 22 && (Now.Minute >= 0 && Now.Minute <= 19))//44 mt8yrhash
             //   this.Start();

          //  if (Proces == ProcesType.Alive)
        //    {
              //  if (Now.Hour == 22 && Now.Minute == 25 && Now.Second <= 2)
                //    MsgSchedules.SendSysMesage("SmallGuildWar will ended after 35 Minutes.", MsgServer.MsgMessage.ChatMode.Center);

             //   if (Now.Hour == 22 && Now.Minute == 40 && Now.Second <= 2)
                //    MsgSchedules.SendSysMesage("SmallGuildWar will ended after 20 Minutes.", MsgServer.MsgMessage.ChatMode.Center);

              //  if (Now.Hour == 23 && Now.Minute == 0 && Now.Second > 5)
                 //   this.End();
         //   }
     //   }

        public void Start()
        {
            RoundOwner = null;
            Proces = ProcesType.Alive;
            ScoreList = new SafeDictionary<uint, Winner>();
            MsgSchedules.SendSysMesage("SmallGuildWar war has began", MsgServer.MsgMessage.ChatMode.Center);
            MsgSchedules.SendInvitation("SmallGuildWar has begun! Would you like to join?", "CPs", 329, 251, 1002, 0, 60);
        }
        public void Reset(ServerSockets.Packet stream)
        {
            ScoreList = new SafeDictionary<uint, Winner>();

            foreach (var npc in Furnitures.Values)
                npc.HitPoints = npc.MaxHitPoints;

            var Pole = Furnitures[Role.SobNpc.StaticMesh.Pole];
            var users = Database.Server.GamePoll.Values.Where(u => Role.Core.GetDistance(u.Player.X, u.Player.Y, Pole.X, Pole.Y) <= Role.SobNpc.SeedDistrance).ToArray();
            if (users != null)
            {
                foreach (var user in users)
                {
                    MsgServer.MsgUpdate upd = new MsgServer.MsgUpdate(stream, Pole.UID, 2);
                    stream = upd.Append(stream, MsgServer.MsgUpdate.DataType.Mesh, (long)Pole.Mesh);
                    stream = upd.Append(stream, MsgServer.MsgUpdate.DataType.Hitpoints, Pole.HitPoints);
                    stream = upd.GetArray(stream);
                    user.Send(stream);
                    if ((Role.SobNpc.StaticMesh)Pole.Mesh == Role.SobNpc.StaticMesh.Pole)
                        user.Send(Pole.GetArray(stream, false));
                }
            }
        }
        public void FinishRound(ServerSockets.Packet stream)
        {
            SortScores(true);
            Furnitures[Role.SobNpc.StaticMesh.Pole].Name = RoundOwner.GuildName;
            MsgSchedules.SendSysMesage("The Round Ownered by guild " + RoundOwner.GuildName + ".", MsgServer.MsgMessage.ChatMode.Center);
            Reset(stream);
        }
        public void End()
        {
            if (Proces == ProcesType.Alive)
            {
                Proces = ProcesType.Dead;
                MsgSchedules.SendSysMesage(RoundOwner == null ? "SmallGuildWar has ended and there is no winner" : "Guild " + RoundOwner.GuildName + ", He is the winner of SmallGuildWar.", MsgServer.MsgMessage.ChatMode.Center);
                ScoreList.Clear();
            }
        }
        public void UpdateScore(ServerSockets.Packet stream, uint Score, Role.Instance.Guild Guild)
        {
            if (!ScoreList.ContainsKey(Guild.Info.GuildID))
                ScoreList.Add(Guild.Info.GuildID, new Winner() { GuildName = Guild.GuildName, GuildID = Guild.Info.GuildID, Score = Score });
            else
                ScoreList[Guild.Info.GuildID].Score += Score;

            SortScores();

            if (Furnitures[Role.SobNpc.StaticMesh.Pole].HitPoints < 1)
            {
                FinishRound(stream);
                return;
            }
        }
        private void SortScores(bool getwinner = false)
        {
            if (Proces != ProcesType.Dead)
            {
                var Array = ScoreList.Values.ToArray();
                var DescendingList = Array.OrderByDescending(p => p.Score).ToArray();
                for (int x = 0; x < DescendingList.Length; x++)
                {
                    var element = DescendingList[x];
                    if (x == 0 && getwinner)
                    {
                        RoundOwner = element;
                    }
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        Game.MsgServer.MsgMessage msg = new MsgServer.MsgMessage("No " + (x + 1).ToString() + ". " + element.GuildName + " (" + element.Score + ")"
                           , MsgServer.MsgMessage.MsgColor.yellow, x == 0 ? MsgServer.MsgMessage.ChatMode.FirstRightCorner : MsgServer.MsgMessage.ChatMode.ContinueRightCorner);

                        SendMapPacket(msg.GetArray(rec.GetStream()));
                    }
                    if (x == 4)
                        break;
                }
            }
        }
        public void SendMapPacket(ServerSockets.Packet packet)
        {
            foreach (var client in Database.Server.ServerMaps[MapID].Values)
                client.Send(packet);
        }
    }
}
