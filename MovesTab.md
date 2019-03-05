This tab lets you edit the following for each move:

* Move Name
* Animation - for now you can only swap the current animation for another existing animation.
* Type
* Power
* Accuracy (this is a scale of 0-255 so 229 would be ~90% hit chance)
* PP (I recommend sticking to multiples of 5. Not sure what happens if you don't)
* Effect \*

\*I haven't tested all of the effects yet to see exactly how they all work. I know the "Deal_Set_Damage" effect is based on move ID and handled in the combat engine part of code so if you try to set a different move to use that effect it might not work correctly. Also sadly the High Crit Chance effect is hard coded to certain moves in the engine and you are not currently able to change those.

![Moves Tab](https://github.com/jakefordyce/PokemonROMEditor/blob/master/images/MovesTab.PNG)

