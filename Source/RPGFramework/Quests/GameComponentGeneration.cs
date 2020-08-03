using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Quests
{
	public class GameComponentGeneration : GameComponent	
	{
        public GameComponentGeneration()
        {

        }

        public GameComponentGeneration(Game game)
        {

        }

        public override void StartedNewGame()
        {
            base.StartedNewGame();
            hiddenSettlements = new Dictionary<int, Settlement>();
            foreach (var factionDef in DefDatabase<FactionDef>.AllDefs)
            {
                if (factionDef.HasModExtension<SettlementOptionModExtension>())
                {
                    var options = factionDef.GetModExtension<SettlementOptionModExtension>();
                    int tile = 0;
                    int numAttempt = 0;
                    while (tile <= 0 && numAttempt < 100)
                    {
                        try
                        {
                            Predicate<int> predicate = null;
                            if (options.biomeDefnames != null && options.biomeDefnames.Count > 0)
                            {
                                predicate = (int x) => options.biomeDefnames.Contains(Find.WorldGrid[x].biome.defName)
                                    && !Find.WorldObjects.AnyWorldObjectAt(x);
                            }
                            else
                            {
                                predicate = (int x) => !Find.WorldObjects.AnyWorldObjectAt(x);
                            }
                            TileFinder.TryFindPassableTileWithTraversalDistance(Find.AnyPlayerHomeMap.Tile, options.distanceToPlayerColony.min,
                                    options.distanceToPlayerColony.max, out tile, predicate);
                            numAttempt++;
                            Log.Message("numAttempt++ - " + numAttempt, true);
                            options.distanceToPlayerColony.max += 10;
                        }
                        catch
                        {
                            numAttempt++;
                        }
                    }
                    if (tile != 0)
                    {
                        try
                        {
                            Vector2 vector = Find.WorldGrid.LongLatOf(tile);
                            Log.Message("Created hidden settlement at " + Find.WorldGrid[tile]
                                + " - " + vector.y.ToStringLatitude() + " - " + vector.x.ToStringLongitude());
                        }
                        catch
                        {
                            Log.Message("Created hidden settlement at " + Find.WorldGrid[tile] + " - " + tile);
                        }
                        Settlement settlement = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
                        settlement.SetFaction(Find.FactionManager.FirstFactionOfDef(factionDef));
                        settlement.Tile = tile;
                        settlement.Name = SettlementNameGenerator.GenerateSettlementName(settlement, factionDef.settlementNameMaker);
                        hiddenSettlements[tile] = settlement;
                    }
                    else
                    {
                        Log.Error("Cant create hidden settlement, tile: " + tile + " number of attempt: " + numAttempt +
                                " min distance: " + options.distanceToPlayerColony.min + " - max distacne: " + options.distanceToPlayerColony.max);
                    }
                }
            }
        }

        public override void GameComponentTick()
        {
            base.GameComponentTick();
            if (Find.TickManager.TicksGame % 60 == 0)
            {
                foreach (var caravan in Find.WorldObjects.Caravans)
                {
					List<int> keysToRemove = new List<int>();
					foreach (var hidden in hiddenSettlements)
					{
						if (Find.WorldGrid.TraversalDistanceBetween(caravan.Tile, hidden.Key, true, int.MaxValue) < 20)
						{
							Find.WorldObjects.Add(hidden.Value);
							keysToRemove.Add(hidden.Key);
							Find.LetterStack.ReceiveLetter(TranslatorFormattedStringExtensions.Translate("FoundCity" + hidden.Value.Faction.def.defName
                                , hidden.Value.Name), TranslatorFormattedStringExtensions.Translate("FoundCityDesc" + hidden.Value.Faction.def.defName
                                , hidden.Value.Name), LetterDefOf.NeutralEvent,
								hidden.Value, null, null, null, null);
						}
					}
					foreach (var key in keysToRemove)
					{
						hiddenSettlements.Remove(key);
					}
				}
            }
        }

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Collections.Look<int, Settlement>(ref this.hiddenSettlements, "hiddenSettlements", LookMode.Value
				, LookMode.Deep, ref hiddenSettlementsKeys, ref hiddenSettlementsValues);
		}

		public Dictionary<int, Settlement> hiddenSettlements = new Dictionary<int, Settlement>();
		public static List<int> hiddenSettlementsKeys = new List<int>();
		public static List<Settlement> hiddenSettlementsValues = new List<Settlement>();
	}

}

