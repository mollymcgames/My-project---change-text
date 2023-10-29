using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGM.Gameplay;
using RPGM.Core;
using UnityEngine.UI;
using OpenAI;
using UnityEngine.Networking;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;


public class TextBoxInput : MonoBehaviour
{

    public GameObject canvas;
    public GameObject inputFieldText;

    private string textinput = "";

    public string dialogueText = "";

    private ChatGptHelper cgh;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public async void CaptureTextInput(string s)
    {
        GameModel model = Schedule.GetModel<GameModel>();
        model.textInput = textinput;

        model.rpgDialog.SetActive(false);

        //calculate a position above the player's sprite.
        var position = gameObject.transform.position;
        var sr = gameObject.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            // Set the width to be three times the current width and the height to be 2
            sr.size = new Vector2(3 * sr.size.x, 2);
        }
        model.dialog.Show(position, "Getting next RPG instructions!");

        textinput = s;
        Debug.Log("Input string: " + textinput);

        inputFieldText.gameObject.SetActive(false);

        await SortOutChatText(textinput);
        model.textInput = dialogueText;

        model.rpgDialogText.text = dialogueText;
        model.rpgDialog.SetActive(true);

        model.textInput = "";
        dialogueText = "";
        inputFieldText.gameObject.GetComponent<TMP_InputField>().text = "";
        inputFieldText.gameObject.SetActive(true);
    }

    public async Task SortOutChatText(string inputText)
    {
        Debug.Log("Asking about this: " + inputText);
        GameObject gameObject = new GameObject("ChatGptHelper");
        cgh = gameObject.AddComponent<ChatGptHelper>();
        Task<string> getDialogueText = cgh.GetChatText(inputText);
        dialogueText = await getDialogueText;
        dialogueText = dialogueText.Replace("```", "").Replace("\\r\\n", "");
        Debug.Log("Returned Dialogue Text: " + dialogueText);
        cgh.ResetChat();
    }
}