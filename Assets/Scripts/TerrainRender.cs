using UnityEngine;

public class TerrainRender : MonoBehaviour
{

    public Component grass;
    public Material defaultMaterialGrass;

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

    public void Revert()
    {
        grass.GetComponent<Renderer>().material = defaultMaterialGrass;
    }
}
