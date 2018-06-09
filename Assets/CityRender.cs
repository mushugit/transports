using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityRender : MonoBehaviour
{
    public GUIStyle labelStyle;

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
        var labelStyle = GUI.skin.GetStyle("Label");
        labelStyle.alignment = TextAnchor.UpperCenter;
        GUI.Label(new Rect(point.x - 100f, Screen.height - point.y, 200f, 25f), label, labelStyle);
    }

}
