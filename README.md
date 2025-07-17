# Rienet-Engine
A small game framework for my 2D games Fear of Imperfection (paused) and TACTICAL BAND (ongoing) made using Monogame, work in progress

# Structure
In a "WorldBody" contains many different "Scenes", inside each scene contains "GameObjects", which can have "PhysicsBodies" to interact with everything else in the scene through collisions and velocity. The world is organized through quadtrees containing bodies and entities. Graphics are handled through semi auto components. I've made semi auto tools for importing scenes and tilesets as well.
For each project you would have to define more specific importers in the GamePanel class which is essentially the main engine (inherits Game), and script entities' classes yourself

# Updates
1. 7/23/2023: Started the project
2. 9/27/2023: Organized a bit and abstractified more content, whilst adding more convenient importing tools
3. i definitely forgot to write on this more, will update once TB is finished
