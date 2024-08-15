using System.Linq;
using Content.Server.Atmos.Components;
using Content.Server.Atmos.Piping.Components;
using Content.Server.Atmos.Reactions;
using Content.Server.NodeContainer.NodeGroups;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Components;
using Robust.Shared.Map.Components;
using Robust.Shared.Utility;

namespace Content.Server.Atmos.EntitySystems;

public partial class AtmosphereSystem
{
    public GasMixture? GetContainingMixture(Entity<TransformComponent?> ent, bool ignoreExposed = false, bool excite = false)
    {
        if (!Resolve(ent, ref ent.Comp))
            return null;

        return GetContainingMixture(ent, ent.Comp.GridUid, ent.Comp.MapUid, ignoreExposed, excite);
    }

    public GasMixture? GetContainingMixture(
        Entity<TransformComponent?> ent,
        Entity<GridAtmosphereComponent?, GasTileOverlayComponent?>? grid,
        Entity<MapAtmosphereComponent?>? map,
        bool ignoreExposed = false,
        bool excite = false)
    {
        if (!Resolve(ent, ref ent.Comp))
            return null;

        if (!ignoreExposed && !ent.Comp.Anchored)
        {
            // Used for things like disposals/cryo to change which air people are exposed to.
            var ev = new AtmosExposedGetAirEvent((ent, ent.Comp), excite);
            RaiseLocalEvent(ent, ref ev);
            if (ev.Handled)
                return ev.Gas;

            // TODO ATMOS: recursively iterate up through parents
            // This really needs recursive InContainer metadata flag for performance
            // And ideally some fast way to get the innermost airtight container.
        }

        var position = _transformSystem.GetGridTilePositionOrDefault((ent, ent.Comp));
        return GetTileMixture(grid, map, position, excite);
    }

    public bool HasAtmosphere(EntityUid gridUid) => _atmosQuery.HasComponent(gridUid);

    public bool SetSimulatedGrid(EntityUid gridUid, bool simulated)
    {
        var ev = new SetSimulatedGridMethodEvent(gridUid, simulated);
        RaiseLocalEvent(gridUid, ref ev);

        return ev.Handled;
    }

    public bool IsSimulatedGrid(EntityUid gridUid)
    {
        var ev = new IsSimulatedGridMethodEvent(gridUid);
        RaiseLocalEvent(gridUid, ref ev);

        return ev.Simulated;
    }

    public IEnumerable<GasMixture> GetAllMixtures(EntityUid gridUid, bool excite = false)
    {
        var ev = new GetAllMixturesMethodEvent(gridUid, excite);
        RaiseLocalEvent(gridUid, ref ev);

        if(!ev.Handled)
            return Enumerable.Empty<GasMixture>();

        DebugTools.AssertNotNull(ev.Mixtures);
        return ev.Mixtures!;
    }

    public void InvalidateTile(EntityUid gridUid, Vector2i tile)
    {
        var ev = new InvalidateTileMethodEvent(gridUid, tile);
        RaiseLocalEvent(gridUid, ref ev);
    }

    public GasMixture?[]? GetTileMixtures(
        Entity<GridAtmosphereComponent?, GasTileOverlayComponent?>? grid,
        Entity<MapAtmosphereComponent?>? map,
        List<Vector2i> tiles,
        bool excite = false)
    {
        GasMixture?[]? mixtures = null;
        var handled = false;

        // If we've been passed a grid, try to let it handle it.
        if (grid is {} gridEnt && Resolve(gridEnt, ref gridEnt.Comp1))
        {
            if (excite)
                Resolve(gridEnt, ref gridEnt.Comp2);

            handled = true;
            mixtures = new GasMixture?[tiles.Count];

            for (var i = 0; i < tiles.Count; i++)
            {
                var tile = tiles[i];
                if (!gridEnt.Comp1.Tiles.TryGetValue(tile, out var atmosTile))
                {
                    // need to get map atmosphere
                    handled = false;
                    continue;
                }

                mixtures[i] = atmosTile.Air;

                if (excite)
                {
                    AddActiveTile(gridEnt.Comp1, atmosTile);
                    InvalidateVisuals((gridEnt.Owner, gridEnt.Comp2), tile);
                }
            }
        }

        if (handled)
            return mixtures;

        // We either don't have a grid, or the event wasn't handled.
        // Let the map handle it instead, and also broadcast the event.
        if (map is {} mapEnt && _mapAtmosQuery.Resolve(mapEnt, ref mapEnt.Comp))
        {
            mixtures ??= new GasMixture?[tiles.Count];
            for (var i = 0; i < tiles.Count; i++)
            {
                mixtures[i] ??= mapEnt.Comp.Mixture;
            }

            return mixtures;
        }

        // Default to a space mixture... This is a space game, after all!
        mixtures ??= new GasMixture?[tiles.Count];
        for (var i = 0; i < tiles.Count; i++)
        {
            mixtures[i] ??= GasMixture.SpaceGas;
        }
        return mixtures;
    }

