using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndustryObjectRender : MonoBehaviour {

    public Industry _Industry { get; private set; }

    public void Industry(Industry ind)
    {
        _Industry = ind;
    }

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
