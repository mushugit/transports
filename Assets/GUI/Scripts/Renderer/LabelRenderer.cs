using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LabelRenderer : MonoBehaviour {

    public GameObject labelPosition;
    public Component label;

    void OnGUI()
    {
        if (World.DisplayLabel)
        {
            label.gameObject.SetActive(true);
            var point = Camera.main.WorldToScreenPoint(labelPosition.transform.position);
            label.transform.position = point;
        }
        else
        {
            label.gameObject.SetActive(false);
        }
    }
}
