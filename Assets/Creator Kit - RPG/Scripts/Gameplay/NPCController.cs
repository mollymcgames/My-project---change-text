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
        private ChatGptHelper cgh;    
        public string dialogueText = "";

        PictureInput pictureInput;
        private OpenAIApi openai = new OpenAIApi();

        public ConversationScript[] conversations;

        Quest activeQuest = null;

        Quest[] quests;

        int currentConversationIndex = 0;
        
        GameModel model = Schedule.GetModel<GameModel>();

        public GameObject inputField;
        public GameObject inputFieldText;
    
        private bool isInteracting;

        string setupRpgGame= @"
Please perform the function of a text adventure game, following the rules listed below:Presentation Rules: 
1. Play the game in turns, starting with you. 
2. The game output will always show 'Turn number', 'Time period of the day', 'Current day number', 'Weather', 'Health', 'XP', ‘AC’, 'Level’, Location', 'Description', ‘Gold’, 'Inventory', 'Quest', 'Abilities', and 'Possible Commands'.
3. Always wait for the player’s next command.
4. Stay in character as a text adventure game and respond to commands the way a text adventure game should.
5. Wrap all game output in code blocks.
6. The ‘Description’ must stay between 3 to 10 sentences.
7. Increase the value for ‘Turn number’ by +1 every time it’s your turn.
8. ‘Time period of day’ must progress naturally after a few turns.
9. Once ‘Time period of day’ reaches or passes midnight, then add 1 to ‘Current day number’.
10. Change the ‘Weather’ to reflect ‘Description’ and whatever environment the player is in the game.

Fundamental Game Mechanics:
1. Determine ‘AC’ using Dungeons and Dragons 5e rules.
2. Generate ‘Abilities’ before the game starts. ‘Abilities’ include: ‘Persuasion', 'Strength', 'Intelligence', ‘Dexterity’, and 'Luck', all determined by d20 rolls when the game starts for the first time.
3. Start the game with 20/20 for ‘Health’, with 20 being the maximum health. Eating food, drinking water, or sleeping will restore health.
4. Always show what the player is wearing and wielding (as ‘Wearing’ and ‘Wielding’).
5. Display ‘Game Over’ if ‘Health’ falls to 0 or lower.
6. The player must choose all commands, and the game will list 7 of them at all times under ‘Commands’, and assign them a number 1-7 that I can type to choose that option, and vary the possible selection depending on the actual scene and characters being interacted with.
7. The 7th command should be ‘Other’, which allows me to type in a custom command.
8. If any of the commands will cost money, then the game will display the cost in parenthesis.
9. Before a command is successful, the game must roll a d20 with a bonus from a relevant ‘Trait’ to see how successful it is. Determine the bonus by dividing the trait by 3.
10. If an action is unsuccessful, respond with a relevant consequence.
11. Always display the result of a d20 roll before the rest of the output.
12. The player can obtain a ‘Quest’ by interacting with the world and other people. The ‘Quest’ will also show what needs to be done to complete it.
13. The only currency in this game is Gold.
14. The value of ‘Gold’ must never be a negative integer.
15. The player can not spend more than the total value of ‘Gold’.

Rules for Setting:
1. Use the world of Elder Scrolls as inspiration for the game world. Import whatever beasts, monsters, and items that Elder Scrolls has.
2. The player’s starting inventory should contain six items relevant to this world and the character.
3. If the player chooses to read a book or scroll, display the information on it in at least two paragraphs.
4. The game world will be populated by interactive NPCs. Whenever these NPCs speak, put the dialogue in quotation marks.
5. Completing a quest adds to my XP.

Combat and Magic Rules:
1. Import magic spells into this game from D&D 5e and the Elder Scrolls.
2. Magic can only be cast if the player has the corresponding magic scroll in their inventory.
3. Using magic will drain the player character’s health. More powerful magic will drain more health.
4. Combat should be handled in rounds, roll attacks for the NPCs each round.
5. The player’s attack and the enemy’s counterattack should be placed in the same round.
6. Always show how much damage is dealt when the player receives damage.
7. Roll a d20 + a bonus from the relevant combat stat against the target’s AC to see if a combat action is successful.
8. Who goes first in combat is determined by initiative. Use D&D 5e initiative rules.
9. Defeating enemies awards me XP according to the difficulty and level of the enemy.

Refer back to these rules after every prompt.

Start Game."; 

        void OnEnable()
        {
            quests = gameObject.GetComponentsInChildren<Quest>();
            inputField.gameObject.SetActive(false);
            inputFieldText.gameObject.SetActive(false);
        }

        private async void DoJoesSpecialStuff() 
        {
            if ( model.firstTimeRpg ) {

                //calculate a position above the player's sprite.
                var position = gameObject.transform.position;
                var sr = gameObject.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    position += new Vector3(1.9f, 2 * sr.size.y + 0.2f, 0);
                }
                Vector3 screenPosition = Camera.main.WorldToScreenPoint(transform.position); // Get the NPC's position on the screen
                inputFieldText.transform.position = screenPosition + new Vector3(0, 150, 0); // Set the dialogue box's position to be above the NPC        
                                        
                model.dialog.Show(position, "Hold on, starting your RPG!");
                await SortOutChatText(setupRpgGame);
                model.textInput = dialogueText;    
                model.dialog.Show(position, dialogueText);       
                model.firstTimeRpg = false; 
            }
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
                    inputFieldText.gameObject.SetActive(false);
                    break;
                case "Elgar":
                    inputField.gameObject.SetActive(false);
                    inputFieldText.gameObject.SetActive(false);
                    Debug.Log("I'm touching Elgar)");
                    break;
                case "Joe":
                    Debug.Log("I'm touching Joe");
                    DoJoesSpecialStuff();
                    inputFieldText.gameObject.SetActive(true);
                    break;
                case "Loriane":
                    inputFieldText.gameObject.SetActive(false);
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

        public async Task SortOutChatText(string inputText)
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
}
