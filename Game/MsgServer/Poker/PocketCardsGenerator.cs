using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightConquer_Project.Game.MsgServer.Poker
{
   public class PocketCardsGenerator
   {
       public enum PokerCardsType : byte
       {
           h = 0,
           s = 1,
           c = 2,
           d = 3
       }
       public string[] Cards = new string[] { "2", "3", "4", "5", "6", "7", "8", "9", "10", "j", "q", "k", "a" };
       private Random Rand;
       public bool[,] cards;

       public PocketCardsGenerator()
       {
           Rand = new Random();
           cards = new bool[13, 4];
       }
       public string GetRandomCard()
       {
           string card = "";
           while (true)
           {
               int index = Rand.Next() % 13;
               int type = Rand.Next(0, 4);
               if (cards[index,type] == false)
               {
                   cards[index, type] = true;
                   card = Cards[index];
                   card += ((PokerCardsType)type).ToString();
                    
                   break;
               }
           }
           return card;
       }
       public void Reset()
       {
           for (int x = 0; x < 13; x++)
           {
               for (int y = 0; y < 4; y++)
                   cards[x, y] = false;
           }
       }


    }
}
