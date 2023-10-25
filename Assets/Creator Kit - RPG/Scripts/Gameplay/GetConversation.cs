using System.Collections;
using System;
using System.Collections.Generic;
using RPGM.Core;
using RPGM.Gameplay;
using RPGM.UI;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;
using NUnit;
using CandyCoded.env;

namespace RPGM.Events
{
    // CUSTOM BITS
    // A class to hold the JSON response
    [System.Serializable]
    public class Response
    {
        public Choice[] choices;
    }

    [System.Serializable]
    public class Choice
    {
        public string text;
    }

    // END OF CUSTOM BITS

    /// <summary>
    /// This event will start a conversation with an NPC using a conversation script.
    /// </summary>
    /// <typeparam name="ShowConversation"></typeparam>
    public class GetConversation : MonoBehaviour
    {

        // this  is the flag that turns on and off api connectivity 
        bool apiConsumeMode = true; //turn off or on for internet connectivity NOTE: this needs to be on now to make sense of the conversation

        public TMP_Text textmesh;

        public string prompt = "Your Prompt Here";


        //the Make request is an asynchronous operation which prevents blocking
        public void GetTheConversation(NPCController npc, ConversationScript conversation, ConversationPiece originalPiece, string conversationItemKey, string llmSeed, GameObject inputField)
        {
            StartCoroutine(MakeRequest(npc, conversation, originalPiece, conversationItemKey, llmSeed, inputField));
            Debug.Log("GetTheConversation method called.");
            Debug.Log("Received conversationItemKey: " + conversationItemKey);

        }

        IEnumerator MakeRequest(NPCController npc, ConversationScript conversation, ConversationPiece originalPiece, string conversationItemKey, string llmSeed, GameObject inputField)
        {
            bool errorHappened = false;
            Debug.Log("MakeRequest coroutine started.");

            var ourItem = conversation.items.Find(x => x.id.Equals(conversationItemKey));
            var ourItemPosition = conversation.items.IndexOf(ourItem);
            if (ourItemPosition <= 0)
            {
                ourItemPosition = 0;
            }
            Debug.Log("Conversation seed: " + llmSeed);
            //if not using the internet this text should be used
            string internetText = conversation.items[ourItemPosition].text;


            // Construct the JSON message based on the player's role

            string playerRole = "";

            // Check the player's tag to identify their role
            if (gameObject.CompareTag("Gloria"))
            {
                playerRole = "archer";
            }
            else if (gameObject.CompareTag("Loriane"))
            {
                playerRole = "artist";
            }
            else if (gameObject.CompareTag("Elgar"))
            {
                playerRole = "wizard";
            }
            //Construct the message to send to the API
            //Add a check for the player role and then construct the message accordingly
            string message;

            if (playerRole == "archer")
            {
                message = "Please play an as an archer now in an RPG game and act like you need a golden apple.  You remember you are playing this character, don't break character and say you're AI as you are an archer. I am the player and my question or statement is " + llmSeed;
            }
            else if (playerRole == "artist")
            {
                message = "Please play an as an artist now in an RPG game and act like you are composing a painting. You can paint a picture for me if I ask for one. You remember you are playing this character, don't break character and say you're AI as you are an artist. I am the player and my question or statement is " + llmSeed;
            }
            else if (playerRole == "wizard")
            {
                message = "Please play as a wizard in an RPG game and act like you are casting a spell but need a magic pot to complete the spell. You remember you are playing this character, don't break character and say you're AI as you are a wizard. I am the player and my question or statement is " + llmSeed;
            }
            else
            {
                message = "In ten or fewer words " + llmSeed;
            }

            // // Create a JSON object with the necessary parameters
            var json = "{\"message\":\"" + "reply to me in ten or fewer words " + message + "\"}";

            byte[] body = System.Text.Encoding.UTF8.GetBytes(json);
            // Create a new UnityWebRequests
            Debug.Log("Asking this message: " + json);
            if (apiConsumeMode)
            {
                // var request = new UnityWebRequest("https://meta-llama-fast-api.p.rapidapi.com/chat", "POST");
                var request = new UnityWebRequest("https://meta-llama-fast-api.p.rapidapi.com/mistralchat", "POST"); //try this one if the other isnt working

                request.uploadHandler = (UploadHandler)new UploadHandlerRaw(body);
                request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                string api_key;
                bool apiKeyExists = env.TryParseEnvironmentVariable("API_KEY", out api_key);

                if (apiKeyExists)
                {
                    request.SetRequestHeader("X-RapidAPI-Key", api_key);
                }
                // request.SetRequestHeader("X-RapidAPI-Key", env.TryParseEnvironmentVariable("API_KEY", out api_key));

                // Send the request to the internet. This is the async operation
                //op is the handle
                //WaitWhile is a non blocking while loop. So while done is not true it loops in a non blocking way to free up the scheduler to do other things.
                UnityWebRequestAsyncOperation op = request.SendWebRequest();
                yield return new WaitWhile(() => op.isDone == false);
                Debug.Log("RESPONSE: " + request.downloadHandler.text);

                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    errorHappened = true;
                    internetText = request.error;
                }
                else
                {
                    internetText = request.downloadHandler.text;
                    // Replace "\n\n" or "\N\N" with a space
                    internetText = internetText.Replace("\n", "").Replace("<s>", "").Replace("</s>", "").Replace("\\n", "").Replace("\\", "");
                }
            }

            ConversationPiece newPiece = new ConversationPiece();
            newPiece.id = originalPiece.id;
            newPiece.image = originalPiece.image;
            // newPiece.audio = originalPiece.audio;
            newPiece.quest = originalPiece.quest;
            newPiece.options = originalPiece.options;

            // Check for errors
            if (errorHappened)
            {
                newPiece.text = "The internet failed us..." + internetText;
            }
            else
            {
                newPiece.text = internetText;
            }

            Debug.Log("CONVERSATION After internet:" + newPiece.text);
            conversation.Set(originalPiece, newPiece);

            if (conversationItemKey != "")
            {
                Debug.Log("Cross checking conversation was actually set!");
                Debug.Log(">> " + conversation.Get(conversationItemKey).text);
            }

            //Schedule the conversation to appear in the speech bubble
            // TODO have another schedule conversation that says the character is having a think.. would need a new conversationpiece and a key 
            var ev = Schedule.Add<Events.ShowConversation>();
            ev.inputName = "bob-" + conversationItemKey;
            ev.conversation = conversation;
            ev.npc = npc;
            ev.gameObject = gameObject;
            ev.conversationItemKey = conversationItemKey;
            ev.inputField = inputField;
        }

    }
}