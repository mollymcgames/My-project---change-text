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
        bool FAKE_DALLE = false; //TODO: Set this to false when you want to use Dalle for real or true otherwise
        // bool REAL_DALLE = true; //unused for now

        PictureInput pictureInput;
        private OpenAIApi openai = new OpenAIApi();

        public ConversationScript[] conversations;

        Quest activeQuest = null;

        Quest[] quests;

        int currentConversationIndex = 0;
        
        GameModel model = Schedule.GetModel<GameModel>();

        public GameObject canvas;
        public GameObject inputField;
    
        private bool isInteracting;
        // public TMP_InputField inputField;

        

        private byte[] unityPngBytes = new byte[] {
			0x89,0x50,0x4E,0x47,0x0D,0x0A,0x1A,0x0A,0x00,0x00,0x00,0x0D,0x49,0x48,0x44,0x52,
			0x00,0x00,0x00,0x40,0x00,0x00,0x00,0x40,0x08,0x00,0x00,0x00,0x00,0x8F,0x02,0x2E,
			0x02,0x00,0x00,0x01,0x57,0x49,0x44,0x41,0x54,0x78,0x01,0xA5,0x57,0xD1,0xAD,0xC4,
			0x30,0x08,0x83,0x81,0x32,0x4A,0x66,0xC9,0x36,0x99,0x85,0x45,0xBC,0x4E,0x74,0xBD,
			0x8F,0x9E,0x5B,0xD4,0xE8,0xF1,0x6A,0x7F,0xDD,0x29,0xB2,0x55,0x0C,0x24,0x60,0xEB,
			0x0D,0x30,0xE7,0xF9,0xF3,0x85,0x40,0x74,0x3F,0xF0,0x52,0x00,0xC3,0x0F,0xBC,0x14,
			0xC0,0xF4,0x0B,0xF0,0x3F,0x01,0x44,0xF3,0x3B,0x3A,0x05,0x8A,0x41,0x67,0x14,0x05,
			0x18,0x74,0x06,0x4A,0x02,0xBE,0x47,0x54,0x04,0x86,0xEF,0xD1,0x0A,0x02,0xF0,0x84,
			0xD9,0x9D,0x28,0x08,0xDC,0x9C,0x1F,0x48,0x21,0xE1,0x4F,0x01,0xDC,0xC9,0x07,0xC2,
			0x2F,0x98,0x49,0x60,0xE7,0x60,0xC7,0xCE,0xD3,0x9D,0x00,0x22,0x02,0x07,0xFA,0x41,
			0x8E,0x27,0x4F,0x31,0x37,0x02,0xF9,0xC3,0xF1,0x7C,0xD2,0x16,0x2E,0xE7,0xB6,0xE5,
			0xB7,0x9D,0xA7,0xBF,0x50,0x06,0x05,0x4A,0x7C,0xD0,0x3B,0x4A,0x2D,0x2B,0xF3,0x97,
			0x93,0x35,0x77,0x02,0xB8,0x3A,0x9C,0x30,0x2F,0x81,0x83,0xD5,0x6C,0x55,0xFE,0xBA,
			0x7D,0x19,0x5B,0xDA,0xAA,0xFC,0xCE,0x0F,0xE0,0xBF,0x53,0xA0,0xC0,0x07,0x8D,0xFF,
			0x82,0x89,0xB4,0x1A,0x7F,0xE5,0xA3,0x5F,0x46,0xAC,0xC6,0x0F,0xBA,0x96,0x1C,0xB1,
			0x12,0x7F,0xE5,0x33,0x26,0xD2,0x4A,0xFC,0x41,0x07,0xB3,0x09,0x56,0xE1,0xE3,0xA1,
			0xB8,0xCE,0x3C,0x5A,0x81,0xBF,0xDA,0x43,0x73,0x75,0xA6,0x71,0xDB,0x7F,0x0F,0x29,
			0x24,0x82,0x95,0x08,0xAF,0x21,0xC9,0x9E,0xBD,0x50,0xE6,0x47,0x12,0x38,0xEF,0x03,
			0x78,0x11,0x2B,0x61,0xB4,0xA5,0x0B,0xE8,0x21,0xE8,0x26,0xEA,0x69,0xAC,0x17,0x12,
			0x0F,0x73,0x21,0x29,0xA5,0x2C,0x37,0x93,0xDE,0xCE,0xFA,0x85,0xA2,0x5F,0x69,0xFA,
			0xA5,0xAA,0x5F,0xEB,0xFA,0xC3,0xA2,0x3F,0x6D,0xFA,0xE3,0xAA,0x3F,0xEF,0xFA,0x80,
			0xA1,0x8F,0x38,0x04,0xE2,0x8B,0xD7,0x43,0x96,0x3E,0xE6,0xE9,0x83,0x26,0xE1,0xC2,
			0xA8,0x2B,0x0C,0xDB,0xC2,0xB8,0x2F,0x2C,0x1C,0xC2,0xCA,0x23,0x2D,0x5D,0xFA,0xDA,
			0xA7,0x2F,0x9E,0xFA,0xEA,0xAB,0x2F,0xDF,0xF2,0xFA,0xFF,0x01,0x1A,0x18,0x53,0x83,
			0xC1,0x4E,0x14,0x1B,0x00,0x00,0x00,0x00,0x49,0x45,0x4E,0x44,0xAE,0x42,0x60,0x82,
		};

        void OnEnable()
        {
            quests = gameObject.GetComponentsInChildren<Quest>();
            inputField.gameObject.SetActive(false);
        }

        private void AddDalleImageToScreen (string pictureInput, bool fakeIt) 
        {
            // Create a new Texture2D object (for holding the generated picture) and then ask Dalle to make it.
            Texture2D texture = new Texture2D(2, 2);
            // @TODO The text here would come from an input window when the character meets the correct NPC.
            // @TODO SOmething is too slow in SendImageRequest - the picture takes ages to appear.
            // "A silver rocket with huge flaps against a firey sunset"
            if ( fakeIt == FAKE_DALLE) {
                SendImageRequest(texture, pictureInput);
            } 
            else 
            {
                texture.LoadImage(unityPngBytes);   
            }

            // When SendImageRequest is done, create a new IMAGE (which has to be asssociated with a TRANSFORM...don't ask....it works....) on our CANVAS
            // So first, we got to get a handle on our Canvas...then we create a new GameObject called "giftPicture"
            canvas = GameObject.Find("Canvas");
            GameObject imgObject = new GameObject("giftPicture");

            // Create the Transform object and add it to our parent canvas
            RectTransform trans = imgObject.AddComponent<RectTransform>();
            trans.transform.SetParent(canvas.transform);
            trans.localScale = Vector3.one;
            trans.anchoredPosition = new Vector2(0f, 0f); // setting position, will be on center
            trans.sizeDelta= new Vector2(250, 350); // custom size

            // Then finally, take the Texture2D object which as our picture in, and place it inside a new IMAGE object.
            Image image = imgObject.AddComponent<Image>();
            image.sprite = Sprite.Create(texture, new Rect(0, 0, 2, 2), Vector2.zero, 1f);
            imgObject.transform.SetParent(canvas.transform);


            //ONLY DISPLAY FOR 2 SECONDS //OK IN HINDSIGHT IT TAKES 2 SECONDS FOR THE IMAGE TO EVEN LOAD SO THIS IS A BIT POINTLESS
            //TIME NEEDS TO BE MORE LIKE 7-10 SECONDS 
            StartCoroutine(DisplayImageForDuration(11f));
        }

        private IEnumerator DisplayImageForDuration(float duration){
            yield return new WaitForSeconds(duration);
            //remove the image from the screen
            GameObject imageObject = GameObject.Find("giftPicture"); ///TODO: This is a bit of a hack, as we're assuming there's only one image on the screen at a time.
            if (imageObject != null)
            {
                Destroy(imageObject);
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
                case "Willy":
                    inputField.gameObject.SetActive(true);
                    if (model.pictureInput == "")
                    {
                        Debug.Log("Not doing a Dalle picture quite yet...");
                    }
                    else
                    {
                        Debug.Log("One Dalle coming up!");
                        AddDalleImageToScreen(model.pictureInput, this.FAKE_DALLE); //only show for 2s
                    }
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
            }
        }

        IEnumerator MakeRequest(OpenAI.CreateImageResponse response)
        {
            while (true) 
            {
                yield return new WaitWhile(() => response.Data != null);
            }
        }

        private async void SendImageRequest(Texture2D texture, string imageDescription)
        {
            var openai = new OpenAIApi();
            
            Debug.Log("Asking for a picture to be drawn....");
            OpenAI.CreateImageResponse response = await openai.CreateImage(new CreateImageRequest
            {
                Prompt = imageDescription,
                Size = ImageSize.Size256
            });

            StartCoroutine(MakeRequest(response));
            
            Debug.Log("Picture drawn....");

            if (response.Data != null && response.Data.Count > 0)
            {
                using(var request = new UnityWebRequest(response.Data[0].Url))
                {
                    request.downloadHandler = new DownloadHandlerBuffer();
                    request.SetRequestHeader("Access-Control-Allow-Origin", "*");
                    await request.SendWebRequest(); // Send the request // @TODO This is a blocking call, so we should probably use a coroutine here. //might not need await

                    while (!request.isDone) await Task.Yield();

                    texture.LoadImage(request.downloadHandler.data);                    
                }
            }
            else
            {
                Debug.LogWarning("No image was created from this prompt.");
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
