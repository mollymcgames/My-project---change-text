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
    // bool REAL_DALLE = true; //TODO: Set this to true when you want to use Dalle for real or false otherwise
    //                         // bool REAL_DALLE = true; //unused for now

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
        textinput = s;
        Debug.Log("Input string: " + textinput);

        GameModel model = Schedule.GetModel<GameModel>();
        model.textInput = textinput;

        Debug.Log("One text box for joe coming up!");
        inputFieldText.gameObject.SetActive(false);


        Vector3 screenPosition = Camera.main.WorldToScreenPoint(transform.position); // Get the NPC's position on the screen
        inputFieldText.transform.position = screenPosition + new Vector3(0, 150, 0); // Set the dialogue box's position to be above the NPC        

        await SortOutChatText(textinput);

        // StartCoroutine(MakeRequest());
        model.textInput = dialogueText;

        //calculate a position above the player's sprite.
        var position = gameObject.transform.position;
        var sr = gameObject.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            position += new Vector3(1.9f, 2 * sr.size.y + 0.2f, 0);
        }

        //show the dialog
        Debug.Log("Sprite position: " + position);
        model.dialog.Show(position, dialogueText);
        model.textInput = "";
        dialogueText = "";
        inputFieldText.gameObject.GetComponent<TMP_InputField>().text = "";
    }

    private async Task SortOutChatText(string inputText)
    {
        Debug.Log("Asking about this: "+ inputText);
        GameObject gameObject = new GameObject("ChatGptHelper");
        cgh = gameObject.AddComponent<ChatGptHelper>();        
        Task<string> getDialogueText = cgh.GetChatText(inputText);
        dialogueText = await getDialogueText;
        Debug.Log("Returned Dialogue Text: " + dialogueText);
        cgh.ResetChat();
    }
}