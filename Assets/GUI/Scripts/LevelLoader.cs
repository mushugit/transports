using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelLoader : MonoBehaviour
{

    public Slider progressBar;
    public Text progressIndicator;

    void Start()
    {
        StartCoroutine("UpdateLoading");
    }

    IEnumerator UpdateLoading()
    {
        var loadingOperation = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);
        while (!loadingOperation.isDone)
        {
            var progress = loadingOperation.progress;
            /*if (loadingOperation.progress < 0.9f)
                progress = Mathf.Clamp01(loadingOperation.progress / 0.9f) / 2f;
            else
                progress = (loadingOperation.progress - 0.9f) * 5f;*/
            progressBar.value = progress;
            progressIndicator.text = string.Format("{0} %", Mathf.Round(progress * 100));
            yield return null;
        }
    }
}
