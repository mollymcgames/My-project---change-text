using System.Collections;
using System.Collections.Generic;
using RPGM.Gameplay;
using UnityEditor;
using UnityEngine;

public class GSEnemyController : MonoBehaviour
{
    private Animator myAnim;
    private Transform target;
    [SerializeField]
    private float speed;
    [SerializeField]
    private float maxRange; // The maximum range at which the enemy can detect the target
    [SerializeField]
    private float minRange; // The minimum range at which the enemy can detect the target
    // Start is called before the first frame update

    private bool shouldChase = false; // Flag to determine whether the enemy should chase the target

    void Start()
    {
        myAnim = GetComponent<Animator>();
        target = FindFirstObjectByType<CharacterController2D>().transform; //follow the player

        // target = GameObject.FindWithTag("Gloria")?.transform; //it can follow gloria but not the object

        //move this block to the Update if doing the DALLEE check for image
        // target = GameObject.FindWithTag("giftPicture")?.transform; // Find the target with the "giftPicture" tag
        if (target == null)
        {
            shouldChase = false; // If the target is not found, don't chase
            Debug.Log("Target with the 'giftPicture' tag not found!");

        }
        else
        {
            shouldChase = true; // If the target is found, set shouldChase to true
            Debug.Log("Found target with the 'giftPicture' tag!");

        }        

    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(target.position, transform.position) <= maxRange && Vector3.Distance(target.position, transform.position) >= minRange ) // Check if the distance between the enemy and the target is less than the range
        {
            FollowPlayer(); // Call the FollowPlayer function
            shouldChase = true; // If the distance is less than the range, set shouldChase to true
        }
        else
        {
            myAnim.SetBool("isMoving", false);
            shouldChase = false; // If the distance is greater than the range, set shouldChase to false
        }
    }

    public void FollowPlayer()
    {
        if (shouldChase && target != null) // Check if shouldChase is true and if the target is not null
        {
            myAnim.SetBool("isMoving", true);
            myAnim.SetFloat("moveX", (target.position.x - transform.position.x));
            myAnim.SetFloat("moveY", (target.position.y - transform.position.y));
            transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
        }
    }    
    // { //this is the code that makes the enemy follow the player
    //     transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
    // }
}
