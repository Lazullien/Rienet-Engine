# Rienet-Engine
A small game framework for my project Atelo made using Monogame, work in progress

# Structure
In a "WorldBody" contains many different "Scenes", inside each scene contains "GameObjects" (yes the scene itself is a gameobject), which include "Tiles", "Entities" etc, who can contain "PhysicsBodies" to interact with everything else in the scene through collisions and velocity. The world is organized through quadtrees, on each update of a body, it updates its chunk status. Graphics are handled through semi auto components, and i don't want to really explain it here. I've made semi auto tools for importing scenes and tilesets as well.
For each project you would have to define more specific importers in the GamePanel class which is essentially the main engine (inherits Game), and you'll have to specify what entities are (they don't have health and stuff, that's up to you)

# Plans
3. Audio
4. Top down 2D compatiblity
5. Effects and Particles
9. Optimization
10. entity behavior importing
