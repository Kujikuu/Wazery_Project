using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightConquer_Project.Game.MsgServer.Poker
{
    public class Card
    {
        public enum CardType : ushort
        {
            Hearts = 0,
            Spades = 1,
            Clubs = 2,
            Diamonds = 3
        }
        public ushort GameCardID
        {
            get
            {
                if (StrCard.Contains("10"))
                {
                    return 8;
                }
                char val = StrCard[0];
                if (val == 'a')
                    return 12;
                else if (val == 'j')
                    return 9;
                else if (val == 'q')
                    return 10;
                else if (val == 'k')
                    return 11;
                else
                    return (ushort)(ushort.Parse(val.ToString()) - 2);
            }
        }
        public CardType GameCardType
        {
            get
            {
                char val;
                if (StrCard.Contains("10"))
                {
                    val = StrCard[2];
                }
                else
                    val = StrCard[1];

                if (val == 'h')
                    return CardType.Hearts;//rosu
                else if (val == 's')
                    return CardType.Spades;//negru
                else if (val == 'c')
                    return CardType.Clubs;//trefla
                return
                    CardType.Diamonds;
            }
        }

        public string StrCard = "";

    }
}
