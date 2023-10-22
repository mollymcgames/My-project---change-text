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
    public class ShowConversation : Event<ShowConversation>
    {
        public NPCController npc;
        public GameObject gameObject;
        public ConversationScript conversation;
        public string conversationItemKey;
        public string inputName = "FirstTime";

        public override void Execute()
        {
            Debug.Log("ShowConversation event now happening..."+inputName);
            Debug.Log("Number of conversation pieces: "+conversation.items.Count);
            Debug.Log("Current conversation item key: "+conversationItemKey);

            ConversationPiece ci;
            //default to first conversation item if no key is specified, else find the right conversation item.
            if (string.IsNullOrEmpty(conversationItemKey))
            {
                Debug.Log("Getting root conversation..");
                ci = conversation.items[0];
            }
            else 
            {
                Debug.Log("Getting specific conversation with key: "+ conversationItemKey);
                ci = conversation.Get(conversationItemKey);
            }

            //if this item contains an unstarted quest, schedule a start quest event for the quest.
            if (ci.quest != null)
            {
                if (!ci.quest.isStarted)
                {
                    Debug.Log("starting new quest");
                    var ev = Schedule.Add<StartQuest>(1);
                    ev.quest = ci.quest;
                    ev.npc = npc;
                }
                if (ci.quest.isFinished && ci.quest.questCompletedConversation != null)
                {
                    ci = ci.quest.questCompletedConversation.items[0];
                }
            }

            //calculate a position above the player's sprite.
            var position = gameObject.transform.position;
            var sr = gameObject.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                position += new Vector3(1.9f, 2 * sr.size.y + (ci.options.Count == 0 ? 0.1f : 0.2f), 0);
            }

            //show the dialog
            Debug.Log("NPC info: " + npc);
            Debug.Log("Sprite position: " + position);
            model.dialog.Show(position, ci.text);
            var animator = gameObject.GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetBool("Talk", true);
                var ev = Schedule.Add<StopTalking>(2);
                ev.animator = animator;
            }
            //REMOVED NPC AUUDIO here
            // if (ci.audio != null)
            // {
            //     UserInterfaceAudio.PlayClip(ci.audio);
            // }

            //speak some gibberish at two speech syllables per word.
            // UserInterfaceAudio.Speak(gameObject.GetInstanceID(), ci.text.Split(' ').Length * 2, 1);

            //if this conversation item has an id, register it in the model.
            Debug.Log("ConversationID: " + ci.id);
            if (!string.IsNullOrEmpty(ci.id))
                model.RegisterConversation(gameObject, ci.id);

            //setup conversation choices, if any.
            if (ci.options.Count == 0)
            {
                //do nothing
            }
            else
            {
                //Create option buttons below the dialog.
                for (var i = 0; i < ci.options.Count; i++)
                {
                    model.dialog.SetButton(i, ci.options[i].text);
                }

                //if user pickes this option, schedule an event to show the new option.
                model.dialog.onButton += (index) =>
                {
                    //hide the old text, so we can display the new.
                    model.dialog.Hide();

                    //This is the id of the next conversation piece.
                    var next = ci.options[index].targetId;

                    //Make sure it actually exists!
                    if (conversation.ContainsKey(next))
                    {
                        Debug.Log("Rescheduling myself");                        
                        //find the conversation piece object and setup a new event with correct parameters.
                        var c = conversation.Get(next);
                        Debug.Log("NEXT key: "+next);


                    //if quest is not finished then use seed text from the input buttons to drive the chat 
                    //but if the quest has finished then use the text thats hard coded in the quest script
                        if (ci.quest != null && ci.quest.isFinished) 
                        {
                            var ev = Schedule.Add<ShowConversation>(0.25f);
                            ev.conversation = conversation;
                            ev.gameObject = gameObject;
                            ev.npc = npc;                            
                            ev.conversationItemKey = ci.options[index].text;

                        } else {
                            var ev = Schedule.Add<Events.GetConversationFromInternet>();
                            ev.conversation = conversation;
                            ev.gameObject = gameObject;
                            ev.npc = npc;
                            ev.conversationItemKey = next;
                            ev.llmSeed = ci.options[index].text;

                        }                                            
                    }
                    else
                    {
                        Debug.LogError($"No conversation with ID:{next}");
                    }
                };

            }

            //if conversation has an icon associated, this will display it.
            model.dialog.SetIcon(ci.image); 
        }

    }
}