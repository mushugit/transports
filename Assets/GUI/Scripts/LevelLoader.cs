using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelLoader : MonoBehaviour
{
    public GameObject LoadCanvas;
    public Slider progressBar;
    public Text progressIndicator;
    public Text detailLabel;
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
        detailLabel.text = "Pré chargement du niveau";


		while (!loadingOperation.isDone && World.gameLoading)
        {
            var progress = loadingOperation.progress;
            progress = Mathf.Clamp01(loadingOperation.progress / 0.9f) / 2f;
            progressBar.value = progress;
            progressIndicator.text = string.Format("{0} %", Mathf.Round(progress * 100));
            yield return null;
        }

		DestroyImmediate(oldCamera);
		DestroyImmediate(oldLight);

		while (World.gameLoading)
        {
            var progress = 0.5f;
            detailLabel.text = World.itemLoading;
            progress += (World.progressLoading / World.totalLoading)/2f;
            progressBar.value = progress;
            progressIndicator.text = string.Format("{0} %", Mathf.Floor(progress * 100));
            yield return null;
        }

        LoadCanvas.SetActive(false);
    }
}
