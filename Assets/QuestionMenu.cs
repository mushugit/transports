using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuestionMenu : MonoBehaviour {

    public delegate void YesNoResponse(bool response);

    public GameObject QuestionMenuObject;
    public TextMeshProUGUI Title;
    public TextMeshProUGUI Message;

    private static QuestionMenu instance;

    private YesNoResponse callback;

    private void Start()
    {
        instance = this;
    }

    private void InitYesNoQuestion(string title,string message, YesNoResponse callback)
    {
        Title.text = $"- {title} -";
        Message.text = message;
        this.callback = callback;
        QuestionMenuObject.SetActive(true);
    }

    public static void YesNoQuestion(string title, string message, YesNoResponse callbackResponse)
    {
        instance?.InitYesNoQuestion(title, message, callbackResponse);
    }

    private void Answer(bool response)
    {
        QuestionMenuObject.SetActive(false);
        callback(response);
    }

    public void Yes()
    {
        Answer(true);
    }

    public void No()
    {
        Answer(false);
    }
}
