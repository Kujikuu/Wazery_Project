using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static LightConquer_Project.Game.MsgServer.MsgMessage;

namespace LightConquer_Project.Database
{
    public class VoteSystem
    {

        public class User
        {
            public uint UID;
            public string IP;
            public DateTime Timer = new DateTime();
            public override string ToString()
            {
                var writer = new DBActions.WriteLine('/');
                writer.Add(UID).Add(IP).Add(Timer.Ticks);
                return writer.Close();
            }
        }

        private static List<User> UsersPoll = new List<User>();


        public static bool TryGetObject(uint UID, string IP, out User obj)
        {
            foreach (var _obj in UsersPoll)
            {
                if (_obj.UID == UID || _obj.IP == IP)
                {
                    obj = _obj;
                    return true;
                }
            }
            obj = null;
            return false;
        }
        public static bool CanClaim(Client.GameClient client)
        {
            if (!client.Player.StartVote)
                return false;
            if (client.Player.StartVote)
                if (Extensions.Time32.Now > client.Player.StartVoteStamp)
                    return true;
            return false;
        }
        public static bool CanVote(Client.GameClient client)
        {
            User _user;
            if (TryGetObject(client.Player.UID, client.Socket.RemoteIp, out _user))
            {
                if (_user.Timer.AddHours(12) < DateTime.Now)
                    return true;
                else
                    return false;
            }
            return true;
        }
        public static void CheckUp(Client.GameClient client)
        {
            if (client.Player.StartVote)
            {
                if (Extensions.Time32.Now > client.Player.StartVoteStamp)
                {
                    User _user;
                    if (TryGetObject(client.Player.UID, client.Socket.RemoteIp, out _user))
                    {
                        _user.Timer = DateTime.Now;
                    }
                    else
                    {
                        _user = new User();
                        _user.UID = client.Player.UID;
                        _user.IP = client.Socket.RemoteIp;
                        _user.Timer = DateTime.Now;
                        UsersPoll.Add(_user);
                    }
                    
                    client.Player.VotePoints += 1;
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        client.Player.RateExp = 15;
                        client.Player.DExpTime = 3600;
                        client.Player.CreateExtraExpPacket(stream);
                    }
                    client.SendSysMesage("[Xtremetop100] "+ client.Player.Name + " -Thank you for your support by voting.", ChatMode.TopLeft);

                    client.Player.StartVote = false;
                }
            }
        }
        public static void Save()
        {
            using (Database.DBActions.Write _wr = new Database.DBActions.Write("Votes.txt"))
            {
                foreach (var _obj in UsersPoll)
                    _wr.Add(_obj.ToString());
                _wr.Execute(DBActions.Mode.Open);
            }
        }
        public static void Load()
        {
            using (Database.DBActions.Read r = new Database.DBActions.Read("Votes.txt"))
            {
                if (r.Reader())
                {
                    int count = r.Count;
                    for (uint x = 0; x < count; x++)
                    {
                        Database.DBActions.ReadLine reader = new DBActions.ReadLine(r.ReadString(""), '/');
                        User user = new User();
                        user.UID = reader.Read((uint)0);
                        user.IP = reader.Read("");
                        user.Timer = DateTime.FromBinary(reader.Read((long)0));
                        UsersPoll.Add(user);
                    }
                }
            }

        }

    }
}
