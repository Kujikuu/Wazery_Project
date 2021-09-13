using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OdysseyServer_Project.Client;
using OdysseyServer_Project.Game.MsgServer;
using static OdysseyServer_Project.Role.Flags;

namespace OdysseyServer_Project.Game.MsgEvents
{
    class Vampire_War : Events
    {
        public DateTime _vampire;
        public Vampire_War()
        {
            EventTitle = "Infection War";
            Duration = 3;
            _Duration = 180;
            BaseMap = 700;
            NoDamage = true;
            MagicAllowed = false;
            MeleeAllowed = false;
            FriendlyFire = true;
            AllowedSkills = new List<ushort> { (ushort)1045, (ushort)1046, (ushort)1047 };
            PotionsAllowed = false;
        }

        public override void TeleportPlayersToMap()
        {
            foreach (GameClient client in PlayerList.Values)
            {
                ChangePKMode(client, PKMode.PK);
                ushort x = 0;
                ushort y = 0;
                Map.GetRandCoord(ref x, ref y);
                client.Teleport(x, y, Map.ID, DinamicID, true, true);
                if (client.Player.ContainFlag(MsgServer.MsgUpdate.Flags.Cyclone))
                    client.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.Cyclone);
                if (client.Player.ContainFlag(MsgServer.MsgUpdate.Flags.Fly))
                    client.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.Fly);
                if (client.Player.ContainFlag(MsgServer.MsgUpdate.Flags.Superman))
                    client.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.Superman);
                if (client.Player.ContainFlag(MsgServer.MsgUpdate.Flags.DragonSwing))
                    client.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.DragonSwing);
                client.Player.HitPoints = 500;
            }
            _vampire = DateTime.Now;
        }

        public override void Hit(GameClient Attacker, GameClient Victim)
        {
            if (Stage == EventStage.Fighting)
            {
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    Victim.Player.SendString(stream, MsgStringPacket.StringID.Effect, true, new string[1] { "levin" });
                    Attacker.Player.SendString(stream, MsgStringPacket.StringID.Effect, true, new string[1] { "heal2" });

                    if ((Attacker.Player.HitPoints + 50) > 500)
                        Attacker.Player.HitPoints = 500;
                    else
                        Attacker.Player.HitPoints += 50;
                }
            }
        }

        public override uint GetDamage(GameClient User, GameClient C)
        {
            return 50;
        }

        public override void DisplayScore()
        {
            
        }

        public override void WaitForWinner()
        {
            base.WaitForWinner();

            if (DateTime.Now >= _vampire)
            {
                foreach (GameClient C in PlayerList.Values.ToList())
                {
                    if (C.Player.HitPoints > 25)
                        C.Player.HitPoints -= 25;
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        C.Player.SendString(stream, MsgStringPacket.StringID.Effect, true, new string[1] { "poison" });
                    }
                }
                _vampire = DateTime.Now.AddMilliseconds(5000);
            }
        }

        public override void CharacterChecks(GameClient C)
        {
            base.CharacterChecks(C);
            if (!C.Player.Alive)
                C.EventBase?.RemovePlayer(C);
            else if (C.Player.HitPoints > 500)
                C.Player.HitPoints = 500;
        }

        public override void Kill(GameClient Attacker, GameClient Victim)
        {
            RemovePlayer(Victim);
        }

        public override void Reward(GameClient client)
        {
            base.Reward(client);
            //client.Player.BoundConquerPoints += 50;
            //client.Player.ConquerPoints += 7000;
        }
    }
}
