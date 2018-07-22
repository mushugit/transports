using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelLoader : MonoBehaviour
{
    public GameObject LoadCanvas;
    public Slider ProgressBar;
    public Text ProgressIndicator;
    public Text DetailLabel;
    public GameObject oldCamera;
    public GameObject oldLight;

    void Start()
    {
        DontDestroyOnLoad(LoadCanvas);
        StartCoroutine("UpdateLoading");
    }

    IEnumerator UpdateLoading()
    {
        var loadingOperation = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1, LoadSceneMode.Additive);
        DetailLabel.text = "Pré chargement du niveau";


        while (!loadingOperation.isDone && World.GameLoading)
        {
            var progress = loadingOperation.progress;
            progress = Mathf.Clamp01(loadingOperation.progress / 0.9f) / 2f;
            ProgressBar.value = progress;
            ProgressIndicator.text = string.Format("{0} %", Mathf.Round(progress * 100));
            yield return null;
        }

        DestroyImmediate(oldCamera);
        DestroyImmediate(oldLight);

        while (World.GameLoading)
        {
            var progress = 0.5f;
            DetailLabel.text = World.ItemLoading;
            progress += (World.ProgressLoading / World.TotalLoading) / 2f;
            ProgressBar.value = progress;
            ProgressIndicator.text = string.Format("{0} %", Mathf.Floor(progress * 100));
            yield return null;
        }

        LoadCanvas.SetActive(false);
    }
}
