using UnityEngine;
using UnityEngine.SceneManagement;

public class LooseMenu : MonoBehaviour {

	public void Back()
	{
		SceneManager.LoadScene(0);
	}
}
