using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WindowFluxContent : MonoBehaviour
{

    public Dropdown source;
    public Dropdown target;

    public void AddFlux()
    {
        IFluxSource sourceConstruction;
        var cityCount = World.Instance.Cities.Count;
        if (source.value < cityCount)
            sourceConstruction = World.Instance.Cities[source.value];
        else
            sourceConstruction = World.Instance.Industries[source.value - cityCount];

        var cityTarget = World.Instance.Cities[target.value];

        Simulation.AddFlux(sourceConstruction, cityTarget);
    }

}
