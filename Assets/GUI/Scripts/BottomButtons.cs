using UnityEngine;

public class BottomButtons : MonoBehaviour
{
    public Component fluxWindowPrefab;

    public void BuildCity()
    {
        Builder.City();
    }

    public void BuildRoad()
    {
        Builder.Road();
    }

    public void Bulldoze()
    {
        Builder.Bulldoze();
    }

    public void BuildDepot()
    {
        Builder.Depot();
    }

    public void CreateFlux()
    {
        WindowFactory.BuildFluxSetup("Créer un flux de transport", new Vector3(Screen.width / 2, Screen.height / 2, 0));
    }
}
