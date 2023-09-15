# Rienet-Engine
A small game framework for my project Atelo made using Monogame, work in progress

# Structure
In a "WorldBody" contains many different "Scenes", inside each scene contains "GameObjects" (yes the scene itself is a gameobject), which include "Tiles", "Entities" etc, who can contain "PhysicsBodies" to interact with everything else in the scene through collisions and velocity. The world is organized in a grid system, where each grid has a 1x1 size, a bigger grid would be the "HitboxChunk" which divides a scene into smaller bits that can be individually called to check collision with bodies confined to that area, bodies that cross multiple hitboxchunks belong to all those chunks, but can only be collided with once, on each update of a body, it updates its chunk status. Graphics are handled through different types of components: GraphicsComponent (just a rectangle), Image, SpriteSheet, and AnimatedSheet, I'm sure you're able to tell what they do by name, but you do have to know that an animation spritesheet has individual animations set horizontally, and each animation is separated vertically in a picture, and you do have to define the bounds of each animation yourself
I've seen other engines use entity-component stuff, i don't really care

# Plans
3. Audio
4. Background Objects and Top down 2D compatiblity
5. Effects and Particles
7. Better Debugging
8. Easier Importing
9. Optimization
