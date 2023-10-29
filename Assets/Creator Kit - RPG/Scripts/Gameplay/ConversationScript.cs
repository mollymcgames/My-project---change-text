using System.Collections.Generic;
using RPGM.Gameplay;
using UnityEditor.Rendering;
using UnityEngine;

namespace RPGM.Gameplay
{
    /// <summary>
    /// A single conversation container.
    /// </summary>
    public class ConversationScript : MonoBehaviour, ISerializationCallbackReceiver
    {

        [HideInInspector] [SerializeField] public List<ConversationPiece> items = new List<ConversationPiece>();
        Dictionary<string, ConversationPiece> index = new Dictionary<string, ConversationPiece>();

        public bool ContainsKey(string id)
        {
            return index.ContainsKey(id);
        }

        public ConversationPiece Get(string id)
        {
            Debug.Log("Getting ConversationPiece with ID:"+id);
            return index[id];
        }

        public void Add(ConversationPiece conversationPiece)
        {
            Debug.Log("Conversation ADD just got called...");
            items.Add(conversationPiece);
        }

        public void Set(ConversationPiece originalConversationPiece, ConversationPiece newConversationPiece)
        {
            if (originalConversationPiece.id != newConversationPiece.id)
            {
                Debug.Log("+++UNEQUAL Updating Conversation with new words");
                foreach (var i in items)
                {
                    var options = i.options;
                    for (var j = 0; j < options.Count; j++)
                    {
                        if (options[j].targetId == originalConversationPiece.id)
                        {
                            var c = options[j];
                            c.targetId = newConversationPiece.id;
                            options[j] = c;
                        }
                    }
                }
            }
            for (var i = 0; i < items.Count; i++)
            {
                if (items[i].id == originalConversationPiece.id)
                {
                    string cleanedText = newConversationPiece.text.Replace("\"", "");
                    Debug.Log("+++Updating Conversation with new words:"+newConversationPiece.text);
                    newConversationPiece.text = cleanedText;
                    items[i] = newConversationPiece;
                    break;
                }
            }

            refreshIndex();
        }

        private void refreshIndex()
        {
            index.Clear();
            foreach (var i in items)
                index[i.id] = i;   
        }

        public void Delete(string id)
        {
            for (var i = 0; i < items.Count; i++)
            {
                if (items[i].id == id)
                {
                    items.RemoveAt(i);
                    break;
                }
            }
        }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            refreshIndex();
        }
    }
}