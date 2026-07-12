// Baystation start
using Content.Shared.Body;
using Content.Shared.Body.Components;
using Content.Shared.Body.Organs;
using Content.Shared.Medical.Wounds;
using Robust.Client.GameObjects;
using Robust.Shared.Timing;

namespace Content.Client.Medical.Wounds;

/// <summary>
///     Hides body sprite layers for limbs that have been cut away.
/// </summary>
public sealed partial class VisualOrganWoundsSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private Robust.Client.Player.IPlayerManager _playerManager = default!;

    private TimeSpan _nextUpdate;
    private static readonly TimeSpan UpdateInterval = TimeSpan.FromSeconds(1);

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_timing.CurTime < _nextUpdate)
            return;

        _nextUpdate = _timing.CurTime + UpdateInterval;

        var player = _playerManager.LocalEntity;
        if (player == null)
            return;

        if (!TryComp<BodyComponent>(player.Value, out var body) || body.Organs == null)
            return;

        var spriteSys = EntityManager.System<SpriteSystem>();

        foreach (var organ in body.Organs.ContainedEntities)
        {
            if (TerminatingOrDeleted(organ))
                continue;

            if (!TryComp<ExternalOrganComponent>(organ, out var ext))
                continue;

            if (!TryComp<VisualOrganComponent>(organ, out var vis))
                continue;

            var layer = vis.Layer;
            if (layer == null)
                continue;

            if ((ext.Status & OrganStatusFlags.CutAway) != 0)
            {
                if (spriteSys.LayerMapTryGet(player.Value, layer, out var baseIdx, false))
                    spriteSys.LayerSetVisible(player.Value, baseIdx, false);
            }
            else
            {
                if (spriteSys.LayerMapTryGet(player.Value, layer, out var baseIdx, false))
                    spriteSys.LayerSetVisible(player.Value, baseIdx, true);
            }
        }
    }
}
// Baystation end
