using TMPro;
using UnityEngine;

public class SaveGameItem : MonoBehaviour
{

    public TextMeshProUGUI textItem;

    private string fullName;

    public void Text(string displayName, string fullName)
    {
        textItem.text = displayName;
        this.fullName = fullName;
    }

    public void Load()
    {
        string errorMessage;
        var loadMenu = GetComponentInParent<LoadMenu>();
        var pauseMenu = GetComponentInParent<PauseMenu>();
        if (!SaveHandler.Load(fullName, out errorMessage))
        {
            PauseMenu.Display($"Erreur de chargement de la sauvegarde {errorMessage}", true);
        }
        loadMenu.Back();
    }
}
