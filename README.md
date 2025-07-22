# H1 Godot .Net Standard FPS Game

FPS Game That follows the classic style of FPS games and

This is a Godot project with the purpose of both learning solid practices in this field and creating what could be a nice finished project.

One of my most important goals is to create a fun game

# H3 7/14/2025
The process of cleaning up the hastely created scripts has started and will clean up all of the code except for the AI and all the latest code since most new code is created with efficiency in mind.

TODO in player.cs
Consider implementing a state pattern to manage player states more cleanly (IsReloading, Grabbing, IsUnderwater, etc.)

Instead of hardcoding references to "/root/World", consider passing world references through parameters

Replace magic numbers (like 10, 13, 14.0f) with named constants or explain them at least

Add more null checks, especially for weapon operations Consider what should happen when expected nodes/components are missing

The weapon switching logic could be more generic Consider storing weapons in a list/dictionary rather than separate variables

The interaction checks are quite lengthy with many type checks Consider implementing a common interface for interactable objects This would allow you to simplify the interaction logic

Review physics operations (like raycasts) to ensure they're not being called more than necessary Consider caching results when possible

For frequently created/destroyed objects (like thrown weapons), consider object pooling

For crosshair updates and other frequent checks, consider an event-based approach rather than polling

features:

Consider adding acceleration/deceleration curves for more natural movement

The procedural animations (weapon sway, head movement) could use smoothing parameters

specific stuff:

The comment about "issues with gun phasing through floor" suggests you need to:

Review the physics layers/masks

Adjust collision shapes

Possibly add checks when throwing weapons

The "Fix me" comment regarding Damagable objects needs proper handling:

Either implement proper handling for Damagable objects

Or refactor the grabbing system to work with your damage system
