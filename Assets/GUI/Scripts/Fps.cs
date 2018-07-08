using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fps : MonoBehaviour
{

    private string fps;

    IEnumerator Start()
    {
        GUI.depth = 2;
        while (true)
        {
            if (Time.timeScale == 1)
            {
                yield return new WaitForSeconds(0.1f);
                fps = "FPS : " + Mathf.Round(1 / Time.deltaTime);
            }
            else
            {
                fps = "Pause";
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    void OnGUI()
    {
        GUI.Label(new Rect(Screen.width - 80,Screen.height - 30, 75, 25), fps);
    }

}
