using Content.Server.Psionics.Abilities;
using Content.Shared.Anomaly;
using Content.Shared.Anomaly.Components;
using Robust.Shared.Random;

namespace Content.Server.Anomaly;

public sealed partial class AnomalySystem
{
    [Dependency] private readonly SharedAnomalySystem _sharedAnomaly = default!;
    [Dependency] private readonly DispelPowerSystem _dispel = default!;


    private void InitializePsionics()
    {
        SubscribeLocalEvent<AnomalyComponent, DispelledEvent>(OnDispelled);
    }
<<<<<<< HEAD

<<<<<<< HEAD
    //Nyano - Summary: gives dispellable behavior to Anomalies.
=======
    //Nyano - Summary: gives dispellable behavior to Anomalies. 
>>>>>>> parent of 462e91c2cc (aaaaaaaaa)
=======
>>>>>>> parent of d439c5a962 (Revert "Merge branch 'VMSolidus-Psionic-Power-Refactor'")
    private void OnDispelled(EntityUid uid, AnomalyComponent component, DispelledEvent args)
    {
        _dispel.DealDispelDamage(uid);
        _sharedAnomaly.ChangeAnomalyHealth(uid, 0 - _random.NextFloat(0.4f, 0.8f), component);
        args.Handled = true;
    }
}
