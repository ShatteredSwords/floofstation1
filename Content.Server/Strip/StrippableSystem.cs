using Content.Server.Administration.Logs;
using Content.Server.Ensnaring;
using Content.Shared.CombatMode;
using Content.Shared.Cuffs;
using Content.Shared.Cuffs.Components;
using Content.Shared.Database;
using Content.Shared.DoAfter;
using Content.Shared.Ensnaring.Components;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Inventory;
using Content.Shared.Inventory.VirtualItem;
using Content.Shared.Popups;
using Content.Shared.Strip;
using Content.Shared.Strip.Components;
using Content.Shared.Verbs;
using Robust.Server.GameObjects;
using Robust.Shared.Player;
using Robust.Shared.Utility;
using System.Linq;

namespace Content.Server.Strip
{
    public sealed class StrippableSystem : SharedStrippableSystem
    {
        [Dependency] private readonly InventorySystem _inventorySystem = default!;
        [Dependency] private readonly EnsnareableSystem _ensnaringSystem = default!;
        [Dependency] private readonly UserInterfaceSystem _userInterfaceSystem = default!;

        [Dependency] private readonly SharedCuffableSystem _cuffableSystem = default!;
        [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
        [Dependency] private readonly SharedHandsSystem _handsSystem = default!;
        [Dependency] private readonly SharedPopupSystem _popupSystem = default!;

        [Dependency] private readonly IAdminLogManager _adminLogger = default!;
        [Dependency] private readonly ThievingSystem _thieving = default!;

        // TODO: ECS popups. Not all of these have ECS equivalents yet.

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<StrippableComponent, GetVerbsEvent<Verb>>(AddStripVerb);
            SubscribeLocalEvent<StrippableComponent, GetVerbsEvent<ExamineVerb>>(AddStripExamineVerb);
            SubscribeLocalEvent<StrippableComponent, ActivateInWorldEvent>(OnActivateInWorld);

            // BUI
            SubscribeLocalEvent<StrippableComponent, StrippingSlotButtonPressed>(OnStripButtonPressed);
            SubscribeLocalEvent<EnsnareableComponent, StrippingEnsnareButtonPressed>(OnStripEnsnareMessage);

            // DoAfters
            SubscribeLocalEvent<HandsComponent, DoAfterAttemptEvent<StrippableDoAfterEvent>>(OnStrippableDoAfterRunning);
            SubscribeLocalEvent<HandsComponent, StrippableDoAfterEvent>(OnStrippableDoAfterFinished);
        }

        private void AddStripVerb(EntityUid uid, StrippableComponent component, GetVerbsEvent<Verb> args)
        {
            if (args.Hands == null || !args.CanAccess || !args.CanInteract || args.Target == args.User)
                return;

            if (!HasComp<ActorComponent>(args.User))
                return;

            Verb verb = new()
            {
                Text = Loc.GetString("strip-verb-get-data-text"),
                Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/outfit.svg.192dpi.png")),
                Act = () => StartOpeningStripper(args.User, (uid, component), true),
            };

            args.Verbs.Add(verb);
        }

        private void AddStripExamineVerb(EntityUid uid, StrippableComponent component, GetVerbsEvent<ExamineVerb> args)
        {
            if (args.Hands == null || !args.CanAccess || !args.CanInteract || args.Target == args.User)
                return;

            if (!HasComp<ActorComponent>(args.User))
                return;

            ExamineVerb verb = new()
            {
                Text = Loc.GetString("strip-verb-get-data-text"),
                Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/outfit.svg.192dpi.png")),
                Act = () => StartOpeningStripper(args.User, (uid, component), true),
                Category = VerbCategory.Examine,
            };

            args.Verbs.Add(verb);
        }

        private void OnActivateInWorld(EntityUid uid, StrippableComponent component, ActivateInWorldEvent args)
        {
            if (args.Target == args.User)
                return;

            if (!HasComp<ActorComponent>(args.User))
                return;

            StartOpeningStripper(args.User, (uid, component));
        }

<<<<<<< HEAD
<<<<<<< HEAD
=======
>>>>>>> parent of 2f3ee29ec0 (Revert "Merge branch 'Simple-Station:master' into Psionic-Power-Refactor")
        public override void StartOpeningStripper(EntityUid user, Entity<StrippableComponent> strippable, bool openInCombat = false)
        {
            base.StartOpeningStripper(user, strippable, openInCombat);

            if (TryComp<CombatModeComponent>(user, out var mode) && mode.IsInCombatMode && !openInCombat)
                return;

<<<<<<< HEAD
            if (TryComp<ActorComponent>(user, out var actor) && HasComp<StrippingComponent>(user))
=======
            if (TryComp<ActorComponent>(user, out var actor))
>>>>>>> parent of 2f3ee29ec0 (Revert "Merge branch 'Simple-Station:master' into Psionic-Power-Refactor")
            {
                if (_userInterfaceSystem.SessionHasOpenUi(strippable, StrippingUiKey.Key, actor.PlayerSession))
                    return;
                _userInterfaceSystem.TryOpen(strippable, StrippingUiKey.Key, actor.PlayerSession);
            }
        }

