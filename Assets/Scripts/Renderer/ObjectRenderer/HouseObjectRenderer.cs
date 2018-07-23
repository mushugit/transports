using UnityEngine;

public class HouseObjectRenderer : MonoBehaviour, IUnityObjectRenderer
{

    public Construction NestedConstruction { get; set; }

    void Start()
    {
        HColor.GetConstructionColor(NestedConstruction, this);
    }
}
