using Content.Shared.Inventory;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Strip.Components
{
    [RegisterComponent, NetworkedComponent]
    public sealed partial class StrippableComponent : Component
    {
        /// <summary>
        /// The strip delay for hands.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite), DataField("handDelay")]
        public float HandStripDelay = 4f;
    }

    [NetSerializable, Serializable]
    public enum StrippingUiKey : byte
    {
        Key,
    }

    [NetSerializable, Serializable]
    public sealed class StrippingSlotButtonPressed : BoundUserInterfaceMessage
    {
        public readonly string Slot;

        public readonly bool IsHand;

        public StrippingSlotButtonPressed(string slot, bool isHand)
        {
            Slot = slot;
            IsHand = isHand;
        }
    }

    [NetSerializable, Serializable]
<<<<<<< HEAD
<<<<<<< HEAD
<<<<<<< HEAD
    public sealed class StrippingEnsnareButtonPressed : BoundUserInterfaceMessage;

    [ByRefEvent]
    public abstract class BaseBeforeStripEvent(TimeSpan initialTime, ThievingStealth stealth = ThievingStealth.Obvious) : EntityEventArgs, IInventoryRelayEvent
    {
        public readonly TimeSpan InitialTime = initialTime;
        public float Multiplier = 1f;
        public TimeSpan Additive = TimeSpan.Zero;
        public ThievingStealth Stealth = stealth;
=======
    public sealed class StrippingEnsnareButtonPressed : BoundUserInterfaceMessage
    {
        public StrippingEnsnareButtonPressed()
        {
        }
    }
>>>>>>> parent of 23059a860d (Reapply "Merge branch 'Simple-Station:master' into Psionic-Power-Refactor")
=======
    public sealed class StrippingEnsnareButtonPressed : BoundUserInterfaceMessage;
>>>>>>> parent of 2f3ee29ec0 (Revert "Merge branch 'Simple-Station:master' into Psionic-Power-Refactor")

    [ByRefEvent]
    public abstract class BaseBeforeStripEvent(TimeSpan initialTime, bool stealth = false) : EntityEventArgs, IInventoryRelayEvent
=======
    public sealed class StrippingEnsnareButtonPressed : BoundUserInterfaceMessage
>>>>>>> parent of 89a6bb3ab5 (Mirror: StrippableSystem doafter overhaul (#205))
    {
        public StrippingEnsnareButtonPressed()
        {
        }
    }

    public abstract class BaseBeforeStripEvent : EntityEventArgs, IInventoryRelayEvent
    {
        public readonly float InitialTime;
        public float Time => MathF.Max(InitialTime * Multiplier + Additive, 0f);
        public float Additive = 0;
        public float Multiplier = 1f;
        public bool Stealth;

        public SlotFlags TargetSlots { get; } = SlotFlags.GLOVES;

        public BaseBeforeStripEvent(float initialTime, bool stealth = false)
        {
            InitialTime = initialTime;
            Stealth = stealth;
        }
    }

    /// <summary>
    /// Used to modify strip times. Raised directed at the user.
    /// </summary>
    /// <remarks>
    /// This is also used by some stripping related interactions, i.e., interactions with items that are currently equipped by another player.
    /// </remarks>
    public sealed class BeforeStripEvent : BaseBeforeStripEvent
    {
        public BeforeStripEvent(float initialTime, bool stealth = false) : base(initialTime, stealth) { }
    }

<<<<<<< HEAD
        public StrippableDoAfterEvent(bool insertOrRemove, bool inventoryOrHand, string slotOrHandName)
        {
            InsertOrRemove = insertOrRemove;
            InventoryOrHand = inventoryOrHand;
            SlotOrHandName = slotOrHandName;
        }

<<<<<<< HEAD
    /// <summary>
    /// Used to modify strip times. Raised directed at the user.
    /// </summary>
    /// <remarks>
    /// This is also used by some stripping related interactions, i.e., interactions with items that are currently equipped by another player.
    /// </remarks>
<<<<<<< HEAD
    [ByRefEvent]
    public sealed class BeforeStripEvent(TimeSpan initialTime, ThievingStealth stealth = ThievingStealth.Obvious) : BaseBeforeStripEvent(initialTime, stealth);

    /// <summary>
    ///     Used to modify strip times. Raised directed at the target.
    /// </summary>
    /// <remarks>
    ///     This is also used by some stripping related interactions, i.e., interactions with items that are currently equipped by another player.
    /// </remarks>
    [ByRefEvent]
    public sealed class BeforeGettingStrippedEvent(TimeSpan initialTime, ThievingStealth stealth = ThievingStealth.Obvious) : BaseBeforeStripEvent(initialTime, stealth);

    /// <summary>
    ///     Organizes the behavior of DoAfters for <see cref="StrippableSystem">.
    /// </summary>
    [Serializable, NetSerializable]
    public sealed partial class StrippableDoAfterEvent : DoAfterEvent
=======
    public sealed class BeforeStripEvent : BaseBeforeStripEvent
>>>>>>> parent of 23059a860d (Reapply "Merge branch 'Simple-Station:master' into Psionic-Power-Refactor")
    {
        public BeforeStripEvent(float initialTime, bool stealth = false) : base(initialTime, stealth) { }
    }

=======
>>>>>>> parent of 89a6bb3ab5 (Mirror: StrippableSystem doafter overhaul (#205))
    /// <summary>
    /// Used to modify strip times. Raised directed at the target.
    /// </summary>
    /// <remarks>
    /// This is also used by some stripping related interactions, i.e., interactions with items that are currently equipped by another player.
    /// </remarks>
    public sealed class BeforeGettingStrippedEvent : BaseBeforeStripEvent
    {
        public BeforeGettingStrippedEvent(float initialTime, bool stealth = false) : base(initialTime, stealth) { }
<<<<<<< HEAD
=======
        public override DoAfterEvent Clone() => this;
>>>>>>> parent of 2f3ee29ec0 (Revert "Merge branch 'Simple-Station:master' into Psionic-Power-Refactor")
=======
>>>>>>> parent of 89a6bb3ab5 (Mirror: StrippableSystem doafter overhaul (#205))
    }
}
