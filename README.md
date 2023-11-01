# Game Development With Advanced AI Tools: RPG Game
---

## Unity setup
The game was written using Unity v2023.1.2f1 however it will 
automatically convert and run in newer versions (tested using Unity v2023.2.0b15)

Simply add this source folder as a new poject "from disk" using Unity Hub and then open it.

## OpenAI setup
The game makes use of OpenAI and so needs an OpenAI authentication token.
It will not work without a token.
Get your API token here: https://platform.openai.com/account/api-keys
Once you have it, in your Windows "user" directory, create a `.openai` folder and
in that create a `auth.json` file and paste in your key.
e.g. `C:\Users\bob\.openai\auth.json`
```
{
	"api_key": "aa-bbcc123123123etc",
	"organization": "org-aabb123123etc"
}
```
Install the required OpenAI package following instructions here:
https://github.com/srcnalt/OpenAI-Unity

## Unity build
The default build options are suitable for this game.  It's necessary to create a 
folder to store the build executable assets within, so do this first.
e.g. `C:\Games\RPG`

As no build options are required, in essence it's possible to go direct to "Build and Run" 
(remember the first build might take a while) and direct the output to the
folder created for your executable assets.


## Gameplay
Control the player using the keyboard arrow keys.
Interact with the Archer, Wizard and Painter by colliding into them. Select your response by clicking on the text option with the arrow keys and then pressing SPACE to continue.

When you collide with the artist type what you want painted into the input boxes and wait for a picture to appear. This might take a minute or two to appear.

Interact with the Dungeon Master by waiting for his game level descriptions, and then typing your choice into the input box.
Bear in mind the dialogue can take a while to appear!

Have fun!


## Troubleshooting

### Issues with Build
If there are issues with making a build it might require the following lines in Scheudle in `SetModel` commented out

```
        static public void SetModel<T>(T instance) where T : class, new()
        {
            Debug.Log("Schedule setting model...");
            // if ( modelSetAlready == false) 
            // {
            //     modelSetAlready = true;
            var singleton = InstanceRegister<T>.instance;
            foreach (var fi in typeof(T).GetFields())
            {
                fi.SetValue(singleton, fi.GetValue(instance));
            }
            // } else {
            //     Debug.Log("No need to set model twice");
            // }
            // Debug.Log("Model set done");
        }
```
