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
    public class ShowGPTConversation : Event<ShowConversation>
    {
        public NPCController npc;
        public GameObject gameObject;
        public ConversationScript conversation;
        public GameObject inputField;


        public override void Execute()
        {

            //calculate a position above the player's sprite.
            var position = gameObject.transform.position;
            var sr = gameObject.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                position += new Vector3(1.9f, 2 * sr.size.y + 0.2f, 0);
            }


            //show the dialog
            Debug.Log("NPC info: " + npc);
            Debug.Log("Sprite position: " + position);
            model.dialog.Show(position, model.textInput);
            var animator = gameObject.GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetBool("Talk", true);
                var ev = Schedule.Add<StopTalking>(2);
                ev.animator = animator;
            }

            //Create option buttons below the dialog.
            // for (var i = 0; i < 2; i++)
            // {
            //     model.dialog.SetButton(i, "button");
            // }

            // //if user pickes this option, schedule an event to show the new option.
            // model.dialog.onButton += (index) =>
            // {
            //     //hide the old text, so we can display the new.
            //     model.dialog.Hide();



            //     //Make sure it actually exists!
            //     if (conversation.ContainsKey(next))
            //     {
            //         Debug.Log("Rescheduling myself");
            //         //find the conversation piece object and setup a new event with correct parameters.
            //         var c = conversation.Get(next);
            //         Debug.Log("NEXT key: " + next);


            //         //if quest is not finished then use seed text from the input buttons to drive the chat 
            //         //but if the quest has finished then use the text thats hard coded in the quest script
            //         if (ci.quest != null && ci.quest.isFinished)
            //         {
            //             var ev = Schedule.Add<ShowConversation>(0.25f);
            //             ev.conversation = conversation;
            //             ev.gameObject = gameObject;
            //             ev.npc = npc;
            //             ev.conversationItemKey = ci.options[index].text;

            //         }
            //         else
            //         {
            //             var ev = Schedule.Add<Events.GetConversationFromInternet>();
            //             ev.conversation = conversation;
            //             ev.gameObject = gameObject;
            //             ev.npc = npc;
            //             ev.conversationItemKey = next;
            //             ev.llmSeed = ci.options[index].text;

            //         }
            //     }
            //     else
            //     {
            //         Debug.LogError($"No conversation with ID:{next}");
            //     }
            // };



            // else inputField.gameObject.SetActive(false);

            //if conversation has an icon associated, this will display it.
            // model.dialog.SetIcon(ci.image);
        }

    }
}