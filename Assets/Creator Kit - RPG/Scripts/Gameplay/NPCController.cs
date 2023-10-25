using RPGM.Core;
using RPGM.Gameplay;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using OpenAI;
using UnityEngine.Networking;
using System.Threading.Tasks;
using TMPro;


namespace RPGM.Gameplay
{
    /// <summary>
    /// Main class for implementing NPC game objects.
    /// </summary>
    public class NPCController : MonoBehaviour
    {
        // bool REAL_DALLE = true; //TODO: Set this to true when you want to use Dalle for real or false otherwise
        // bool REAL_DALLE = true; //unused for now

        PictureInput pictureInput;
        private OpenAIApi openai = new OpenAIApi();

        public ConversationScript[] conversations;

        Quest activeQuest = null;

        Quest[] quests;

        int currentConversationIndex = 0;
        
        GameModel model = Schedule.GetModel<GameModel>();

        public GameObject inputField;
    
        private bool isInteracting;

        void OnEnable()
        {
            quests = gameObject.GetComponentsInChildren<Quest>();
            inputField.gameObject.SetActive(false);
        }

        public void OnCollisionEnter2D(Collision2D collision)
        {
            string conversationKey = "";
            Debug.Log("Ouch, I collided");
            Debug.Log("Picture words: "+ model.pictureInput != "" ? model.pictureInput : "NO PICTURE INPUT SET");

            switch (this.tag) {
                case "Gloria":
                    Debug.Log("I'm touching Gloria");
                    inputField.gameObject.SetActive(false);
                    break;
                case "Elgar":
                    inputField.gameObject.SetActive(false);
                    Debug.Log("I'm touching Elgar)");
                    break;
                case "Joe":
                    Debug.Log("I'm touching Joe");
                    switch ( currentConversationIndex) {
                        case(0):
                            conversationKey = "";
                            break;
                        case(1):
                            conversationKey = "1.1";
                            break;
                        case(2):
                            conversationKey = "1.2";
                            break;
                        default:
                            conversationKey = "";
                            break;
                    }               
                    break;
                case "Loriane":
                    inputField.gameObject.SetActive(false);
                    switch ( currentConversationIndex) {
                        case(0):
                            conversationKey = "";
                            break;
                        case(1):
                            conversationKey = "1.1";
                            break;
                        case(2):
                            conversationKey = "1.2";
                            break;
                        default:
                            conversationKey = "";
                            break;
                    }               
                    break;
                default:
                    Debug.Log("I'm touching pretty much anything!!");
                    break;
            }
            var c = GetConversation();
            if (c != null)
            {
                Debug.Log("About to get conversation for key: "+conversationKey);
                // Going to have to pass in here details about what collision happened, as these are needed by "ShowConversation" later.
                // This is done by setting some values AFTER the schedule has been added.
                var ev = Schedule.Add<Events.GetConversationFromInternet>();
                ev.conversation = c;
                ev.npc = this;
                ev.gameObject = gameObject;
                // Remember this can be different depending on if the NPC has a quest or not!
                ev.conversationItemKey = conversationKey; 
                ev.llmSeed = c.items[0].text;
                ev.inputField = inputField;
            }
        }

        public void CompleteQuest(Quest q)
        {
            if (activeQuest != q) throw new System.Exception("Completed quest is not the active quest.");
            foreach (var i in activeQuest.requiredItems)
            {
                model.RemoveInventoryItem(i.item, i.count);
            }
            activeQuest.RewardItemsToPlayer();
            activeQuest.OnFinishQuest();
            activeQuest = null;
        }

        public void StartQuest(Quest q)
        {
            if (activeQuest != null) throw new System.Exception("Only one quest should be active.");
            activeQuest = q;
        }

        ConversationScript GetConversation()
        {
            Debug.Log("currentConversationIndex is now: "+currentConversationIndex);
            Debug.Log("Quest length: "+ quests.Length);
            if (activeQuest == null && quests.Length > 0) { // quests MIGHT have to check for a count >0 rather than NULL
                Debug.Log("Handling conversation for NPC WITH a quest");
                return conversations[0];
            } 
            else if ( quests.Length <= 0 ) 
            {
                Debug.Log("Handling conversation for NPC without a quest. Current index: "+currentConversationIndex);
                currentConversationIndex++;
                return conversations[0]; 
            }

            foreach (var q in quests)
            {
                if (q == activeQuest)
                {
                    if (q.IsQuestComplete())
                    {
                        CompleteQuest(q);
                        return q.questCompletedConversation;
                    }
                    return q.questInProgressConversation;
                }
            }
            return null;
        }
    }
}
