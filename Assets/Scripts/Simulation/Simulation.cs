using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Simulation
{
    public const float TickFrequency = 0.02f;

    public bool Running;

    private readonly List<Flux> _flux;

    private World _world;

    public Simulation(World world)
    {
        _flux = new List<Flux>();
        _world = world;
    }

    public IEnumerator Run()
    {
        Running = true;
        while (Running)
        {
            foreach (var c in _world.Cities)
            {
                c.GenerateCargo();
            }

            foreach (var i in _world.Industries)
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

    public void AddFlux(IFluxSource source, IFluxTarget target, int quantity = 1)
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

    public void AddFlux(Flux dummyFlux)
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

    public void RemoveFlux(Flux f)
    {
        _flux.Remove(f);
    }

    public void Clear()
    {
        _flux.Clear();
        Running = false;
    }

    public void CityDestroyed(City c)
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

