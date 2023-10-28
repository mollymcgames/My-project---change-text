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
using System;
using System.Xml.Schema;

public class ChatGptHelper : MonoBehaviour
{
    private static bool chatTextComplete = false;
    private static string dialogueText = "";

    public void ResetChat() 
    {
        chatTextComplete = false;
    }
    
    public async Task<string> GetChatText(string textinput)
    {
        dialogueText = "";

        GoGetChatGPTText(textinput);
        
        await Task.Run(() =>
        {
            while (chatTextComplete == false)
            {
                // Debug.Log("Waiting for dialogueText");
            }
        });
        return dialogueText;
    }

    public async void GoGetChatGPTText(string textinput) 
    {
        GameModel model = Schedule.GetModel<GameModel>();

        dialogueText = "";

        var newMessage = new ChatMessage
        {
            Content = "In ten or fewer words " + textinput,
            Role = "user"
        };
        model.messages.Add(newMessage);
        var openai = new OpenAIApi();

        Debug.Log("sending to internet....");
        var response = await openai.CreateChatCompletion(new CreateChatCompletionRequest()
        {
            Model = "gpt-3.5-turbo-0613",
            Messages = model.messages
        });
        
        dialogueText = response.Choices[0].Message.Content; 
        // Might want to add the response to our "messages" List too, so that chatgpt "remembers" the full chat context...
        var chatMessage = new ChatMessage
        {
            Content = dialogueText,
            Role = "assistant"
        }; 
        model.messages.Add(chatMessage);
        Debug.Log("dialogueText: " + dialogueText);

        chatTextComplete = true;
    }
}