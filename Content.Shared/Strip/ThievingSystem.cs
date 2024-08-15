using Content.Shared.Inventory;
<<<<<<< HEAD
<<<<<<< HEAD
<<<<<<< HEAD
using Content.Shared.Popups;
=======
>>>>>>> parent of 23059a860d (Reapply "Merge branch 'Simple-Station:master' into Psionic-Power-Refactor")
=======
using Content.Shared.Strip;
>>>>>>> parent of 2f3ee29ec0 (Revert "Merge branch 'Simple-Station:master' into Psionic-Power-Refactor")
=======
>>>>>>> parent of 89a6bb3ab5 (Mirror: StrippableSystem doafter overhaul (#205))
using Content.Shared.Strip.Components;
using Robust.Shared.Serialization;

namespace Content.Shared.Strip;

public sealed class ThievingSystem : EntitySystem
{

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ThievingComponent, BeforeStripEvent>(OnBeforeStrip);
        SubscribeLocalEvent<ThievingComponent, InventoryRelayedEvent<BeforeStripEvent>>((e, c, ev) => OnBeforeStrip(e, c, ev.Args));
    }

    private void OnBeforeStrip(EntityUid uid, ThievingComponent component, BeforeStripEvent args)
    {
        args.Stealth = (ThievingStealth) Math.Max((sbyte) args.Stealth, (sbyte) component.Stealth);
        args.Additive -= component.StripTimeReduction;
        args.Multiplier *= component.StripTimeMultiplier;
    }

    public PopupType? GetPopupTypeFromStealth(ThievingStealth stealth)
    {
        switch (stealth)
        {
            case ThievingStealth.Hidden:
                return null;

            case ThievingStealth.Subtle:
                return PopupType.Small;

            default:
                return PopupType.Large;
        }
    }
}
[Serializable, NetSerializable]
public enum ThievingStealth : sbyte
{
    /// <summary>
    /// 	Target sees a large popup indicating that an item is being stolen by who
    /// </summary>
    Obvious = 0,

    /// <summary>
    /// 	Target sees a small popup indicating that an item is being stolen
    /// </summary>
    Subtle = 1,

    /// <summary>
    /// 	Target does not see any popup regarding the stealing of an item
    /// </summary>
    Hidden = 2
}
