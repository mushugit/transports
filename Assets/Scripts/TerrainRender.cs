using UnityEngine;

public class TerrainRender : MonoBehaviour
{

    public Component grass;

    public Material red;
    public Material blue;

    public void MakeRed()
    {
        grass.GetComponent<Renderer>().material = red;
    }

    public void MakeBlue()
    {
        grass.GetComponent<Renderer>().material = blue;
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
