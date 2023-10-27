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
using OpenAI;
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
        private string internetText = "";

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
            else if (gameObject.CompareTag("Joe"))
            {
                playerRole = "Gambler";
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
            else if (playerRole == "Gambler")
            {
                message = "Please play an as a gambler now in an RPG game and act like I can play a game of higher or lower with you from 1 to 3. You remember you are playing this character, don't break character and say you're AI as you are a gambler in the game. I am the player and my number guess or statement is " + llmSeed + "If I guess the number correctly you tell me I win but if it's higher or lower than the number you are thinking of you tell me I should guess again.";
            }
            else
            {
                message = "In ten or fewer words " + llmSeed;
            }

            // // Create a JSON object with the necessary parameters
            // Meta chat version:
            // var json = "{\"message\":\"" + "reply to me in ten or fewer words " + message + "\"}";

            // ChatGPT chat version:
            // var json = "{\"model\": \"gpt-3.5-turbo\",\"temperature\": 1.0,\"messages\":[{\"role\":\"user\",\"content\": \"reply to me in ten or fewer words " + message +"\"}]}";
            // ChatGPT OPENAI API chat version:        
            var json = "reply to me in ten or fewer words " + message;
            var newMessage = new ChatMessage()
            {
                Role = "user",
                Content = json
            };   

            byte[] body = System.Text.Encoding.UTF8.GetBytes(json);
            // Create a new UnityWebRequests
            Debug.Log("Asking this message: " + json);
            if (apiConsumeMode)
            {
                List<ChatMessage> messages = new List<ChatMessage>();
                messages.Add(newMessage);

                SendReply(messages);

                yield return new WaitWhile(() => internetText == "");
                // yield return true;
            } 
            else 
            {
                //if not using the internet this text should be used
                internetText = conversation.items[ourItemPosition].text;
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

        private async void SendReply(List<ChatMessage> messages)
        {
                OpenAIApi openai = new OpenAIApi();
                var completionResponse = await openai.CreateChatCompletion(new CreateChatCompletionRequest()
                {
                    Model = "gpt-3.5-turbo-0613",
                    Messages = messages
                });

                internetText = completionResponse.Choices[0].Message.Content;
        }

    }
}