using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Simulation
{
    public static readonly float TickFrequency = 0.02f;
    public static bool Running;

    private static readonly List<Flux> _flux;

    static Simulation()
    {
        _flux = new List<Flux>();
    }

    public static IEnumerator Run()
    {
        Running = true;
        while (Running)
        {
            foreach (var c in World.Instance.Cities)
            {
                c.GenerateCargo();
            }

            foreach (var i in World.Instance.Industries)
            {
                i.GenerateCargo();
            }

            foreach (var f in _flux)
            {
                f.Move();
            }

            yield return new WaitForSeconds(TickFrequency);
        }
    }

    public static void AddFlux(IFluxSource source, IFluxTarget target, int quantity = 1)
    {
        int cost;
        if (!Economy.CheckCost(World.LocalEconomy, "flux_create", "ajouter un flux", out cost))
        {
            return;
        }

        if (source.OutgoingFlux.ContainsKey(target))
        {
            var f = source.OutgoingFlux[target];
            f.AddTrucks(quantity);
        }
        else
        {
            var f = new Flux(source, target, quantity);

            if (f.Path == null)
            {
                Message.ShowError("Flux impossible",
                    $"Impossible de trouver un flux de {source} vers {target} par la route.");
                World.LocalEconomy.Credit(cost);
                return;
            }

            _flux.Add(f);
        }
    }

    public static void AddFlux(Flux dummyFlux)
    {
        int cost;
        if (!Economy.CheckCost(World.LocalEconomy, "flux_create", "ajouter un flux", out cost))
        {
            return;
        }

        var f = new Flux(dummyFlux);

        if (f.Path == null)
        {
            Message.ShowError("Flux impossible",
                $"Impossible de trouver un flux de {f.Source} vers {f.Target} par la route.");
            World.LocalEconomy.Credit(cost);
            return;
        }

        _flux.Add(f);
    }

    public static void RemoveFlux(Flux f)
    {
        _flux.Remove(f);
    }

    public static void Clear()
    {
        _flux.Clear();
        Running = false;
    }

    public static void CityDestroyed(City c)
    {
        foreach (var flux in _flux)
        {
            if (flux.Source == c || flux.Target == c)
            {
                RemoveFlux(flux);
                Flux.RemoveFlux(flux);
                if (flux.Source == c)
                {
                    flux.Target.RemoveFlux(flux);
                }
                else
                {
                    flux.Source.RemoveFlux(flux);
                }
            }
        }
    }
}

