using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityRender : MonoBehaviour
{
    string label;

    public void Label(string name)
    {
        label = name;
    }

    public static Component Build(Vector3 position, Component cityPrefab)
    {
        return Instantiate(cityPrefab, position, Quaternion.identity);
    }

    void Start()
    {
        var c = Random.ColorHSV();
        GetComponent<Renderer>().material.color = c;
    }

    void OnGUI()
    {
        var point = Camera.main.WorldToScreenPoint(transform.position);
        GUI.Label(new Rect(point.x - 30f, Screen.height - point.y, 100f, 25f), label); // display its name, or other string
    }

}
