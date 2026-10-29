# Frog FC

A two-player game on one keyboard. Two frogs stand on a lily
pad floating on a pond, with a hole on each end of the leaf.
Drop the ball in the other player's hole tog et a point. First
to 5 wins.

Each frog has two ways to hit the ball:

- **Tongue** - fires in whatever direction you're holding, goes
  where you aimed, barely moves the ball. This is how to score.
  It also allows you to knock players back. It has a half-second
  cooldown.
- **Headbutt** - any body contact launches the ball hard. No cooldown.
  It's too fast to drop in a hole, so it's better used to hit the ball
  up and clear it.


## Controls

Player 1 
  Move : `A` `D`
  Jump (double jump): `W`
  Tongue: `Left Shift`
Player 2
  Move : `←` `→`
  Jump (double jump): `↑`
  Tongue: `Right Shift`

  To aim the tongue: hold a direction while firing
  Headbutt is automatic; just run into the ball

`Space` starts the match from the title screen. 
`R` restarts after someone wins.

## The Charge Bar

The three lily pads at the top of the screen are the ball's charge. Every
tongue hit adds one. Each lily pad makes the ball heavier and slower. A
charged ball drops through a hole more easily, and is harder to clear away
from your end.

The charge drains after a few seconds. It serves as a window to attack in
instead of setting up the ball to be heavy all game. 

## Running it

Built in **Unity 6.5 (6000.5.10f1)**.

1. Open the project folder in Unity Hub
2. Open `Assets/Scenes/_Scene_0`
3. Press Play

## Credits

- **Audio** : Pixabay (uploaders `freesound_community`, `tanweraman`),
  Pixabay Content License
- **Font** : SuperFrog, from Fontspace
- **Art** : vector art made using Figma

## Files

- `changes.txt` - what changed between the one-pager design document and the finished game
