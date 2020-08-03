using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.BaseGen;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;

namespace Quests
{
    public static class SettlementGeneration
    {

        public static bool IsChunk(Thing item)
        {
            if (item?.def?.thingCategories != null)
            {
                foreach (var category in item.def.thingCategories)
                {
                    if (category == ThingCategoryDefOf.Chunks || category == ThingCategoryDefOf.StoneChunks)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static void DoSettlementGeneration(Map map, string path, Faction faction, bool disableFog)
        {
            Log.Message("DoSettlementGeneration");
            if (faction == Faction.OfPlayer || faction == null)
            {
                faction = Faction.OfAncients;
            }

            List<Thing> thingsToDestroy = new List<Thing>();
            HashSet<IntVec3> tilesToProcess = new HashSet<IntVec3>();
            int radiusToClear = 10;

            foreach (var building in map.listerThings.AllThings
                .Where(x => x is Building && x.Faction == faction && !(x is Mineable)))
            {
                foreach (var pos in GenRadial.RadialCellsAround(building.Position, radiusToClear, true))
                {
                    if (GenGrid.InBounds(pos, map))
                    {
                        tilesToProcess.Add(pos);
                    }
                }
                thingsToDestroy.Add(building);
            }

            List<Pawn> pawns = new List<Pawn>();
            List<Building> buildings = new List<Building>();
            List<Thing> things = new List<Thing>();
            List<Plant> plants = new List<Plant>();
            Dictionary<IntVec3, TerrainDef> terrains = new Dictionary<IntVec3, TerrainDef>();
            Dictionary<IntVec3, RoofDef> roofs = new Dictionary<IntVec3, RoofDef>();

            Scribe.loader.InitLoading(path);

            Scribe_Collections.Look<Pawn>(ref pawns, "Pawns", LookMode.Deep, new object[0]);
            Scribe_Collections.Look<Building>(ref buildings, "Buildings", LookMode.Deep, new object[0]);
            Scribe_Collections.Look<Thing>(ref things, "Things", LookMode.Deep, new object[0]);
            Scribe_Collections.Look<Plant>(ref plants, "Plants", LookMode.Deep, new object[0]);

            Scribe_Collections.Look<IntVec3, TerrainDef>(ref terrains, "Terrains",
                LookMode.Value, LookMode.Def, ref terrainKeys, ref terrainValues);
            Scribe_Collections.Look<IntVec3, RoofDef>(ref roofs, "Roofs",
                LookMode.Value, LookMode.Def, ref roofsKeys, ref roofsValues);

            Scribe.loader.FinalizeLoading();

            if (pawns != null && pawns.Count > 0)
            {
                foreach (var pawn in pawns)
                {
                    try
                    {
                        if (GenGrid.InBounds(pawn.Position, map))
                        {
                            GenSpawn.Spawn(pawn, pawn.Position, map, WipeMode.Vanish);
                            pawn.SetFaction(faction);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Error in map generating, cant spawn " + pawn + " - " + ex);
                    }
                }
            }

            if (buildings != null && buildings.Count > 0)
            {
                foreach (var building in buildings)
                {
                    foreach (var pos in GenRadial.RadialCellsAround(building.Position, radiusToClear, true))
                    {
                        if (GenGrid.InBounds(pos, map))
                        {
                            tilesToProcess.Add(pos);
                        }
                    }
                }
                if (tilesToProcess != null && tilesToProcess.Count > 0)
                {
                    foreach (var pos in tilesToProcess)
                    {
                        var things2 = map.thingGrid.ThingsListAt(pos);
                        foreach (var thing in things2)
                        {
                            if (thing is Building || (thing is Plant plant && plant.def != ThingDefOf.Plant_Grass) || IsChunk(thing))
                            {
                                thingsToDestroy.Add(thing);
                            }
                        }
                        var water = pos.GetTerrain(map);
                        if (water.IsWater)
                        {
                            map.terrainGrid.SetTerrain(pos, TerrainDefOf.Soil);
                        }
                        var terrain = pos.GetTerrain(map);
                        if (terrain != null && map.terrainGrid.CanRemoveTopLayerAt(pos))
                        {
                            map.terrainGrid.RemoveTopLayer(pos, false);
                        }
                        var roof = pos.GetRoof(map);
                        if (roof != null && (!map.roofGrid.RoofAt(pos).isNatural || map.roofGrid.RoofAt(pos) == RoofDefOf.RoofRockThin))
                        {
                            map.roofGrid.SetRoof(pos, null);
                        }
                    }
                }

                if (thingsToDestroy != null && thingsToDestroy.Count > 0)
                {
                    for (int i = thingsToDestroy.Count - 1; i >= 0; i--)
                    {
                        try
                        {
                            if (thingsToDestroy[i].Spawned)
                            {
                                thingsToDestroy[i].DeSpawn(DestroyMode.WillReplace);
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error("Cant despawn: " + thingsToDestroy[i] + " - "
                                + thingsToDestroy[i].Position + "error: " + ex);
                        }
                    }
                }

                foreach (var building in buildings)
                {
                    try
                    {
                        if (GenGrid.InBounds(building.Position, map))
                        {
                            GenSpawn.Spawn(building, building.Position, map, building.Rotation, WipeMode.Vanish);
                            if (building.def.CanHaveFaction)
                            {
                                building.SetFaction(faction);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Error in map generating, cant spawn " + building + " - " + ex);
                    }
                }
            }

            if (tilesToProcess != null && tilesToProcess.Count > 0)
            {
                tilesToProcess = tilesToProcess.OrderBy(x => x.x).Skip((int)(tilesToProcess.Count * 25 / 100)).ToHashSet();
                tilesToProcess = tilesToProcess.OrderByDescending(x => x.x).Skip((int)(tilesToProcess.Count * 25 / 100)).ToHashSet();

                tilesToProcess = tilesToProcess.OrderBy(x => x.z).Skip((int)(tilesToProcess.Count * 25 / 100)).ToHashSet();
                tilesToProcess = tilesToProcess.OrderByDescending(x => x.z).Skip((int)(tilesToProcess.Count * 25 / 100)).ToHashSet();
            }

            if (faction.def.HasModExtension<SettlementOptionModExtension>())
            {
                var options = faction.def.GetModExtension<SettlementOptionModExtension>();
                if (options.removeVanillaGeneratedPawns)
                {
                    for (int i = map.mapPawns.PawnsInFaction(faction).Count - 1; i >= 0; i--)
                    {
                        map.mapPawns.PawnsInFaction(faction)[i].DeSpawn(DestroyMode.Vanish);
                    }
                }
                if (options.pawnsToGenerate != null && options.pawnsToGenerate.Count > 0)
                {
                    foreach (var pawn in options.pawnsToGenerate)
                    {
                        foreach (var i in Enumerable.Range(1, (int)pawn.selectionWeight))
                        {
                            var settler = PawnGenerator.GeneratePawn(new PawnGenerationRequest(pawn.kind, faction));
                            var pos = tilesToProcess.Where(x => map.thingGrid.ThingsListAt(x)
                            .Where(y => y is Building).Count() == 0).RandomElement();
                            GenSpawn.Spawn(settler, pos, map);
                        }
                    }
                }
            }

            foreach (var pawn in map.mapPawns.PawnsInFaction(faction))
            {
                Hediff dummyHediff = HediffMaker.MakeHediff(QuestsDefOf.DummyNoStarvingHediff, pawn);
                pawn.health.AddHediff(dummyHediff);
                var lord = pawn.GetLord();
                if (lord != null)
                {
                    map.lordManager.RemoveLord(lord);
                }
                var lordJob = new LordJob_DefendPoint();
                if (tilesToProcess != null && tilesToProcess.Count > 0)
                {
                    lordJob = new LordJob_DefendPoint(tilesToProcess.RandomElement());
                }
                else
                {
                    lordJob = new LordJob_DefendPoint(pawn.Position);
                }
                LordMaker.MakeNewLord(faction, lordJob, map, null).AddPawn(pawn);
            }

            if (plants != null && plants.Count > 0)
            {
                foreach (var plant in plants)
                {
                    if (map.fertilityGrid.FertilityAt(plant.Position) >= plant.def.plant.fertilityMin)
                    {
                        GenSpawn.Spawn(plant, plant.Position, map, WipeMode.Vanish);
                    }
                    //if (map.Biome.AllWildPlants.Contains(plant.def))
                    //{
                    //    GenSpawn.Spawn(plant, plant.Position, map, WipeMode.Vanish);
                    //}
                    //else
                    //{
                    //    map.Biome.AllWildPlants.Where(x)
                    //}
                }
            }
            if (terrains != null && terrains.Count > 0)
            {
                foreach (var terrain in terrains)
                {
                    try
                    {
                        if (GenGrid.InBounds(terrain.Key, map))
                        {
                            map.terrainGrid.SetTerrain(terrain.Key, terrain.Value);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Error in map generating, cant spawn " + terrain.Key + " - " + ex);
                    }
                }
            }

            if (roofs != null && roofs.Count > 0)
            {
                foreach (var roof in roofs)
                {
                    try
                    {
                        if (GenGrid.InBounds(roof.Key, map))
                        {
                            map.roofGrid.SetRoof(roof.Key, roof.Value);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Error in map generating, cant spawn " + roof.Key + " - " + ex);
                    }
                }
            }

            Pawn checker = map.mapPawns.PawnsInFaction(Faction.OfPlayer).FirstOrDefault();
            if (checker != null)
            {
                foreach (var item in map.listerThings.AllThings)
                {
                    if (item.IsForbidden(checker))
                    {
                        var containers = map.listerThings.AllThings.Where(x => x is Building_Storage &&
                        map.thingGrid.ThingsListAt(x.Position).Where(y => y.IsForbidden(checker)).Count() == 0);
                        if (containers != null && containers.Count() > 0)
                        {
                            var container = (Building_Storage)GenClosest.ClosestThing_Global
                                (item.Position, containers, 9999f);
                            if (container != null)
                            {
                                item.Position = container.Position;
                            }
                        }
                    }
                }
            }
            if (disableFog != true)
            {
                try
                {
                    FloodFillerFog.DebugRefogMap(map);
                }
                catch
                {
                    foreach (var cell in map.AllCells)
                    {
                        if (!tilesToProcess.Contains(cell) && !(cell.GetFirstBuilding(map) is Mineable))
                        {
                            var item = cell.GetFirstItem(map);
                            if (item != null)
                            {
                                var room = item.GetRoom();
                                if (room != null)
                                {
                                    if (room.PsychologicallyOutdoors)
                                    {
                                        FloodFillerFog.FloodUnfog(cell, map);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public static List<IntVec3> terrainKeys = new List<IntVec3>();
        public static List<TerrainDef> terrainValues = new List<TerrainDef>();
        public static List<IntVec3> roofsKeys = new List<IntVec3>();
        public static List<RoofDef> roofsValues = new List<RoofDef>();
    }
}

