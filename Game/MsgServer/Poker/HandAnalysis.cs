using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace LightConquer_Project.Game.MsgServer.Poker
{
    /// <summary>
    /// original from http://www.codeproject.com/Articles/12279/Fast-Texas-Holdem-Hand-Evaluation-and-Analysis
    /// </summary>
    public partial class Hand 
    {

        static double JacksOrBetterWinnings(uint handval, int coins)
        {
            switch ((Hand.HandTypes)Hand.HandType(handval))
            {
                case Hand.HandTypes.StraightFlush:
                    if (Hand.CardRank((int)Hand.TopCard(handval)) == Hand.RankAce)
                        if (coins < 5) return 250.0 * coins;
                        else return 4000.0;
                    return 40.0 * coins;
                case Hand.HandTypes.FourOfAKind: return 20.0 * coins;
                case Hand.HandTypes.FullHouse: return 9.0 * coins;
                case Hand.HandTypes.Flush: return 6.0 * coins;
                case Hand.HandTypes.Straight: return 4.0 * coins;
                case Hand.HandTypes.Trips: return 3.0 * coins;
                case Hand.HandTypes.TwoPair: return 2.0 * coins;
                case Hand.HandTypes.Pair:
                    if (Hand.CardRank((int)Hand.TopCard(handval)) >= Hand.RankJack)
                        return 1.0 * coins;
                    break;
            }
            return 0.0;
        }
        public static double ExpectedValue(uint holdmask, ref int[] cards, int bet)
        {
            ulong handmask = 0UL, deadcards = 0UL;
            double winnings = 0.0;
            long count = 0;

            // Create Hold mask and Dead card mask
            for (int i = 0; i < 5; i++)
            {
                if ((holdmask & (1UL << i)) != 0)
                    handmask |= (1UL << cards[i]);
                else
                    deadcards |= (1UL << cards[i]);
            }

            // Iterate through all possible masks
            foreach (ulong mask in Hand.Hands(handmask, deadcards, 5))
            {
                winnings += JacksOrBetterWinnings(Hand.Evaluate(mask, 5), bet);
                count++;
            }

            return (count > 0 ? winnings / count : 0.0);
        }
        public static int CardRank(int card)
        {
#if DEBUG
            // Legal values are 0 - 52.
            if (card < 0 || card > 52)
                throw new ArgumentOutOfRangeException("card");
#endif
            return card % 13;
        }
        public static uint TopCard(System.UInt32 hv)
        {
            return ((hv >> TOP_CARD_SHIFT) & CARD_MASK);
        }
        public static uint HandType(uint handValue)
        {
            return (handValue >> HANDTYPE_SHIFT);
        }
          public static void HandOdds(string[] pockets, string board, string dead, long[] wins, long[] ties, long[] losses, ref long totalHands)
        {
            ulong[] pocketmasks = new ulong[pockets.Length];
            ulong[] pockethands = new ulong[pockets.Length];
            int count = 0, bestcount;
            ulong boardmask = 0UL, deadcards_mask = 0UL, deadcards = Hand.ParseHand(dead, ref count);

            totalHands = 0;
            deadcards_mask |= deadcards;

            // Read pocket cards
            for (int i = 0; i < pockets.Length; i++)
            {
                count = 0;
                pocketmasks[i] = Hand.ParseHand(pockets[i], "", ref count);
                if (count != 2)
                    throw new ArgumentException("There must be two pocket cards."); // Must have 2 cards in each pocket card set.
                deadcards_mask |= pocketmasks[i];
                wins[i] = ties[i] = losses[i] = 0;
            }

            // Read board cards
            count = 0;
            boardmask = Hand.ParseHand("", board, ref count);


#if DEBUG
            Debug.Assert(count >= 0 && count <= 5); // The board must have zero or more cards but no more than a total of 5

            // Check pocket cards, board, and dead cards for duplicates
            if ((boardmask & deadcards) != 0)
                throw new ArgumentException("Duplicate between cards dead cards and board");

            // Validate the input
            for (int i = 0; i < pockets.Length; i++)
            {
                for (int j = i + 1; j < pockets.Length; j++)
                {
                    if ((pocketmasks[i] & pocketmasks[j]) != 0)
                        throw new ArgumentException("Duplicate pocket cards");
                }

                if ((pocketmasks[i] & boardmask) != 0)
                    throw new ArgumentException("Duplicate between cards pocket and board");

                if ((pocketmasks[i] & deadcards) != 0)
                    throw new ArgumentException("Duplicate between cards pocket and dead cards");
            }
#endif

            // Iterate through all board possiblities that doesn't include any pocket cards.
            foreach (ulong boardhand in Hands(boardmask, deadcards_mask, 5))
            {
                // Evaluate all hands and determine the best hand
                ulong bestpocket = Evaluate(pocketmasks[0] | boardhand, 7);
                pockethands[0] = bestpocket;
                bestcount = 1;
                for (int i = 1; i < pockets.Length; i++)
                {
                    pockethands[i] = Evaluate(pocketmasks[i] | boardhand, 7);
                    if (pockethands[i] > bestpocket)
                    {
                        bestpocket = pockethands[i];
                        bestcount = 1;
                    }
                    else if (pockethands[i] == bestpocket)
                    {
                        bestcount++;
                    }
                }

                // Calculate wins/ties/loses for each pocket + board combination.
                for (int i = 0; i < pockets.Length; i++)
                {
                    if (pockethands[i] == bestpocket)
                    {
                        if (bestcount > 1)
                        {
                            ties[i]++;
                        }
                        else
                        {
                            wins[i]++;
                        }
                    }
                    else if (pockethands[i] < bestpocket)
                    {
                        losses[i]++;
                    }
                }

                totalHands++;
            }
        }
    }
}
