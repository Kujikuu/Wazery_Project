using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightConquer_Project
{
    public class MapGroupThread
    {
        public const int AI_Buffers = 500,
            AI_Guards = 700,
            AI_Monsters = 400,
            User_Buffers = 500,
            User_Stamina = 500,
            User_StampXPCount = 3000,
            User_AutoAttack = 10,
            User_CheckSeconds = 1000,
            User_FloorSpell = 100,
            User_CheckItems = 1000;

        public Extensions.ThreadGroup.ThreadItem Thread;
        public MapGroupThread(int interval, string name)
        {
            Thread = new Extensions.ThreadGroup.ThreadItem(interval, name, OnProcess);
        }
        public void Start()
        {
            Thread.Open();
        }
        Role.GameMap _desert;
        public Role.GameMap Desert
        {
            get
            {
                if (_desert == null)
                    _desert = Database.Server.ServerMaps[1000];
                return _desert;
            }
        }
        Role.GameMap _bird;
        public Role.GameMap Bird
        {
            get
            {
                if (_bird == null)
                    _bird = Database.Server.ServerMaps[1000];
                return _bird;
            }
        }

        Role.GameMap _towerofmystery;
        public Role.GameMap TowerofMystery
        {
            get
            {
                if (_towerofmystery == null)
                    _towerofmystery = Database.Server.ServerMaps[3998];
                return _towerofmystery;
            }
        }
        public void OnProcess()
        {
            Extensions.Time32 clock = Extensions.Time32.Now;
          
    
            //Desert.CheckUpSoldierReamins(clock);
            //Bird.CheckUpSoldierReamins(clock);
          
             //TowerofMystery.GenerateSectorTraps(50,336, 1417);
             //TowerofMystery.GenerateSectorTraps(59,334, 1417);
             //TowerofMystery.GenerateSectorTraps(32,351, 1417);
             //  TowerofMystery.GenerateSectorTraps(30,346, 1417);
             //  TowerofMystery.GenerateSectorTraps(12, 355, 1417);
             //  TowerofMystery.GenerateSectorTraps(22, 341, 1417);

            foreach (var user in Database.Server.GamePoll.Values)
            {
                if (user.Fake)
                    continue;
                user.Player.View.CheckUpMonsters(clock);

                if (clock > user.BuffersStamp)
                {
                    Client.PoolProcesses.BuffersCallback(user);
                    user.BuffersStamp.Value = clock.Value + User_Buffers;
                }
                if (clock > user.StaminStamp)
                {
                    Client.PoolProcesses.StaminaCallback(user);
                    user.StaminStamp.Value = clock.Value + User_Stamina;
                }
                if (clock > user.XPCountStamp)
                {
                    Client.PoolProcesses.StampXPCountCallback(user);
                    user.XPCountStamp.Value = clock.Value + User_StampXPCount;
                }
                if (clock > user.AttackStamp)
                {
                    Client.PoolProcesses.AutoAttackCallback(user);
                    user.AttackStamp.Value = clock.Value + User_AutoAttack;
                }
                if (clock > user.CheckSecondsStamp)
                {
                    Client.PoolProcesses.CheckSeconds(user);
                    user.CheckSecondsStamp.Value = clock.Value + User_CheckSeconds;
                }
                if (clock > user.FloorSpellStamp)
                {
                    Client.PoolProcesses.FloorSpell(user);
                    user.FloorSpellStamp.Value = clock.Value + User_FloorSpell;
                }
                if (clock > user.FloorSpellStamp)
                {
                    Client.PoolProcesses.FloorSpell(user);
                    user.FloorSpellStamp.Value = clock.Value + User_FloorSpell;
                }
                if (clock > user.CheckItemsView)
                {
                    Client.PoolProcesses.CheckItems(user);
                    user.CheckItemsView.Value = clock.Value + User_CheckItems;
                }
            }
         //   timer.Stop();
       //     if (timer.ElapsedMilliseconds > 0)
           //     Console.WriteLine(timer.ElapsedMilliseconds);
        }
    }
}
