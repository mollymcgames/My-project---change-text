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
    private float range;
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
        FollowPlayer();
    }

    public void FollowPlayer()
    {
        if (shouldChase && target != null) // Check if shouldChase is true and if the target is not null
        {
            transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
        }
    }    
    // { //this is the code that makes the enemy follow the player
    //     transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
    // }
}
