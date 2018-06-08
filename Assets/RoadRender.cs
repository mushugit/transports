using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadRender : MonoBehaviour
{

    static Component[] roadPrefabs;

    public static Component Build(Vector3 position, Component[] roadPrefabs)
    {
        RoadRender.roadPrefabs = roadPrefabs;
        return Instantiate(roadPrefabs[0], position, Quaternion.identity);
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
