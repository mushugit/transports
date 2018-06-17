using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityObjectRender : MonoBehaviour
{




    void Start()
    {
        var c = Random.ColorHSV();
        GetComponent<Renderer>().material.color = c;
    }


}