    public GasMixture? GetTileMixture (Entity<TransformComponent?> entity, bool excite = false)
    {
        if (!Resolve(entity.Owner, ref entity.Comp))
            return null;

        var indices = _transformSystem.GetGridTilePositionOrDefault(entity);
        return GetTileMixture(entity.Comp.GridUid, entity.Comp.MapUid, indices, excite);
    }

    public GasMixture? GetTileMixture(
        Entity<GridAtmosphereComponent?, GasTileOverlayComponent?>? grid,
        Entity<MapAtmosphereComponent?>? map,
        Vector2i gridTile,
        bool excite = false)
    {
        // If we've been passed a grid, try to let it handle it.
        if (grid is {} gridEnt
            && Resolve(gridEnt, ref gridEnt.Comp1, false)
            && gridEnt.Comp1.Tiles.TryGetValue(gridTile, out var tile))
        {
            if (excite)
            {
                AddActiveTile(gridEnt.Comp1, tile);
                InvalidateVisuals((grid.Value.Owner, grid.Value.Comp2), gridTile);
            }

            return tile.Air;
        }

        if (map is {} mapEnt && _mapAtmosQuery.Resolve(mapEnt, ref mapEnt.Comp, false))
            return mapEnt.Comp.Mixture;

        // Default to a space mixture... This is a space game, after all!
        return GasMixture.SpaceGas;
    }

    public ReactionResult ReactTile(EntityUid gridId, Vector2i tile)
    {
        var ev = new ReactTileMethodEvent(gridId, tile);
        RaiseLocalEvent(gridId, ref ev);

        ev.Handled = true;

        return ev.Result;
    }

    public bool IsTileAirBlocked(EntityUid gridUid, Vector2i tile, AtmosDirection directions = AtmosDirection.All, MapGridComponent? mapGridComp = null)
    {
<<<<<<< HEAD
        if (!Resolve(gridUid, ref mapGridComp, false))
            return false;
=======
        var ev = new IsTileAirBlockedMethodEvent(gridUid, tile, directions, mapGridComp);
        RaiseLocalEvent(gridUid, ref ev);
>>>>>>> parent of 462e91c2cc (aaaaaaaaa)

        // If nothing handled the event, it'll default to true.
        return ev.Result;
    }

    public bool IsTileSpace(Entity<GridAtmosphereComponent?>? grid, Entity<MapAtmosphereComponent?>? map, Vector2i tile)
    {
        if (grid is {} gridEnt && _atmosQuery.Resolve(gridEnt, ref gridEnt.Comp, false)
            && gridEnt.Comp.Tiles.TryGetValue(tile, out var tileAtmos))
        {
            return tileAtmos.Space;
        }

        if (map is {} mapEnt && _mapAtmosQuery.Resolve(mapEnt, ref mapEnt.Comp, false))
            return mapEnt.Comp.Space;

        // If nothing handled the event, it'll default to true.
        // Oh well, this is a space game after all, deal with it!
        return true;
    }

    public bool IsTileMixtureProbablySafe(Entity<GridAtmosphereComponent?>? grid, Entity<MapAtmosphereComponent?> map, Vector2i tile)
    {
        return IsMixtureProbablySafe(GetTileMixture(grid, map, tile));
    }

    public float GetTileHeatCapacity(Entity<GridAtmosphereComponent?>? grid, Entity<MapAtmosphereComponent?> map, Vector2i tile)
    {
        return GetHeatCapacity(GetTileMixture(grid, map, tile) ?? GasMixture.SpaceGas);
    }

