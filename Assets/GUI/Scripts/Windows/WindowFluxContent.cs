using UnityEngine;
using UnityEngine.UI;

public class WindowFluxContent : MonoBehaviour
{

    public Dropdown source;
    public Dropdown target;
    public Text quantity;
    public Dropdown type;

    public void AddFlux()
    {
        ServiceLocator.GetInstance<Simulation>().AddFlux(GetSource(), GetTarget(), GetCharacteristics(), GetQuantity());
    }

    private IFluxSource GetSource()
    {
        var cityCount = World.Instance.Cities.Count;
        if (source.value < cityCount)
            return World.Instance.Cities[source.value];
        else
            return World.Instance.Industries[source.value - cityCount];
    }

    private IFluxTarget GetTarget()
    {
        return World.Instance.Cities[target.value];
    }

    private int GetQuantity()
    {
        var intQuantity = 1;
        if (int.TryParse(quantity.text, out intQuantity))
            return intQuantity;
        else
            return 1;
    }

    private RoadVehiculeCharacteristics GetCharacteristics()
    {
        switch (type.value)
        {
            case 0:
                return new RoadVehiculeCharacteristics(1, Flux.DefaultSpeed);
            case 1:
                return new RoadVehiculeCharacteristics(2, Flux.DefaultSpeed / 1.75f);
            default:
                return new RoadVehiculeCharacteristics(1, Flux.DefaultSpeed);
        }
    }

}
