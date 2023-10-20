using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGM.Gameplay;
using RPGM.Core;

public class PictureInput : MonoBehaviour
{
    private string pictureInput;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void CapturePictureInput(string s) 
    {
        pictureInput = s;
        Debug.Log("Input string: "+pictureInput);

        GameModel model = Schedule.GetModel<GameModel>();
        model.pictureInput = pictureInput; 

    }
}
