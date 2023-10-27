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

    private string textinput;

    public string dialogueText = "";


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void CaptureTextInput(string s)
    {
        textinput = s;
        Debug.Log("Input string: " + textinput);

        GameModel model = Schedule.GetModel<GameModel>();
        model.textInput = textinput;

        Debug.Log("One text box for joe coming up!");
        inputFieldText.gameObject.SetActive(false);


        Vector3 screenPosition = Camera.main.WorldToScreenPoint(transform.position); // Get the NPC's position on the screen
        inputFieldText.transform.position = screenPosition + new Vector3(0, 150, 0); // Set the dialogue box's position to be above the NPC        
    
        StartCoroutine(MakeRequest());
        model.textInput = dialogueText;
        if(dialogueText == "")
        {
            dialogueText = "I don't know what to say to that.";
        }

        // while (dialogueText == "")
        // {
        //     Debug.Log("Waiting for dialogue text to be set...");
        // }

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
    }

    IEnumerator MakeRequest()
    {
        var newMessage = new ChatMessage
        {
            Content = textinput,
            Role = "user"
        };
        List<ChatMessage> messages = new List<ChatMessage>();
        messages.Add(newMessage);
        SendTextRequest(messages);
        Debug.Log("MakeRequest coroutine started.");
        yield return new WaitUntil(() => dialogueText == "");  //WaitWhile is the opposite of WaitUntil
    }

    private async void SendTextRequest(List<ChatMessage> messages)
    {
        var openai = new OpenAIApi();

        Debug.Log("sending to internet....");
        var response = await openai.CreateChatCompletion(new CreateChatCompletionRequest()
        {
            Model = "gpt-3.5-turbo-0613",
            Messages = messages
        });

        dialogueText = response.Choices[0].Message.Content;    
    }

}