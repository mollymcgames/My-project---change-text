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
        bool apiConsumeMode = true;
        
        public TMP_Text textmesh;

        public string prompt = "Your Prompt Here";

        //If using chatgpt
        // //The engine you want to use (keep in mind that it has to be the exact name of the engine)
        // private string model = "text-davinci-003";
        // public float temperature = 0.5f;
        // public int maxTokens = 200;
        


        //the Make request is an asynchronous operation which prevents blocking
        public void GetTheConversation(NPCController npc, ConversationScript conversation, ConversationPiece originalPiece, string conversationItemKey, string llmSeed)
        {
            Debug.Log("Off we go!");
            StartCoroutine(MakeRequest(npc, conversation, originalPiece,conversationItemKey, llmSeed));
            Debug.Log("And back!");
        }        

        IEnumerator MakeRequest(NPCController npc, ConversationScript conversation, ConversationPiece originalPiece, string conversationItemKey, string llmSeed)
        {
            bool errorHappened = false;

            var ourItem = conversation.items.Find(x => x.id.Equals(conversationItemKey)); 
            var ourItemPosition = conversation.items.IndexOf(ourItem);
            if ( ourItemPosition <= 0) {
                ourItemPosition = 0;
            }      
            Debug.Log("Conversation seed: "+ llmSeed);
            //if not using the internet this text should be used
            string internetText = conversation.items[ourItemPosition].text;


            // // Create a JSON object with the necessary parameters
            var json = "{\"message\":\"In ten or fewer words " + llmSeed + "\"}";

            byte[] body = System.Text.Encoding.UTF8.GetBytes(json);
            // Create a new UnityWebRequests
            Debug.Log("Asking this message: "+json);
            if (apiConsumeMode) 
            {
                // var request = new UnityWebRequest("https://meta-llama-fast-api.p.rapidapi.com/chat", "POST");
                var request = new UnityWebRequest("https://meta-llama-fast-api.p.rapidapi.com/mistralchat", "POST"); //try this one if the other isnt working


                request.uploadHandler = (UploadHandler)new UploadHandlerRaw(body);
                request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");


                
            
                string api_key;
                bool apiKeyExists = env.TryParseEnvironmentVariable("API_KEY", out api_key);

                if (apiKeyExists){
                     request.SetRequestHeader("X-RapidAPI-Key",api_key);
                }
                // request.SetRequestHeader("X-RapidAPI-Key", env.TryParseEnvironmentVariable("API_KEY", out api_key));

                // Send the request to the internet. This is the async operation
                //op is the handle
                //WaitWhile is a non blocking while loop. So while done is not true it loops in a non blocking way to free up the scheduler to do other things.
                UnityWebRequestAsyncOperation op = request.SendWebRequest();
                yield return new WaitWhile(() => op.isDone == false);
                Debug.Log("RESPONSE: "+request.downloadHandler.text);

                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    errorHappened = true;
                    internetText = request.error;
                } else {
                    internetText = request.downloadHandler.text;
                    // Replace "\n\n" or "\N\N" with a space
                    internetText = internetText.Replace("\n", "").Replace("<s>", "").Replace("</s>", "").Replace("\\n", "").Replace("\\", "");
                }
            }

            ConversationPiece newPiece = new ConversationPiece();           
            newPiece.id = originalPiece.id;
            newPiece.image = originalPiece.image;
            newPiece.audio = originalPiece.audio;
            newPiece.quest = originalPiece.quest;
            newPiece.options = originalPiece.options;

            // Check for errors
            if (errorHappened)
            {
                newPiece.text = "The internet failed us..."+internetText;
            }
            else
            {
                newPiece.text = internetText;
            }

            Debug.Log("CONVERSATION After internet:"+newPiece.text);
            conversation.Set(originalPiece, newPiece);

            if ( conversationItemKey != "") 
            {
                Debug.Log("Cross checking conversation was actually set!");
                Debug.Log(">> "+conversation.Get(conversationItemKey).text);
            }

            //Schedule the conversation to appear in the speech bubble
            // TODO have another schedule conversation that says the character is having a think.. would need a new conversationpiece and a key 
            var ev = Schedule.Add<Events.ShowConversation>();
            ev.inputName = "bob-"+conversationItemKey;
            ev.conversation = conversation;
            ev.npc = npc;
            ev.gameObject = gameObject;
            ev.conversationItemKey = conversationItemKey;
        }

    }
}