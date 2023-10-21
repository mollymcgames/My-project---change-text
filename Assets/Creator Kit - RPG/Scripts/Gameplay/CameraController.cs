using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RPGM.Gameplay
{
    /// <summary>
    /// A simple camera follower class. It saves the offset from the
    ///  focus position when started, and preserves that offset when following the focus.
    /// </summary>
    /// 


    public class CameraController : MonoBehaviour
    {
        public Transform focus;
        public float smoothTime = 2;
        public Vector2 minPosition; //get the X and Y values from the editor
        public Vector2 maxPosition;        

        Vector3 offset;

        void Awake()
        {
            offset = focus.position - transform.position;
        }

        void LateUpdate()
        {
            if(transform.position != focus.position)
            {
                Vector3 targetPosition = new Vector3(focus.position.x, focus.position.y, transform.position.z);
                targetPosition.x = Mathf.Clamp(targetPosition.x, minPosition.x, maxPosition.x);
                targetPosition.y = Mathf.Clamp(targetPosition.y, minPosition.y, maxPosition.y);
                
                transform.position = Vector3.Lerp(transform.position, targetPosition, smoothTime);
            
            }
            // transform.position = Vector3.Lerp(transform.position, focus.position - offset, Time.deltaTime * smoothTime);
        }
    }
}