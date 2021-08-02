using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.BaseGen;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;
using static LocationGeneration.GetOrGenerateMapPatch;

namespace LocationGeneration
{
    [StaticConstructorOnStartup]
    static class HarmonyContainer
    {
        static HarmonyContainer()
        {
            Harmony harmony = new Harmony("LocationGeneration.HarmonyPatches");
            harmony.PatchAll();
        }
    }

    [HarmonyPatch(typeof(SymbolResolver_SinglePawn))]
    [HarmonyPatch("TryFindSpawnCell")]
    public static class Patch_TryFindSpawnCell
    {
        public static void Postfix(ResolveParams rp, out IntVec3 cell)
        {
            Map map = BaseGen.globalSettings.map;
            var result = CellFinder.TryFindRandomCellInsideWith(rp.rect, (IntVec3 x) => x.Standable(map) && (rp.singlePawnSpawnCellExtraPredicate == null
            || rp.singlePawnSpawnCellExtraPredicate(x)), out cell);
        }
    }

    [HarmonyPatch(typeof(SymbolResolver_Settlement), "Resolve")]
    public class VisitSettlementFloat
    {
        public static readonly FloatRange DefaultPawnsPoints = new FloatRange(1150f, 1600f);
        private static bool Prefix(ResolveParams rp)
        {
            Map map = BaseGen.globalSettings.map;
            if (GetOrGenerateMapPatch.customSettlementGeneration)
            {
                Faction faction = rp.faction ?? Find.FactionManager.RandomEnemyFaction();
                SettlementGeneration.DoSettlementGeneration(map, GetOrGenerateMapPatch.locationData.file.FullName, GetOrGenerateMapPatch.locationData.locationDef, faction, false);

                rp.rect = rp.rect.MovedBy(map.Center - rp.rect.CenterCell);
                //foreach (var cell in rp.rect.Cells)
                //{
                //}

                Lord singlePawnLord = rp.singlePawnLord ?? LordMaker.MakeNewLord(faction, new LordJob_DefendBase(faction, rp.rect.CenterCell), map);
                TraverseParms traverseParms = TraverseParms.For(TraverseMode.PassDoors);
                ResolveParams resolveParams = rp;
                resolveParams.rect = rp.rect;
                resolveParams.faction = faction;
                resolveParams.singlePawnLord = singlePawnLord;
                resolveParams.pawnGroupKindDef = (rp.pawnGroupKindDef ?? PawnGroupKindDefOf.Settlement);
                //resolveParams.singlePawnSpawnCellExtraPredicate = (rp.singlePawnSpawnCellExtraPredicate ?? ((Predicate<IntVec3>)((IntVec3 x) => map.reachability.CanReachMapEdge(x, traverseParms))));
                if (resolveParams.pawnGroupMakerParams == null)
                {
                    resolveParams.pawnGroupMakerParams = new PawnGroupMakerParms();
                    resolveParams.pawnGroupMakerParams.tile = map.Tile;
                    resolveParams.pawnGroupMakerParams.faction = faction;
                    resolveParams.pawnGroupMakerParams.points = (rp.settlementPawnGroupPoints ?? DefaultPawnsPoints.RandomInRange);
                    resolveParams.pawnGroupMakerParams.inhabitants = true;
                    resolveParams.pawnGroupMakerParams.seed = rp.settlementPawnGroupSeed;
                }
                BaseGen.symbolStack.Push("pawnGroup", resolveParams);
                return false;
            }
            GetOrGenerateMapPatch.customSettlementGeneration = false;
            return true;
        }
    }
    [HarmonyPatch(typeof(SettlementDefeatUtility))]
    [HarmonyPatch("CheckDefeated")]
    public static class Patch_SettlementDefeatUtility_IsDefeated
    {
        private static bool IsDefeated(Map map, Faction faction)
        {
            List<Pawn> list = map.mapPawns.SpawnedPawnsInFaction(faction);
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].RaceProps.Humanlike)
                {
                    return false;
                }
            }
            return true;
        }

        private static bool Prefix(Settlement factionBase)
        {
            bool result;
            if (factionBase.HasMap)
            {
                if (!IsDefeated(factionBase.Map, factionBase.Faction))
                {
                    result = false;
                }
                else
                {
                    result = true;
                }
            }
            else
            {
                result = true;
            }
            return result;
        }
    }

    [HarmonyPatch(typeof(Log))]
    [HarmonyPatch(nameof(Log.Notify_MessageReceivedThreadedInternal))]
    static class Notify_MessageReceivedThreadedInternal_Patch
    {
        public static bool Prefix()
        {
            return false;
        }
    }
    [HarmonyPatch(typeof(CaravanArrivalAction_VisitSite))]
    [HarmonyPatch("Arrived")]
    public static class CaravanVisitSitePatch
    {
        public static void Prefix(CaravanArrivalAction_VisitSite __instance, Caravan caravan, Site ___site)
        {
            caravanArrival = true;
            Log.Message("GetOrGenerateMapPatch.caravanArrival true");
        }
        public static void Postfix(CaravanArrivalAction_VisitSite __instance, Caravan caravan, Site ___site)
        {
            if (!___site.HasMap)
            {
                LongEventHandler.QueueLongEvent(delegate ()
                {
                    var filePreset = SettlementGeneration.GetPresetFor(___site, out LocationDef locationDef);
                    if (filePreset != null)
                    {
                        customSettlementGeneration = true;
                        locationData = new LocationData { file = filePreset, locationDef = locationDef };
                    }
                    Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(___site.Tile, null);
                    CaravanEnterMapUtility.Enter(caravan, orGenerateMap, CaravanEnterMode.Edge, 0, true, null);

                    if (filePreset != null)
                    {
                        SettlementGeneration.InitialiseLocationGeneration(orGenerateMap, filePreset, locationDef);
                    }
                }, "GeneratingMapForNewEncounter", false, null, true);
            }
        }
    }


    [HarmonyPatch(typeof(CaravanArrivalAction_VisitSettlement))]
    [HarmonyPatch("Arrived")]
    public static class CaravanVisitPatch
    {
        public static void Prefix(CaravanArrivalAction_VisitSettlement __instance, Caravan caravan, Settlement ___settlement)
        {
            caravanArrival = true;
            Log.Message("GetOrGenerateMapPatch.caravanArrival true");
        }
        public static void Postfix(CaravanArrivalAction_VisitSettlement __instance, Caravan caravan, Settlement ___settlement)
        {
            if (!___settlement.HasMap)
            {
                LongEventHandler.QueueLongEvent(delegate ()
                {
                    var filePreset = SettlementGeneration.GetPresetFor(___settlement, out LocationDef locationDef);
                    if (filePreset != null)
                    {
                        customSettlementGeneration = true;
                        locationData = new LocationData { file = filePreset, locationDef = locationDef };
                    }
                    Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(___settlement.Tile, null);
                    CaravanEnterMapUtility.Enter(caravan, orGenerateMap, CaravanEnterMode.Edge, 0, true, null);

                    if (filePreset != null)
                    {
                        SettlementGeneration.InitialiseLocationGeneration(orGenerateMap, filePreset, locationDef);
                    }

                }, "GeneratingMapForNewEncounter", false, null, true);
                return;
            }
            Map orGenerateMap2 = GetOrGenerateMapUtility.GetOrGenerateMap(___settlement.Tile, null);
            CaravanEnterMapUtility.Enter(caravan, orGenerateMap2, CaravanEnterMode.Edge, 0, true, null);
        }
    }

    [HarmonyPatch(typeof(MapGenerator))]
    [HarmonyPatch("GenerateMap")]
    public static class GenerateMapPatch
    {
        public static void Prefix(ref IntVec3 mapSize, MapParent parent, MapGeneratorDef mapGenerator, IEnumerable<GenStepWithParams> extraGenStepDefs = null, Action<Map> extraInitBeforeContentGen = null)
        {
            var worldComp = Find.World.GetComponent<WorldComponentGeneration>();
            if (worldComp.tileSizes.ContainsKey(parent.Tile))
            {
                mapSize = worldComp.tileSizes[parent.Tile];
                worldComp.tileSizes.Remove(parent.Tile);
                Log.Message("Changing map size to " + mapSize);
            }
        }

        public static void Postfix(IntVec3 mapSize, MapParent parent, MapGeneratorDef mapGenerator, IEnumerable<GenStepWithParams> extraGenStepDefs = null, Action<Map> extraInitBeforeContentGen = null)
        {
            Log.Message("GetOrGenerateMapPatch.caravanArrival: " + GetOrGenerateMapPatch.caravanArrival);
            if (!caravanArrival)
            {
                var preset = SettlementGeneration.GetPresetFor(parent, out LocationDef locationDef);
                if (preset != null && locationDef != null)
                {
                    SettlementGeneration.DoSettlementGeneration(parent.Map, preset.FullName, locationDef, parent.Faction, false);
                }
            }
        }
    }
    [HarmonyPatch(typeof(SettlementUtility), "AttackNow")]
    public class GetOrGenerateMapPatch
    {
        public class LocationData
        {
            public LocationDef locationDef;
            public FileInfo file;
        }

        public static bool customSettlementGeneration;
        public static bool caravanArrival;
        public static LocationData locationData;

        public static void Prefix(ref Caravan caravan, ref Settlement settlement)
        {
            var filePreset = SettlementGeneration.GetPresetFor(settlement, out LocationDef locationDef);
            if (filePreset != null)
            {
                locationData = new LocationData { file = filePreset, locationDef = locationDef };
                customSettlementGeneration = true;
            }
        }
        public static void Postfix(ref Caravan caravan, ref Settlement settlement)
        {
            if (customSettlementGeneration)
            {
                customSettlementGeneration = false;
            }
        }
    }
}