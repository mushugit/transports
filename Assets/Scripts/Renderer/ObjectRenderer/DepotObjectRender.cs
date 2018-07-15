using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DepotObjectRender : MonoBehaviour, IUnityObjectRenderer
{
    public Construction NestedConstruction { get; set; }

    void Start()
    {
        var c = Random.ColorHSV();
        while (c == Color.black)
        {
            c = Random.ColorHSV();
        }

        var renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            r.material.color = c;
        }
    }

}
