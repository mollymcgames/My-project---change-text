using UnityEngine;
using RPGM.Core;
using RPGM.Gameplay;

namespace RPGM.Gameplay
{
    /// <summary>
    /// The global game controller. It contains the game model and executes the schedule.
    /// </summary>
    public class GameController : MonoBehaviour
    {
        //The reference is shared where needed, and Unity will deserialize
        //over the shared reference, rather than create a new instance.
        //To preserve this behaviour, this script must be deserialized last.
        public GameModel model;

        public GameObject inputField;


        protected virtual void OnEnable()
        {
            Debug.Log("GameController Setting Model...");
            Schedule.SetModel<GameModel>(model);
        }

        protected virtual void Update()
        {
            Schedule.Tick();
        }
    }
}