        private void OnStripButtonPressed(Entity<StrippableComponent> strippable, ref StrippingSlotButtonPressed args)
        {
            if (args.Session.AttachedEntity is not { Valid: true } user ||
                !TryComp<HandsComponent>(user, out var userHands) ||
                !TryComp<HandsComponent>(strippable.Owner, out var targetHands))
                return;

            if (args.IsHand)
            {
                StripHand((user, userHands), (strippable.Owner, targetHands), args.Slot, strippable);
                return;
            }

            if (!TryComp<InventoryComponent>(strippable, out var inventory))
                return;

            var hasEnt = _inventorySystem.TryGetSlotEntity(strippable, args.Slot, out var held, inventory);

            if (userHands.ActiveHandEntity != null && !hasEnt)
                StartStripInsertInventory((user, userHands), strippable.Owner, userHands.ActiveHandEntity.Value, args.Slot);
            else if (userHands.ActiveHandEntity == null && hasEnt)
                StartStripRemoveInventory(user, strippable.Owner, held!.Value, args.Slot);
        }

        private void StripHand(
            Entity<HandsComponent?> user,
            Entity<HandsComponent?> target,
            string handId,
            StrippableComponent? targetStrippable)
        {
            if (!Resolve(user, ref user.Comp) ||
                !Resolve(target, ref target.Comp) ||
                !Resolve(target, ref targetStrippable))
                return;

            if (!_handsSystem.TryGetHand(target.Owner, handId, out var handSlot))
                return;

            // Is the target a handcuff?
            if (TryComp<VirtualItemComponent>(handSlot.HeldEntity, out var virtualItem) &&
                TryComp<CuffableComponent>(target.Owner, out var cuffable) &&
                _cuffableSystem.GetAllCuffs(cuffable).Contains(virtualItem.BlockingEntity))
            {
                _cuffableSystem.TryUncuff(target.Owner, user, virtualItem.BlockingEntity, cuffable);
                return;
            }

            if (user.Comp.ActiveHandEntity != null && handSlot.HeldEntity == null)
                StartStripInsertHand(user, target, user.Comp.ActiveHandEntity.Value, handId, targetStrippable);
            else if (user.Comp.ActiveHandEntity == null && handSlot.HeldEntity != null)
                StartStripRemoveHand(user, target, handSlot.HeldEntity.Value, handId, targetStrippable);
        }

