using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndustryObjectRender : MonoBehaviour, IUnityObjectRenderer
{

    public Construction NestedConstruction { get; set; }

    void Start()
    {
        var c = Random.ColorHSV();

        var renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            r.material.color = c;
        }
    }
}
