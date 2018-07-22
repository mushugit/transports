using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Simulation
{
    public static readonly float TickFrequency = 0.02f;
    public static bool Running = false;
    private static List<Flux> flux;

    static Simulation()
    {
        flux = new List<Flux>();
    }

    public static IEnumerator Run()
    {
        Running = true;
        while (Running)
        {
            foreach (City c in World.Instance.Cities)
            {
                c.GenerateCargo();
            }
            foreach (Industry i in World.Instance.Industries)
            {
                i.GenerateCargo();
            }
            foreach (Flux f in flux)
            {
                f.Move();
            }
            yield return new WaitForSeconds(TickFrequency);
        }
    }

    public static void AddFlux(IFluxSource source, IFluxTarget target, RoadVehiculeCharacteristics type, int quantity = 1)
    {
        int cost;
        if (!Economy.CheckCost(World.LocalEconomy, "flux_create", "ajouter un flux", out cost))
            return;

        if (source.OutgoingFlux.ContainsKey(target))
        {
            var f = source.OutgoingFlux[target];
            f.AddTrucks(quantity, type);
        }
        else
        {
            var f = new Flux(source, target, quantity, type);

            if (f.Path == null)
            {
                Message.ShowError("Flux impossible",
                    $"Impossible de trouver un flux de {source} vers {target} par la route.");
                World.LocalEconomy.Credit(cost);
                return;
            }

            flux.Add(f);
        }
    }

    public static void AddFlux(Flux dummyFlux)
    {
        int cost;
        if (!Economy.CheckCost(World.LocalEconomy, "flux_create", "ajouter un flux", out cost))
            return;

        var f = new Flux(dummyFlux);

        if (f.Path == null)
        {
            Message.ShowError("Flux impossible",
                $"Impossible de trouver un flux de {f.Source} vers {f.Target} par la route.");
            World.LocalEconomy.Credit(cost);
            return;
        }

        flux.Add(f);
    }

    public static void RemoveFlux(Flux f)
    {
        flux.Remove(f);
    }

    public static void Clear()
    {
        flux.Clear();
        Running = false;
    }

    public static void CityDestroyed(City c)
    {
        foreach (Flux f in flux)
        {
            if (f.Source == c || f.Target == c)
            {
                RemoveFlux(f);
                Flux.RemoveFlux(f);
                if (f.Source == c)
                    f.Target.RemoveFlux(f);
                else
                    f.Source.RemoveFlux(f);
            }
        }
    }
}

