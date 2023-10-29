using System.Collections;
using System.Collections.Generic;
using RPGM.Core;
using RPGM.Gameplay;
using RPGM.UI;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;

namespace RPGM.Events
{
    /// <summary>
    /// This event will start a conversation with an NPC using a conversation script.
    /// </summary>
    /// <typeparam name="ShowConversation"></typeparam>
    public class GetConversationFromInternet : Event<GetConversationFromInternet>
    {
        // Going to have to create some variables here that contain details about what collision happened, as these are needed by "ShowConversation" later.
        // public ConversationScript[] conversations;
        public ConversationScript conversation;
        public string conversationItemKey;
        public NPCController npc;
        public GameObject gameObject;
        Quest activeQuest = null;
        public GameObject inputField;

        public string llmSeed;


        public override void Execute()
        {            

            Debug.Log("CONVERSATION ITEM ID USING KEY: "+conversation.items.Find(x => x.id.Equals(conversationItemKey)).id); //Key is 1.1
            var ourItem = conversation.items.Find(x => x.id.Equals(conversationItemKey)); 
            var ourItemPosition = conversation.items.IndexOf(ourItem);
            Debug.Log("CONVERSATION ITEM POSITION: "+ourItemPosition);
            if ( ourItemPosition <= 0 || ourItemPosition >= conversation.items.Count) { 
                ourItemPosition = 0;
            }

            ConversationPiece originalPiece = conversation.items[ourItemPosition];            
            GetConversation netConversation = gameObject.AddComponent<GetConversation>();
            netConversation.GetTheConversation(npc, conversation, originalPiece, conversationItemKey, llmSeed, inputField); //kicks off the conversation
        }

    }
}

