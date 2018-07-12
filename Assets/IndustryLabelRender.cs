using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndustryLabelRender : MonoBehaviour {

    public GameObject industry;

    void OnGUI()
    {
        var point = Camera.main.WorldToScreenPoint(industry.transform.position);
        transform.position = point;
    }
}
