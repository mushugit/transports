
using TMPro;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public static readonly int StartScreenSceneIndex = 0;
    public static readonly float MessageDelay = 2f;

    public static bool GameIsPaused = false;

    public GameObject PauseMenuObject;
    public GameObject LoadMenuObject;
    public GameObject InfoBox;
    public TextMeshProUGUI MessageText;


    private static PauseMenu instance;

    private bool isSaved = false;
    private float timeScale;

    private void Start()
    {
        timeScale = 1;
        Resume();
        instance = this;
    }

    private void Update()
    {
        if (World.GameLoading)
            return;


        if (Input.GetButtonDown("Pause"))
        {
            if (GameIsPaused)
            {
                if (!LoadMenuObject.activeSelf)
                {
                    Resume();
                }
            }
            else
            {
                if (Builder.IsBuilding || Builder.IsDestroying)
                    Builder.CancelAction();
                else
                    Pause();
            }
        }
    }

    public static void Display(string message, bool isError = false)
    {
        instance?.DisplayMessage(message, isError);
    }

    public void DisplayMessage(string message, bool isError = false)
    {
        MessageText.text = "";
        if (isError)
            MessageText.color = Color.red;
        else
            MessageText.color = Color.white;
        MessageText.text = message;
        InfoBox.SetActive(true);
    }

    public static void ClearMessage()
    {
        if (instance != null)
        {
            instance.MessageText.text = "";
            instance.InfoBox.SetActive(false);
        }
    }

    public static void ForceResume()
    {
        instance?.Resume();
    }

    public void Resume()
    {
        ClearMessage();
        isSaved = false;
        PauseMenuObject?.SetActive(false);
        Time.timeScale = timeScale;
        GameIsPaused = false;
    }

    public void Save()
    {
        string errorMessage;
        if (!SaveHandler.Save(out errorMessage))
            DisplayMessage($"Erreur de création de la sauvegarde : {errorMessage}", true);
        else
            isSaved = true;
        //Resume();
    }

    public void Load()
    {
        ClearMessage();
        PauseMenuObject.SetActive(false);
        LoadMenuObject.SetActive(true);
    }

    void Pause()
    {
        //Debug.Log("Pause");
        timeScale = Time.timeScale;
        PauseMenuObject.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;

    }

    public void ReturnStart()
    {
        ClearMessage();
        if (!isSaved)
        {
            PauseMenuObject.SetActive(false);
            QuestionMenu.YesNoQuestion("Quitter",
                "Votre partie n'est pas sauveardée.\nVoulez-vous vraiment retourner à l'accueil sans sauvegarder ? (votre partie sera perdue)",
                InternalReturnStart);
        }
        else
            InternalReturnStart(true);
    }

    private void InternalReturnStart(bool doReturn)
    {
        PauseMenuObject.SetActive(true);
        if (doReturn)
        {
            ForceResume();
            World.ReloadLevel(StartScreenSceneIndex);
        }
    }

    public void Quit()
    {
        if (!isSaved)
        {
            PauseMenuObject.SetActive(false);
            QuestionMenu.YesNoQuestion("Quitter",
                "Votre partie n'est pas sauveardée.\nVoulez-vous vraiment quitter sans sauvegarder ? (votre partie sera perdue)",
                InternalQuit);
        }
        else
            InternalQuit(true);
    }

    private void InternalQuit(bool doQuit)
    {
        PauseMenuObject.SetActive(true);
        if (doQuit)
        {
            if (Debug.isDebugBuild) Debug.Log("Quit");
            Application.Quit();
        }
    }
}
