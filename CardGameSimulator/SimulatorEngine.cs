using System;
using System.Collections.Generic;

public class CardSimulator
   {
   public enum Commander { Jack, Queen, King }
   public enum Result { Draw, Jack, Queen, King }

   private static readonly string[] suits = { "♠", "♥", "♦", "♣" };
   private static readonly Random rng = new Random();
   private readonly int TrialsPerMatchup;

   public CardSimulator(int trials)
      {
      TrialsPerMatchup = trials;
      }

   public void Start()
      {
      var results = new Dictionary<string, Dictionary<Result, int>>();
      var commanders = new[] { Commander.Jack, Commander.Queen, Commander.King };

      foreach (var c1 in commanders)
         {
         foreach (var c2 in commanders)
            {
            string key = $"{c1}_vs_{c2}";
            results[key] = new Dictionary<Result, int>
                {
                    { Result.Draw, 0 },
                    { Result.Jack, 0 },
                    { Result.Queen, 0 },
                    { Result.King, 0 }
                };

            for (int i = 0; i < TrialsPerMatchup; i++)
               results[key][SimulateMatch(c1, c2)]++;
            }
         }

      foreach (var matchup in results)
         {
         Console.WriteLine($"--- {matchup.Key} ---");
         foreach (var entry in matchup.Value)
            Console.WriteLine($"{entry.Key}: {entry.Value} ({(entry.Value / (float)TrialsPerMatchup * 100f):F2}%)");
         }
      }

   Result SimulateMatch(Commander c1, Commander c2)
      {
      var deck = CreateDeck();
      Shuffle(deck);

      var hand1 = DrawCards(deck, 7);
      var hand2 = DrawCards(deck, 7);

      var suit1 = suits[rng.Next(4)];
      var suit2 = suits[rng.Next(4)];

      const int maxRounds = 30;
      int hexUsed1 = 0, hexUsed2 = 0;
      int sabUsed1 = 0, sabUsed2 = 0;
      int strikeUsed1 = 0, strikeUsed2 = 0;

      for (int round = 0; round < maxRounds; round++)
         {
         if (hand1.Count < 3 || hand2.Count < 3)
            break;

         var play1 = PopRandomCards(hand1, 3);
         var play2 = PopRandomCards(hand2, 3);

         bool strikeActive1 = false, strikeActive2 = false;
         HashSet<int> hexBoost1 = new HashSet<int>();
         HashSet<int> hexBoost2 = new HashSet<int>();

         // King declares Commanding Strike
         if (c1 == Commander.King && strikeUsed1 < 3 && round % 5 == 0)
            {
            strikeActive1 = true;
            strikeUsed1++;
            }

         if (c2 == Commander.King && strikeUsed2 < 3 && round % 5 == 0)
            {
            strikeActive2 = true;
            strikeUsed2++;
            }

         // Queen Hex (WITH SACRIFICE)
         if (c1 == Commander.Queen && hexUsed1 < 3 && round % 5 == 0 && hand1.Count > 0 && hand2.Count > 0)
            {
            int lane = rng.Next(3);
            play2[lane] = PopRandomCard(hand2);
            PopRandomCard(hand1); // Sacrifice
            hexBoost2.Add(lane);
            hexUsed1++;
            }

         if (c2 == Commander.Queen && hexUsed2 < 3 && round % 5 == 0 && hand2.Count > 0 && hand1.Count > 0)
            {
            int lane = rng.Next(3);
            play1[lane] = PopRandomCard(hand1);
            PopRandomCard(hand2); // Sacrifice
            hexBoost1.Add(lane);
            hexUsed2++;
            }

         // Jack Sabotage
         if (c1 == Commander.Jack && sabUsed1 < 3 && hand1.Count <= 2)
            {
            SwapAdjacent(play2);
            sabUsed1++;
            }

         if (c2 == Commander.Jack && sabUsed2 < 3 && hand2.Count <= 2)
            {
            SwapAdjacent(play1);
            sabUsed2++;
            }

         // Lane resolution
         for (int i = 0; i < 3; i++)
            {
            var cA = play1[i];
            var cB = play2[i];

            int v1 = ModifyCard(cA, c1, suit1, strikeActive1) + (hexBoost1.Contains(i) ? 1 : 0);
            int v2 = ModifyCard(cB, c2, suit2, strikeActive2) + (hexBoost2.Contains(i) ? 1 : 0);

            int winner = ResolveCombat(cA, cB, v1, v2);

            if (winner == 1)
               {
               if (hand2.Count > 0)
                  TryAttack(cA, hand1, hand2, c1, suit1, suit2);
               else
                  hand1.Add(cA);
               }
            else if (winner == 2)
               {
               if (hand1.Count > 0)
                  TryAttack(cB, hand2, hand1, c2, suit2, suit1);
               else
                  hand2.Add(cB);
               }
            }
         }

      return ResolveWinner(c1, c2, hand1.Count, hand2.Count);
      }

   int ModifyCard((int, string) card, Commander commander, string kingSuit, bool strikeActive)
      {
      if (card.Item1 == 100) return 100;
      if (commander == Commander.King && strikeActive && card.Item2 == kingSuit)
         return card.Item1 + 1;
      return card.Item1;
      }

   void TryAttack((int, string) attacker, List<(int, string)> handSelf, List<(int, string)> handEnemy, Commander commander, string suitSelf, string suitEnemy)
      {
      if (handEnemy.Count == 0) return;
      var target = PopRandomCard(handEnemy);
      int vA = ModifyCard(attacker, commander, suitSelf, false);
      int vB = ModifyCard(target, commander, suitEnemy, false);
      if (ResolveCombat(attacker, target, vA, vB) == 1)
         handSelf.Add(attacker);
      }

   int ResolveCombat((int, string) a, (int, string) b, int vA, int vB)
      {
      if (a.Item1 == 100 && b.Item1 != 100) return 1;
      if (b.Item1 == 100 && a.Item1 != 100) return 2;
      if (vA > vB) return 1;
      if (vB > vA) return 2;
      return 0;
      }

   Result ResolveWinner(Commander c1, Commander c2, int h1, int h2)
      {
      if (h1 == 0 && h2 == 0) return Result.Draw;
      if (h1 == 0) return ConvertToResult(c2);
      if (h2 == 0) return ConvertToResult(c1);
      if (h1 > h2) return ConvertToResult(c1);
      if (h2 > h1) return ConvertToResult(c2);
      return Result.Draw;
      }

   Result ConvertToResult(Commander c) =>
       c == Commander.Jack ? Result.Jack :
       c == Commander.Queen ? Result.Queen : Result.King;

   List<(int, string)> CreateDeck()
      {
      var deck = new List<(int, string)>();
      foreach (var suit in suits)
         for (int i = 1; i <= 13; i++)
            deck.Add((i, suit));
      deck.Add((100, null)); deck.Add((100, null));
      return deck;
      }

   void Shuffle<T>(List<T> list)
      {
      int n = list.Count;
      while (n > 1)
         {
         int k = rng.Next(n--);
         (list[n], list[k]) = (list[k], list[n]);
         }
      }

   List<(int, string)> DrawCards(List<(int, string)> deck, int count)
      {
      var cards = deck.GetRange(0, count);
      deck.RemoveRange(0, count);
      return cards;
      }

   (int, string) PopRandomCard(List<(int, string)> hand)
      {
      int i = rng.Next(hand.Count);
      var card = hand[i];
      hand.RemoveAt(i);
      return card;
      }

   List<(int, string)> PopRandomCards(List<(int, string)> hand, int count)
      {
      var cards = new List<(int, string)>();
      for (int i = 0; i < count; i++)
         cards.Add(PopRandomCard(hand));
      return cards;
      }

   void SwapAdjacent(List<(int, string)> cards)
      {
      int i = rng.Next(2);
      (cards[i], cards[i + 1]) = (cards[i + 1], cards[i]);
      }
   }
