# How to setup project for development
This project is using git lfs because the enterprise github instance limits file size to 50mb. The files configured for git lfs are in .gitattributes.

You will have to install git lfs to work with this project. See https://git-lfs.com/

# Start Scene File
`Menu` is the scene to start with. It will redirect the user to the scene `Level 1` when they hit play. Finishing Level 1 will load level 2, finishing level 2 will load Level 3.

# How to play and key parts
## How to Play
See in game instructions for how to play. To summarize, use WASD to move, while looking with the mouse. Press Shift to run, F to open doors, pickup rocks and grenades, and interact with consoles, and press T to throw rocks to distract the smaller patrolling guards and G for grenades to disable guards (both patrolling and large guards). Press Escape to see the instructions, or exit the game.

The goal is to free all the prisoners, and then escape to the extraction point, a green circle located in the warehouse. Getting caught will send you back to the start and reset your progress.

## Key Parts
* Press F to open doors throughout the game. Some doors are automatic.
* There are rocks that can be thrown to distract guards throughout the level, including 1 near the begining of level 1.
* There are 2 types of guards. One patrols and has a radius where they will see you and cause the game to end, and the other chases the player when the player is in the room.
* There are prisoners locked behind doors. You can press F to open the door and free them. Before they are freed, they pace the cells using a navagent. Once freed, they flee and disappear.
* There are stun grenades throughout the game, including 1 near the start of level 1, and many near the start of level 2.
* There are Robot Control Consoles in level 2 that allow the player to press F to interact with to disable the guard in the same room.
* There is an extraction zone at the end of the level that will end the level if you have freed the prisoners, or prompt you to finish doing so.

# Problem Areas
No major problem areas we're aware of.


# Manifest
Kevin Aiken - Implemented the pause menu, popup instructions at the beginning of the level, created the prisoner AI's, added a prisoner display, and implemented basic throwing shown in the demo. For the final he added the laptops that disable guards when interacted with, the animation to interact with the laptop, and added the gameover menu and implemented the level resetting on game over.
C# scripts:
Author and primary contributor for PrisonerController.cs, PauseMenu.cs, PopupInstructions.cs, GameOver.cs, and LaptopController.cs.
Contributed to PlayerController.cs, adding prisoner logic to count and display prisoners remaining, and block user from finishing before rescuing all of them, as well as interacting with the laptop to disable guards, and the gameover logic. 


Clayton - Implemented the player character and was the primary author of PlayerController.cs. Set up the camera, added menu and game music, and the rock pickup sound. Implemented rock pickup animation and scripting. Also added same logic but for the grenade pickup. In the same script added ability to reset inventories and objects when entering a new level. Setup the animation for the player character, including the blendtree for movement.
Added grenade pickup functionality. Added next level menu and the Game Complete menu (NextLevelMenu.cs). Created Level 2. Added Pause.cs for pause functionality when the next level menu and game complete menu are open. Created FreeLook3rdPerson.cs for hiding the cursor when clicked in game. Besides this, various bugfixes in a variety of scripts/game objects.


Nico - Implemented the start game menu. Updated the pause menu to look better and dynamically respond to different screen resolutions. Created animation for throwing an object at guard and knocking out gaurd. 
Updates Popupinstructions.cs and playintructrions.cs to work with new animations and to create a cleaner UI experience.
Created ThrowRock.cs to work with the throwing rock animation. Created RockImpact.cs to go with the rock animation as well.
Created gamestarter.cs script for the starting menu. GameQuitter.cs to get a working exit game button to work. 
Updated SimpleGuard.cs to disable FOV when hit by stun grenade. ThrowStunGrenade.cs created to manage the collision with the guards and for knocking them out. 
Adding in TriggerParticleEffects.cs to enable particle effects when grenade hits the player.
Updated chasing guard enumeration states to get distracted by rock objects.
Added in new animations for throwing a stun grenade. Added in particle effects to go off when stun grenade hits enemy or hits the floor. 
Updated RockImpact.cs to serve as a distraction to guards and then triggers guards to create a new navmesh navigation point. Update both guards logic so that the guards
divert to the location of whereever the rock landed. 

Alberto Trevino - Created the Main level. Created the door animation and animator for an automatic door for when the main player is near as well as the animation state.
This is controlled by the script I created TriggerDoor.cs. I also created another door that has a button, this is triggered by the player by pressing "F".
In order to do this I created functions in PlayerController.cs. The functions are "CheckForButtonInteraction(), OnAnimatorIK(), TriggerNearbyDoor()". I also created ButtonController.cs that is used to link the button to the associated door. I also created DoorController.cs which changes that state of the button door.
Created vaulting animation for when the player is near a vaultable object as well as created animation states for the payer to initiate the vault and exit the vaulting state.

Varun: Handled the implementation of the AI Simple Guard Enemy. Wrote the SimpleGuard.cs and modified the PlayerController.cs to account for the Player and Enemy Interaction. Handled implementation of Following Guard Enemy. Author of FollowingGuard.cs and inRoomTrigger.cs. Also designed Level 3 of the game.
