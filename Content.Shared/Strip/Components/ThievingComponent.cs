using Robust.Shared.GameStates;

namespace Content.Shared.Strip.Components;

/// <summary>
/// Give this to an entity when you want to decrease stripping times
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ThievingComponent : Component
{
    /// <summary>
    /// How much the strip time should be shortened by
    /// </summary>
<<<<<<< HEAD
    [DataField]
    public TimeSpan StripTimeReduction = TimeSpan.FromSeconds(0.5f);
=======
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("stripTimeReduction")]
<<<<<<< HEAD
<<<<<<< HEAD
    public float StripTimeReduction = 0.5f;
>>>>>>> parent of 23059a860d (Reapply "Merge branch 'Simple-Station:master' into Psionic-Power-Refactor")

    /// <summary>
    ///  A multiplier coefficient for strip time
    /// </summary>
    [DataField]
    public float StripTimeMultiplier = 1f;
=======
    public TimeSpan StripTimeReduction = TimeSpan.FromSeconds(0.5f);
>>>>>>> parent of 2f3ee29ec0 (Revert "Merge branch 'Simple-Station:master' into Psionic-Power-Refactor")
=======
    public float StripTimeReduction = 0.5f;
>>>>>>> parent of 89a6bb3ab5 (Mirror: StrippableSystem doafter overhaul (#205))

    /// <summary>
    /// Should it notify the user if they're stripping a pocket?
    /// </summary>
    [DataField]
    public ThievingStealth Stealth = ThievingStealth.Hidden;

    /// <summary>
    ///  Should the user be able to see hidden items? (i.e pockets)
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IgnoreStripHidden;
}