        private void OnStripEnsnareMessage(EntityUid uid, EnsnareableComponent component, StrippingEnsnareButtonPressed args)
        {
            if (args.Session.AttachedEntity is not { Valid: true } user)
                return;

            foreach (var entity in component.Container.ContainedEntities)
            {
                if (!TryComp<EnsnaringComponent>(entity, out var ensnaring))
                    continue;

                _ensnaringSystem.TryFree(uid, user, entity, ensnaring);
                return;
            }
        }

<<<<<<< HEAD
=======
>>>>>>> parent of 23059a860d (Reapply "Merge branch 'Simple-Station:master' into Psionic-Power-Refactor")
=======
>>>>>>> parent of 2f3ee29ec0 (Revert "Merge branch 'Simple-Station:master' into Psionic-Power-Refactor")
        /// <summary>
        ///     Checks whether the item is in a user's active hand and whether it can be inserted into the inventory slot.
        /// </summary>
        private bool CanStripInsertInventory(
            Entity<HandsComponent?> user,
            EntityUid target,
            EntityUid held,
            string slot)
        {
            if (!Resolve(user, ref user.Comp))
                return false;

            if (user.Comp.ActiveHand == null)
                return false;

            if (user.Comp.ActiveHandEntity == null)
                return false;

            if (user.Comp.ActiveHandEntity != held)
                return false;

            if (!_handsSystem.CanDropHeld(user, user.Comp.ActiveHand))
            {
                _popupSystem.PopupCursor(Loc.GetString("strippable-component-cannot-drop"), user);
                return false;
            }

            if (_inventorySystem.TryGetSlotEntity(target, slot, out _))
            {
                _popupSystem.PopupCursor(Loc.GetString("strippable-component-item-slot-occupied", ("owner", target)), user);
                return false;
            }

            if (!_inventorySystem.CanEquip(user, target, held, slot, out _))
            {
                _popupSystem.PopupCursor(Loc.GetString("strippable-component-cannot-equip-message", ("owner", target)), user);
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Begins a DoAfter to insert the item in the user's active hand into the inventory slot.
        /// </summary>
        private void StartStripInsertInventory(
            Entity<HandsComponent?> user,
            EntityUid target,
            EntityUid held,
            string slot)
        {
            if (!Resolve(user, ref user.Comp))
                return;

            if (!CanStripInsertInventory(user, target, held, slot))
                return;

            if (!_inventorySystem.TryGetSlot(target, slot, out var slotDef))
            {
                Log.Error($"{ToPrettyString(user)} attempted to place an item in a non-existent inventory slot ({slot}) on {ToPrettyString(target)}");
                return;
            }

            var (time, stealth) = GetStripTimeModifiers(user, target, slotDef.StripTime);

<<<<<<< HEAD
<<<<<<< HEAD
            bool hidden = stealth == ThievingStealth.Hidden;

            if (!hidden)
                StripPopup("strippable-component-alert-owner-insert", stealth, target, user: Identity.Entity(user, EntityManager), item: user.Comp.ActiveHandEntity!.Value);

            var prefix = hidden ? "stealthily " : "";
=======
            var doAfterArgs = new DoAfterArgs(EntityManager, user, ev.Time, new AwaitedDoAfterEvent(), null, target: target, used: held)
            {
                ExtraCheck = Check,
                Hidden = ev.Stealth,
                AttemptFrequency = AttemptFrequency.EveryTick,
                BreakOnDamage = true,
                BreakOnTargetMove = true,
                BreakOnUserMove = true,
                NeedHand = true,
                DuplicateCondition = DuplicateConditions.SameTool // Block any other DoAfters featuring this same entity.
            };

            if (!ev.Stealth && Check() && userHands.ActiveHandEntity != null)
            {
                var message = Loc.GetString("strippable-component-alert-owner-insert",
                    ("user", Identity.Entity(user, EntityManager)), ("item", userHands.ActiveHandEntity));
                _popup.PopupEntity(message, target, target, PopupType.Large);
            }

            var prefix = ev.Stealth ? "stealthily " : "";
>>>>>>> parent of 23059a860d (Reapply "Merge branch 'Simple-Station:master' into Psionic-Power-Refactor")
=======
            if (!stealth)
                _popupSystem.PopupEntity(Loc.GetString("strippable-component-alert-owner-insert", ("user", Identity.Entity(user, EntityManager)), ("item", user.Comp.ActiveHandEntity!.Value)), target, target, PopupType.Large);

            var prefix = stealth ? "stealthily " : "";
>>>>>>> parent of 2f3ee29ec0 (Revert "Merge branch 'Simple-Station:master' into Psionic-Power-Refactor")
            _adminLogger.Add(LogType.Stripping, LogImpact.Low, $"{ToPrettyString(user):actor} is trying to {prefix}place the item {ToPrettyString(held):item} in {ToPrettyString(target):target}'s {slot} slot");

            var doAfterArgs = new DoAfterArgs(EntityManager, user, time, new StrippableDoAfterEvent(true, true, slot), user, target, held)
            {
<<<<<<< HEAD
<<<<<<< HEAD
                Hidden = hidden,
=======
                _inventorySystem.TryEquip(user, target, held, slot);

                _adminLogger.Add(LogType.Stripping, LogImpact.Medium, $"{ToPrettyString(user):actor} has placed the item {ToPrettyString(held):item} in {ToPrettyString(target):target}'s {slot} slot");
            }
        }

        /// <summary>
        ///     Places item in user's active hand in one of the entity's hands.
        /// </summary>
        private async void PlaceActiveHandItemInHands(
            EntityUid user,
            EntityUid target,
            EntityUid held,
            string handName,
            StrippableComponent component)
        {
            var hands = Comp<HandsComponent>(target);
            var userHands = Comp<HandsComponent>(user);

            bool Check()
            {
                if (userHands.ActiveHandEntity != held)
                    return false;

                if (!_handsSystem.CanDropHeld(user, userHands.ActiveHand!))
                {
                    _popup.PopupCursor(Loc.GetString("strippable-component-cannot-drop"), user);
                    return false;
                }

                if (!_handsSystem.TryGetHand(target, handName, out var hand, hands)
                    || !_handsSystem.CanPickupToHand(target, userHands.ActiveHandEntity.Value, hand, checkActionBlocker: false, hands))
                {
                    _popup.PopupCursor(Loc.GetString("strippable-component-cannot-put-message",("owner", target)), user);
                    return false;
                }

                return true;
            }

            var userEv = new BeforeStripEvent(component.HandStripDelay);
            RaiseLocalEvent(user, userEv);
            var ev = new BeforeGettingStrippedEvent(userEv.Time, userEv.Stealth);
            RaiseLocalEvent(target, ev);

            var doAfterArgs = new DoAfterArgs(EntityManager, user, ev.Time, new AwaitedDoAfterEvent(), null, target: target, used: held)
            {
                ExtraCheck = Check,
                Hidden = ev.Stealth,
>>>>>>> parent of 23059a860d (Reapply "Merge branch 'Simple-Station:master' into Psionic-Power-Refactor")
=======
                Hidden = stealth,
>>>>>>> parent of 2f3ee29ec0 (Revert "Merge branch 'Simple-Station:master' into Psionic-Power-Refactor")
                AttemptFrequency = AttemptFrequency.EveryTick,
                BreakOnDamage = true,
                BreakOnTargetMove = true,
                BreakOnUserMove = true,
                NeedHand = true,
                DuplicateCondition = DuplicateConditions.SameTool
            };

            _doAfterSystem.TryStartDoAfter(doAfterArgs);
        }

        /// <summary>
        ///     Inserts the item in the user's active hand into the inventory slot.
        /// </summary>
        private void StripInsertInventory(
            Entity<HandsComponent?> user,
            EntityUid target,
            EntityUid held,
            string slot)
        {
            if (!Resolve(user, ref user.Comp))
                return;

            if (!CanStripInsertInventory(user, target, held, slot))
                return;

            if (!_handsSystem.TryDrop(user, handsComp: user.Comp))
                return;

            _inventorySystem.TryEquip(user, target, held, slot);
            _adminLogger.Add(LogType.Stripping, LogImpact.Medium, $"{ToPrettyString(user):actor} has placed the item {ToPrettyString(held):item} in {ToPrettyString(target):target}'s {slot} slot");
        }

        /// <summary>
        ///     Checks whether the item can be removed from the target's inventory.
        /// </summary>
        private bool CanStripRemoveInventory(
            EntityUid user,
            EntityUid target,
            EntityUid item,
            string slot)
        {
            if (!_inventorySystem.TryGetSlotEntity(target, slot, out var slotItem))
            {
                _popupSystem.PopupCursor(Loc.GetString("strippable-component-item-slot-free-message", ("owner", target)), user);
                return false;
            }

            if (slotItem != item)
                return false;

            if (!_inventorySystem.CanUnequip(user, target, slot, out var reason))
            {
                _popupSystem.PopupCursor(Loc.GetString(reason), user);
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Begins a DoAfter to remove the item from the target's inventory and insert it in the user's active hand.
        /// </summary>
        private void StartStripRemoveInventory(
            EntityUid user,
            EntityUid target,
            EntityUid item,
            string slot)
        {
            if (!CanStripRemoveInventory(user, target, item, slot))
                return;

            if (!_inventorySystem.TryGetSlot(target, slot, out var slotDef))
            {
                Log.Error($"{ToPrettyString(user)} attempted to take an item from a non-existent inventory slot ({slot}) on {ToPrettyString(target)}");
                return;
            }

            var (time, stealth) = GetStripTimeModifiers(user, target, slotDef.StripTime);

<<<<<<< HEAD
<<<<<<< HEAD
            bool hidden = stealth == ThievingStealth.Hidden;

            if (!hidden)
            {
                if (slotDef.StripHidden)
                    StripPopup("strippable-component-alert-owner-hidden", stealth, target, slot: slot);
                else
                    StripPopup("strippable-component-alert-owner", stealth, target, user: Identity.Entity(user, EntityManager), item: item);
            }

            var prefix = hidden ? "stealthily " : "";
            _adminLogger.Add(LogType.Stripping, LogImpact.Low, $"{ToPrettyString(user):actor} is trying to {prefix}strip the item {ToPrettyString(item):item} from {ToPrettyString(target):target}'s {slot} slot");

            var doAfterArgs = new DoAfterArgs(EntityManager, user, time, new StrippableDoAfterEvent(false, true, slot), user, target, item)
            {
                Hidden = hidden,
                AttemptFrequency = AttemptFrequency.EveryTick,
                BreakOnDamage = true,
                BreakOnTargetMove = true,
                BreakOnUserMove = true,
                NeedHand = true,
                BreakOnHandChange = false, // Allow simultaneously removing multiple items.
                DuplicateCondition = DuplicateConditions.SameTool
            };

            _doAfterSystem.TryStartDoAfter(doAfterArgs);
        }

        /// <summary>
        ///     Removes the item from the target's inventory and inserts it in the user's active hand.
        /// </summary>
        private void StripRemoveInventory(
            EntityUid user,
            EntityUid target,
            EntityUid item,
            string slot,
            bool hidden)
        {
            if (!CanStripRemoveInventory(user, target, item, slot))
=======
            var doAfterArgs = new DoAfterArgs(EntityManager, user, ev.Time, new AwaitedDoAfterEvent(), null, target: target, used: item)
=======
            if (!stealth)
>>>>>>> parent of 2f3ee29ec0 (Revert "Merge branch 'Simple-Station:master' into Psionic-Power-Refactor")
            {
                if (slotDef.StripHidden)
                    _popupSystem.PopupEntity(Loc.GetString("strippable-component-alert-owner-hidden", ("slot", slot)), target, target, PopupType.Large);
                else
                    _popupSystem.PopupEntity(Loc.GetString("strippable-component-alert-owner", ("user", Identity.Entity(user, EntityManager)), ("item", item)), target, target, PopupType.Large);
            }

            var prefix = stealth ? "stealthily " : "";
            _adminLogger.Add(LogType.Stripping, LogImpact.Low, $"{ToPrettyString(user):actor} is trying to {prefix}strip the item {ToPrettyString(item):item} from {ToPrettyString(target):target}'s {slot} slot");

            var doAfterArgs = new DoAfterArgs(EntityManager, user, time, new StrippableDoAfterEvent(false, true, slot), user, target, item)
            {
                Hidden = stealth,
                AttemptFrequency = AttemptFrequency.EveryTick,
                BreakOnDamage = true,
                BreakOnTargetMove = true,
                BreakOnUserMove = true,
                NeedHand = true,
                BreakOnHandChange = false, // Allow simultaneously removing multiple items.
                DuplicateCondition = DuplicateConditions.SameTool
            };

<<<<<<< HEAD
            if (!ev.Stealth && Check())
            {
                if (slotDef.StripHidden)
                {
                    _popup.PopupEntity(Loc.GetString("strippable-component-alert-owner-hidden", ("slot", slot)), target,
                        target, PopupType.Large);
                }
                else if (_inventorySystem.TryGetSlotEntity(strippable, slot, out var slotItem))
                {
                    _popup.PopupEntity(Loc.GetString("strippable-component-alert-owner", ("user", Identity.Entity(user, EntityManager)), ("item", slotItem)), target,
                        target, PopupType.Large);
                }
            }

            var prefix = ev.Stealth ? "stealthily " : "";
            _adminLogger.Add(LogType.Stripping, LogImpact.Low, $"{ToPrettyString(user):actor} is trying to {prefix}strip the item {ToPrettyString(item):item} from {ToPrettyString(target):target}'s {slot} slot");

            var result = await _doAfter.WaitDoAfter(doAfterArgs);
            if (result != DoAfterStatus.Finished)
>>>>>>> parent of 23059a860d (Reapply "Merge branch 'Simple-Station:master' into Psionic-Power-Refactor")
                return;

            if (!_inventorySystem.TryUnequip(user, strippable, slot))
                return;

            // Raise a dropped event, so that things like gas tank internals properly deactivate when stripping
            RaiseLocalEvent(item, new DroppedEvent(user), true);

<<<<<<< HEAD
            _handsSystem.PickupOrDrop(user, item, animateUser: hidden, animate: hidden);
=======
            _handsSystem.PickupOrDrop(user, item, animateUser: !ev.Stealth, animate: !ev.Stealth);
>>>>>>> parent of 23059a860d (Reapply "Merge branch 'Simple-Station:master' into Psionic-Power-Refactor")
            _adminLogger.Add(LogType.Stripping, LogImpact.Medium, $"{ToPrettyString(user):actor} has stripped the item {ToPrettyString(item):item} from {ToPrettyString(target):target}'s {slot} slot");

=======
            _doAfterSystem.TryStartDoAfter(doAfterArgs);
>>>>>>> parent of 2f3ee29ec0 (Revert "Merge branch 'Simple-Station:master' into Psionic-Power-Refactor")
        }

        /// <summary>
        ///     Removes the item from the target's inventory and inserts it in the user's active hand.
        /// </summary>
        private void StripRemoveInventory(
            EntityUid user,
            EntityUid target,
            EntityUid item,
            string slot,
            bool stealth)
        {
            if (!CanStripRemoveInventory(user, target, item, slot))
                return;

            if (!_inventorySystem.TryUnequip(user, target, slot))
                return;

            RaiseLocalEvent(item, new DroppedEvent(user), true); // Gas tank internals etc.

            _handsSystem.PickupOrDrop(user, item, animateUser: stealth, animate: stealth);
            _adminLogger.Add(LogType.Stripping, LogImpact.Medium, $"{ToPrettyString(user):actor} has stripped the item {ToPrettyString(item):item} from {ToPrettyString(target):target}'s {slot} slot");
        }

        /// <summary>
        ///     Checks whether the item in the user's active hand can be inserted into one of the target's hands.
        /// </summary>
        private bool CanStripInsertHand(
            Entity<HandsComponent?> user,
            Entity<HandsComponent?> target,
            EntityUid held,
            string handName)
        {
            if (!Resolve(user, ref user.Comp) ||
                !Resolve(target, ref target.Comp))
                return false;

            if (user.Comp.ActiveHand == null)
                return false;

            if (user.Comp.ActiveHandEntity == null)
                return false;

            if (user.Comp.ActiveHandEntity != held)
                return false;

            if (!_handsSystem.CanDropHeld(user, user.Comp.ActiveHand))
            {
                _popupSystem.PopupCursor(Loc.GetString("strippable-component-cannot-drop"), user);
                return false;
            }

            if (!_handsSystem.TryGetHand(target, handName, out var handSlot, target.Comp) ||
                !_handsSystem.CanPickupToHand(target, user.Comp.ActiveHandEntity.Value, handSlot, checkActionBlocker: false, target.Comp))
            {
<<<<<<< HEAD
<<<<<<< HEAD
=======
>>>>>>> parent of 2f3ee29ec0 (Revert "Merge branch 'Simple-Station:master' into Psionic-Power-Refactor")
                _popupSystem.PopupCursor(Loc.GetString("strippable-component-cannot-put-message", ("owner", target)), user);
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Begins a DoAfter to insert the item in the user's active hand into one of the target's hands.
        /// </summary>
        private void StartStripInsertHand(
            Entity<HandsComponent?> user,
            Entity<HandsComponent?> target,
            EntityUid held,
            string handName,
            StrippableComponent? targetStrippable = null)
        {
            if (!Resolve(user, ref user.Comp) ||
                !Resolve(target, ref target.Comp) ||
                !Resolve(target, ref targetStrippable))
                return;

            if (!CanStripInsertHand(user, target, held, handName))
                return;

            var (time, stealth) = GetStripTimeModifiers(user, target, targetStrippable.HandStripDelay);

<<<<<<< HEAD
            bool hidden = stealth == ThievingStealth.Hidden;

            var prefix = hidden ? "stealthily " : "";
=======
            var prefix = stealth ? "stealthily " : "";
>>>>>>> parent of 2f3ee29ec0 (Revert "Merge branch 'Simple-Station:master' into Psionic-Power-Refactor")
            _adminLogger.Add(LogType.Stripping, LogImpact.Low, $"{ToPrettyString(user):actor} is trying to {prefix}place the item {ToPrettyString(held):item} in {ToPrettyString(target):target}'s hands");

            var doAfterArgs = new DoAfterArgs(EntityManager, user, time, new StrippableDoAfterEvent(true, false, handName), user, target, held)
            {
<<<<<<< HEAD
                Hidden = hidden,
=======
                ExtraCheck = Check,
                Hidden = ev.Stealth,
>>>>>>> parent of 23059a860d (Reapply "Merge branch 'Simple-Station:master' into Psionic-Power-Refactor")
=======
                Hidden = stealth,
>>>>>>> parent of 2f3ee29ec0 (Revert "Merge branch 'Simple-Station:master' into Psionic-Power-Refactor")
                AttemptFrequency = AttemptFrequency.EveryTick,
                BreakOnDamage = true,
                BreakOnTargetMove = true,
                BreakOnUserMove = true,
                NeedHand = true,
                DuplicateCondition = DuplicateConditions.SameTool
            };

<<<<<<< HEAD
<<<<<<< HEAD
            _doAfterSystem.TryStartDoAfter(doAfterArgs);
        }

        /// <summary>
        ///     Places the item in the user's active hand into one of the target's hands.
        /// </summary>
        private void StripInsertHand(
            Entity<HandsComponent?> user,
            Entity<HandsComponent?> target,
            EntityUid held,
            string handName,
            bool hidden)
        {
            if (!Resolve(user, ref user.Comp) ||
                !Resolve(target, ref target.Comp))
                return;

            if (!CanStripInsertHand(user, target, held, handName))
                return;

            _handsSystem.TryDrop(user, checkActionBlocker: false, handsComp: user.Comp);
            _handsSystem.TryPickup(target, held, handName, checkActionBlocker: false, animateUser: hidden, animate: hidden, handsComp: target.Comp);
            _adminLogger.Add(LogType.Stripping, LogImpact.Medium, $"{ToPrettyString(user):actor} has placed the item {ToPrettyString(held):item} in {ToPrettyString(target):target}'s hands");

            // Hand update will trigger strippable update.
        }

        /// <summary>
        ///     Checks whether the item is in the target's hand and whether it can be dropped.
        /// </summary>
        private bool CanStripRemoveHand(
            EntityUid user,
            Entity<HandsComponent?> target,
            EntityUid item,
            string handName)
        {
            if (!Resolve(target, ref target.Comp))
                return false;

            if (!_handsSystem.TryGetHand(target, handName, out var handSlot, target.Comp))
=======
            if (!ev.Stealth && Check() && _handsSystem.TryGetHand(target, handName, out var handSlot, hands) && handSlot.HeldEntity != null)
>>>>>>> parent of 23059a860d (Reapply "Merge branch 'Simple-Station:master' into Psionic-Power-Refactor")
            {
                _popup.PopupEntity(
                    Loc.GetString("strippable-component-alert-owner",
                    ("user", Identity.Entity(user, EntityManager)), ("item", item)),
                    strippable.Owner,
                    strippable.Owner);
            }
=======
            _doAfterSystem.TryStartDoAfter(doAfterArgs);
        }
>>>>>>> parent of 2f3ee29ec0 (Revert "Merge branch 'Simple-Station:master' into Psionic-Power-Refactor")

        /// <summary>
        ///     Places the item in the user's active hand into one of the target's hands.
        /// </summary>
        private void StripInsertHand(
            Entity<HandsComponent?> user,
            Entity<HandsComponent?> target,
            EntityUid held,
            string handName,
            bool stealth)
        {
            if (!Resolve(user, ref user.Comp) ||
                !Resolve(target, ref target.Comp))
                return;

<<<<<<< HEAD
<<<<<<< HEAD
<<<<<<< HEAD
=======
            if (!CanStripInsertHand(user, target, held, handName))
                return;

=======
>>>>>>> parent of 7b89ce1326 (Cherrypick "Fix StrippableSystem Blunders" (#504))
            _handsSystem.TryDrop(user, checkActionBlocker: false, handsComp: user.Comp);
            _handsSystem.TryPickup(target, held, handName, checkActionBlocker: false, animateUser: stealth, animate: stealth, handsComp: target.Comp);
            _adminLogger.Add(LogType.Stripping, LogImpact.Medium, $"{ToPrettyString(user):actor} has placed the item {ToPrettyString(held):item} in {ToPrettyString(target):target}'s hands");

            // Hand update will trigger strippable update.
        }

        /// <summary>
        ///     Checks whether the item is in the target's hand and whether it can be dropped.
        /// </summary>
        private bool CanStripRemoveHand(
            EntityUid user,
            Entity<HandsComponent?> target,
            EntityUid item,
            string handName)
        {
            if (!Resolve(target, ref target.Comp))
                return false;

            if (!_handsSystem.TryGetHand(target, handName, out var handSlot, target.Comp))
            {
                _popupSystem.PopupCursor(Loc.GetString("strippable-component-item-slot-free-message", ("owner", target)), user);
                return false;
            }

            if (HasComp<VirtualItemComponent>(handSlot.HeldEntity))
                return false;

            if (handSlot.HeldEntity == null)
                return false;

            if (handSlot.HeldEntity != item)
                return false;

            if (!_handsSystem.CanDropHeld(target, handSlot, false))
            {
                _popupSystem.PopupCursor(Loc.GetString("strippable-component-cannot-drop-message", ("owner", target)), user);
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Begins a DoAfter to remove the item from the target's hand and insert it in the user's active hand.
        /// </summary>
        private void StartStripRemoveHand(
            Entity<HandsComponent?> user,
            Entity<HandsComponent?> target,
            EntityUid item,
            string handName,
            StrippableComponent? targetStrippable = null)
        {
            if (!Resolve(user, ref user.Comp) ||
                !Resolve(target, ref target.Comp) ||
                !Resolve(target, ref targetStrippable))
                return;

>>>>>>> parent of 2f3ee29ec0 (Revert "Merge branch 'Simple-Station:master' into Psionic-Power-Refactor")
            if (!CanStripRemoveHand(user, target, item, handName))
                return;

            var (time, stealth) = GetStripTimeModifiers(user, target, targetStrippable.HandStripDelay);

<<<<<<< HEAD
            bool hidden = stealth == ThievingStealth.Hidden;

            if (!hidden)
                StripPopup("strippable-component-alert-owner", stealth, target, user: Identity.Entity(user, EntityManager), item: item);

            var prefix = hidden ? "stealthily " : "";
=======
            if (!stealth)
                _popupSystem.PopupEntity( Loc.GetString("strippable-component-alert-owner", ("user", Identity.Entity(user, EntityManager)), ("item", item)), target, target);

            var prefix = stealth ? "stealthily " : "";
>>>>>>> parent of 2f3ee29ec0 (Revert "Merge branch 'Simple-Station:master' into Psionic-Power-Refactor")
            _adminLogger.Add(LogType.Stripping, LogImpact.Low, $"{ToPrettyString(user):actor} is trying to {prefix}strip the item {ToPrettyString(item):item} from {ToPrettyString(target):target}'s hands");

            var doAfterArgs = new DoAfterArgs(EntityManager, user, time, new StrippableDoAfterEvent(false, false, handName), user, target, item)
            {
<<<<<<< HEAD
                Hidden = hidden,
=======
                Hidden = stealth,
>>>>>>> parent of 2f3ee29ec0 (Revert "Merge branch 'Simple-Station:master' into Psionic-Power-Refactor")
                AttemptFrequency = AttemptFrequency.EveryTick,
                BreakOnDamage = true,
                BreakOnTargetMove = true,
                BreakOnUserMove = true,
                NeedHand = true,
                BreakOnHandChange = false, // Allow simultaneously removing multiple items.
                DuplicateCondition = DuplicateConditions.SameTool
            };

            _doAfterSystem.TryStartDoAfter(doAfterArgs);
        }

        /// <summary>
        ///     Takes the item from the target's hand and inserts it in the user's active hand.
        /// </summary>
        private void StripRemoveHand(
            Entity<HandsComponent?> user,
            Entity<HandsComponent?> target,
            EntityUid item,
<<<<<<< HEAD
            string handName,
<<<<<<< HEAD
            bool hidden)
=======
=======
>>>>>>> parent of 7b89ce1326 (Cherrypick "Fix StrippableSystem Blunders" (#504))
            bool stealth)
>>>>>>> parent of 2f3ee29ec0 (Revert "Merge branch 'Simple-Station:master' into Psionic-Power-Refactor")
        {
            if (!Resolve(user, ref user.Comp) ||
                !Resolve(target, ref target.Comp))
                return;

            _handsSystem.TryDrop(target, item, checkActionBlocker: false, handsComp: target.Comp);
<<<<<<< HEAD
            _handsSystem.PickupOrDrop(user, item, animateUser: hidden, animate: hidden, handsComp: user.Comp);
=======
            _handsSystem.PickupOrDrop(user, item, animateUser: stealth, animate: stealth, handsComp: user.Comp);
>>>>>>> parent of 2f3ee29ec0 (Revert "Merge branch 'Simple-Station:master' into Psionic-Power-Refactor")
            _adminLogger.Add(LogType.Stripping, LogImpact.Medium, $"{ToPrettyString(user):actor} has stripped the item {ToPrettyString(item):item} from {ToPrettyString(target):target}'s hands");

            // Hand update will trigger strippable update.
        }

        private void OnStrippableDoAfterRunning(Entity<HandsComponent> entity, ref DoAfterAttemptEvent<StrippableDoAfterEvent> ev)
        {
            var args = ev.DoAfter.Args;

            DebugTools.Assert(entity.Owner == args.User);
            DebugTools.Assert(args.Target != null);
            DebugTools.Assert(args.Used != null);
            DebugTools.Assert(ev.Event.SlotOrHandName != null);

            if (ev.Event.InventoryOrHand)
            {
                if ( ev.Event.InsertOrRemove && !CanStripInsertInventory((entity.Owner, entity.Comp), args.Target.Value, args.Used.Value, ev.Event.SlotOrHandName) ||
                    !ev.Event.InsertOrRemove && !CanStripRemoveInventory(entity.Owner, args.Target.Value, args.Used.Value, ev.Event.SlotOrHandName))
                        ev.Cancel();
            }
            else
            {
                if ( ev.Event.InsertOrRemove && !CanStripInsertHand((entity.Owner, entity.Comp), args.Target.Value, args.Used.Value, ev.Event.SlotOrHandName) ||
                    !ev.Event.InsertOrRemove && !CanStripRemoveHand(entity.Owner, args.Target.Value, args.Used.Value, ev.Event.SlotOrHandName))
                        ev.Cancel();
            }
        }

        private void OnStrippableDoAfterFinished(Entity<HandsComponent> entity, ref StrippableDoAfterEvent ev)
        {
            if (ev.Cancelled)
                return;

            DebugTools.Assert(entity.Owner == ev.User);
            DebugTools.Assert(ev.Target != null);
            DebugTools.Assert(ev.Used != null);
            DebugTools.Assert(ev.SlotOrHandName != null);

            if (ev.InventoryOrHand)
            {
                if (ev.InsertOrRemove)
                        StripInsertInventory((entity.Owner, entity.Comp), ev.Target.Value, ev.Used.Value, ev.SlotOrHandName);
                else    StripRemoveInventory(entity.Owner, ev.Target.Value, ev.Used.Value, ev.SlotOrHandName, ev.Args.Hidden);
            }
            else
            {
                if (ev.InsertOrRemove)
                        StripInsertHand((entity.Owner, entity.Comp), ev.Target.Value, ev.Used.Value, ev.SlotOrHandName, ev.Args.Hidden);
                else    StripRemoveHand((entity.Owner, entity.Comp), ev.Target.Value, ev.Used.Value, ev.Args.Hidden);
            }
<<<<<<< HEAD
=======
            _handsSystem.TryDrop(target, item, checkActionBlocker: false, handsComp: hands);
            _handsSystem.PickupOrDrop(user, item, animateUser: !ev.Stealth, animate: !ev.Stealth, handsComp: userHands);
            // hand update will trigger strippable update
            _adminLogger.Add(LogType.Stripping, LogImpact.Medium,
                $"{ToPrettyString(user):actor} has stripped the item {ToPrettyString(item):item} from {ToPrettyString(target):target}'s hands");
>>>>>>> parent of 23059a860d (Reapply "Merge branch 'Simple-Station:master' into Psionic-Power-Refactor")
=======
>>>>>>> parent of 2f3ee29ec0 (Revert "Merge branch 'Simple-Station:master' into Psionic-Power-Refactor")
        }
    }
}