    public TileMixtureEnumerator GetAdjacentTileMixtures(Entity<GridAtmosphereComponent?> grid, Vector2i tile, bool includeBlocked = false, bool excite = false)
    {
        if (!_atmosQuery.Resolve(grid, ref grid.Comp, false))
            return TileMixtureEnumerator.Empty;

        return !grid.Comp.Tiles.TryGetValue(tile, out var atmosTile)
            ? TileMixtureEnumerator.Empty
            : new(atmosTile.AdjacentTiles);
    }

<<<<<<< HEAD
    public void HotspotExpose(Entity<GridAtmosphereComponent?> grid, Vector2i tile, float exposedTemperature, float exposedVolume,
=======
    public IEnumerable<GasMixture> GetAdjacentTileMixtures(EntityUid gridUid, Vector2i tile, bool includeBlocked = false, bool excite = false)
    {
        var ev = new GetAdjacentTileMixturesMethodEvent(gridUid, tile, includeBlocked, excite);
        RaiseLocalEvent(gridUid, ref ev);

        return ev.Result ?? Enumerable.Empty<GasMixture>();
    }

    public void UpdateAdjacent(EntityUid gridUid, Vector2i tile, MapGridComponent? mapGridComp = null)
    {
        var ev = new UpdateAdjacentMethodEvent(gridUid, tile, mapGridComp);
        RaiseLocalEvent(gridUid, ref ev);
    }

    public void HotspotExpose(EntityUid gridUid, Vector2i tile, float exposedTemperature, float exposedVolume,
>>>>>>> parent of 462e91c2cc (aaaaaaaaa)
        EntityUid? sparkSourceUid = null, bool soh = false)
    {
        if (!_atmosQuery.Resolve(grid, ref grid.Comp, false))
            return;

        if (grid.Comp.Tiles.TryGetValue(tile, out var atmosTile))
            HotspotExpose(grid.Comp, atmosTile, exposedTemperature, exposedVolume, soh, sparkSourceUid);
    }

    public void HotspotExpose(TileAtmosphere tile, float exposedTemperature, float exposedVolume,
        EntityUid? sparkSourceUid = null, bool soh = false)
    {
        if (!_atmosQuery.TryGetComponent(tile.GridIndex, out var atmos))
            return;

        DebugTools.Assert(atmos.Tiles.TryGetValue(tile.GridIndices, out var tmp) && tmp == tile);
        HotspotExpose(atmos, tile, exposedTemperature, exposedVolume, soh, sparkSourceUid);
    }

    public void HotspotExtinguish(EntityUid gridUid, Vector2i tile)
    {
        var ev = new HotspotExtinguishMethodEvent(gridUid, tile);
        RaiseLocalEvent(gridUid, ref ev);
    }

    public bool IsHotspotActive(EntityUid gridUid, Vector2i tile)
    {
        var ev = new IsHotspotActiveMethodEvent(gridUid, tile);
        RaiseLocalEvent(gridUid, ref ev);

        // If not handled, this will be false. Just like in space!
        return ev.Result;
    }

<<<<<<< HEAD
    public bool AddPipeNet(Entity<GridAtmosphereComponent?> grid, PipeNet pipeNet)
=======
    public void FixTileVacuum(EntityUid gridUid, Vector2i tile)
    {
        var ev = new FixTileVacuumMethodEvent(gridUid, tile);
        RaiseLocalEvent(gridUid, ref ev);
    }

    public void AddPipeNet(EntityUid gridUid, PipeNet pipeNet)
>>>>>>> parent of 462e91c2cc (aaaaaaaaa)
    {
        return _atmosQuery.Resolve(grid, ref grid.Comp, false) && grid.Comp.PipeNets.Add(pipeNet);
    }

    public bool RemovePipeNet(Entity<GridAtmosphereComponent?> grid, PipeNet pipeNet)
    {
        return _atmosQuery.Resolve(grid, ref grid.Comp, false) && grid.Comp.PipeNets.Remove(pipeNet);
    }

    public bool AddAtmosDevice(Entity<GridAtmosphereComponent?> grid, Entity<AtmosDeviceComponent> device)
    {
        DebugTools.Assert(device.Comp.JoinedGrid == null);
        DebugTools.Assert(Transform(device).GridUid == grid);

        if (!_atmosQuery.Resolve(grid, ref grid.Comp, false))
            return false;

        if (!grid.Comp.AtmosDevices.Add(device))
            return false;

        device.Comp.JoinedGrid = grid;
        return true;
    }

    public bool RemoveAtmosDevice(Entity<GridAtmosphereComponent?> grid, Entity<AtmosDeviceComponent> device)
    {
        DebugTools.Assert(device.Comp.JoinedGrid == grid);

        if (!_atmosQuery.Resolve(grid, ref grid.Comp, false))
            return false;

        if (!grid.Comp.AtmosDevices.Remove(device))
            return false;

        device.Comp.JoinedGrid = null;
        return true;
    }

    [ByRefEvent] private record struct SetSimulatedGridMethodEvent
        (EntityUid Grid, bool Simulated, bool Handled = false);

    [ByRefEvent] private record struct IsSimulatedGridMethodEvent
        (EntityUid Grid, bool Simulated = false, bool Handled = false);

    [ByRefEvent] private record struct GetAllMixturesMethodEvent
        (EntityUid Grid, bool Excite = false, IEnumerable<GasMixture>? Mixtures = null, bool Handled = false);

<<<<<<< HEAD
    [ByRefEvent] private record struct ReactTileMethodEvent
        (EntityUid GridId, Vector2i Tile, ReactionResult Result = default, bool Handled = false);

=======
    [ByRefEvent] private record struct InvalidateTileMethodEvent
        (EntityUid Grid, Vector2i Tile, bool Handled = false);

    [ByRefEvent] private record struct GetTileMixturesMethodEvent
        (EntityUid? GridUid, EntityUid? MapUid, List<Vector2i> Tiles, bool Excite = false, GasMixture?[]? Mixtures = null, bool Handled = false);

    [ByRefEvent] private record struct GetTileMixtureMethodEvent
        (EntityUid? GridUid, EntityUid? MapUid, Vector2i Tile, bool Excite = false, GasMixture? Mixture = null, bool Handled = false);

    [ByRefEvent] private record struct ReactTileMethodEvent
        (EntityUid GridId, Vector2i Tile, ReactionResult Result = default, bool Handled = false);

    [ByRefEvent] private record struct IsTileAirBlockedMethodEvent
        (EntityUid Grid, Vector2i Tile, AtmosDirection Direction = AtmosDirection.All, MapGridComponent? MapGridComponent = null, bool Result = false, bool Handled = false)
    {
        /// <summary>
        ///     True if one of the enabled blockers has <see cref="AirtightComponent.NoAirWhenFullyAirBlocked"/>. Note
        ///     that this does not actually check if all directions are blocked.
        /// </summary>
        public bool NoAir = false;
    }

    [ByRefEvent] private record struct IsTileSpaceMethodEvent
        (EntityUid? Grid, EntityUid? Map, Vector2i Tile, MapGridComponent? MapGridComponent = null, bool Result = true, bool Handled = false);

    [ByRefEvent] private record struct GetAdjacentTilesMethodEvent
        (EntityUid Grid, Vector2i Tile, IEnumerable<Vector2i>? Result = null, bool Handled = false);

    [ByRefEvent] private record struct GetAdjacentTileMixturesMethodEvent
        (EntityUid Grid, Vector2i Tile, bool IncludeBlocked, bool Excite,
            IEnumerable<GasMixture>? Result = null, bool Handled = false);

    [ByRefEvent] private record struct UpdateAdjacentMethodEvent
        (EntityUid Grid, Vector2i Tile, MapGridComponent? MapGridComponent = null, bool Handled = false);

    [ByRefEvent] private record struct HotspotExposeMethodEvent
        (EntityUid Grid, EntityUid? SparkSourceUid, Vector2i Tile, float ExposedTemperature, float ExposedVolume, bool soh, bool Handled = false);

>>>>>>> parent of 462e91c2cc (aaaaaaaaa)
    [ByRefEvent] private record struct HotspotExtinguishMethodEvent
        (EntityUid Grid, Vector2i Tile, bool Handled = false);

    [ByRefEvent] private record struct IsHotspotActiveMethodEvent
        (EntityUid Grid, Vector2i Tile, bool Result = false, bool Handled = false);
<<<<<<< HEAD
=======

    [ByRefEvent] private record struct FixTileVacuumMethodEvent
        (EntityUid Grid, Vector2i Tile, bool Handled = false);

    [ByRefEvent] private record struct AddPipeNetMethodEvent
        (EntityUid Grid, PipeNet PipeNet, bool Handled = false);

    [ByRefEvent] private record struct RemovePipeNetMethodEvent
        (EntityUid Grid, PipeNet PipeNet, bool Handled = false);

    [ByRefEvent] private record struct AddAtmosDeviceMethodEvent
        (EntityUid Grid, AtmosDeviceComponent Device, bool Result = false, bool Handled = false);

    [ByRefEvent] private record struct RemoveAtmosDeviceMethodEvent
        (EntityUid Grid, AtmosDeviceComponent Device, bool Result = false, bool Handled = false);
>>>>>>> parent of 462e91c2cc (aaaaaaaaa)
}
