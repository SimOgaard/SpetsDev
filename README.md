## Update:
* [When unity 2021/2022 long support comes out, uppgrade](https://unity3d.com/unity/qa/lts-releases)


## High priority checklist: (things bellow gets put here to be queued)

* Regex all "_" followed by char to big char, for example: fast_noise_lite = fastNoiseLite, _collider = _collider
* Triangles sometimes after deletion and addition gets turnt upside down, might be that some vertices are set to be counter clockwise, (only happens if the same triangle is destroyed and created twice)
* different static grounds like rocky and grassy and sandy and clay and mud etc, right now you only have grassy
* Be able to choose if nothing (no triangle) AND everything (any triangle) should be changed in triangle mesh manipulation
* Remove clouds earlier on sun rise/fall (clouds should grow during morning and shrink during sunset from the middle of the cloud. same for moon)
* Fix how it should look between sunset and moon rise (darkest value should be lowest when moon is out NOT when sun just went down)
* Script som hanterar grass cutting (med specifika funktioner som ändrar materialet beroende på tex radius) (matematiska funktioner som representerar flera olika former som koner när du ittererar över 2d array (func(x, y) => true/false)) (något system för att representera komplexa former, kanske två colliders i en xor funktion) (collider inside collider with xor)
* The actual solid ground should be able to be muddy/stony


## Things to add:

* Rain and lightning
* Svärd med smear interpolation https://youtu.be/1FrIBkuq0ZI?t=246 
* Procedural trees/stones/bricks either by mimicking what he does in blender https://www.youtube.com/watch?v=ERA7-I5nPAU&ab_channel=t3ssel8r or simple rotation ofsset on tree stump and leaf
* Apply a boyancy force to items under water dependent on their precalculated volumes
* Yxa som dras i marken kan höga gräs
* Able to cut down trees with hole tecnique to acheave wood cutting effect, when two meshes intersect show inside of tree
* Spike ability höger gräset
* Target locking / hoaming på spikes
* Sand med waves (sand dunes
* Water med ocean waves
* God rays
* Fog/clouds https://youtu.be/A7tikkWLBE8?t=660 


## Bugs that needs to be fixed:

* Sprite of equipment going through the air is visually interactable, when it shouldnt be
* Någon del i pixel perfect scropt leder till förg fuckip när du klickar utanför, jag vet det för samma sak hände på Combitech Simulation 
* Wind map for current grass is not correct? https://discord.com/channels/695162838712582215/804311516282224711/921862951030386808 
* Pause doesnt stop all rigid bodies (make it so that you save their velocities and set them to zero without gravity)
* Updating the object pool breakes already instanciated objects
* Make shaders visible in prefab (probably forward base)
* DayNightCycle snap rotation gimble lock
* Throwing palabera is not true when roots are imaginary and you are elevated or they are elevated (in the air), since 45 degrees is not optimal in that scenario
* Figure out why your created cube does not look the same as createprimitive (probably normals)
* Cooldown eathbending pillar is broken
* Fix rigidbody mass, drag, forces, etc on every rigidbody and their interaction.
* Resource ID out of range in GetResource (happend once idk why just restarted unity)
* SetReplacementShader for transparent cutout does not work? (grass shader does not write to camera normal)


## Optimizations:

* Displacement or normal map instead of multiple triangles
* Physics ground does not need to be the same high resolution mesh since it is using normal shading
* Instead of using ienumerator to change burning triangle to ash, append a struct with triangle index, material to change to and at what time to do it. although it probably needs to be keept sorted, once it is done you can once a frame go through the list and change any triangle that should be changed and stop once the specified time is not less than current time
* Offset whole world position so that player is more or less allways in the middle
* Do not use fastnoise lite in shaders
* Do not sample noise value for water foam when the value is 0
* Precalculate density and volumes do not do it on runtime


## General shader restructuring:

* White color on directional light and actually use the light color
* Add step 3 and 4 of roystan esc toon shader to your own toon shaders https://roystan.net/articles/toon-shader.html
* Add normal sprite to billboard shaders https://www.youtube.com/watch?v=vOXrrEvYUVg&ab_channel=Artindi
* Support multiple lights (point light)
* Fix so that you can use forward base with light cookie
* Soft/fuzzy shadow (sebastianlaunge geogrophy game's cloud shadows)
* Point lightning ?using unity particle system?
* Pixel outline https://www.youtube.com/watch?v=jFevm02NJ5M&ab_channel=JamesKing


## Camera:

* Camera movement system that can lock onto enemies and is more still (doesnt do small movements only large)
* Small trail that lerps between enemmies that you decide to lock onto
* Camera that follows you and doenst move as much just large movements no small movements
* Change camera rotation to what david wrote in discord
* Holding q or e rotation goes to far add a overshoot threshold 
* Change wind direction dependent on camera rotation
* Make zoom in and out be like rotation (smooth and snaps)


# What is this game about?

* Open world som breath of the wild:
* Du börjar i mitten, världen är begrensad. Du får reda på vart 'slutmålet' är i början av spelet.
* Det ska vara stammina system. Det ska vara inventory system. Allt ska vara interaktivt som breath of wild.
* Weapon system that is breakable? 
* Water is copper nitrate


## Equipments:

earthbending:
uppgrade 1: only spawn in a straight line
uppgrade 2: choose start point and direction (rumble ult)
uppgrade 3. choose start point and follow mouse (cancellable)

found in a chest on top of a pillar out of reach, when you walk too close pillars comes upp around the chest surrounding you and enemies spaw, clearing the enemies makes the pillars go down again, the wall is of x layers of stone: inner layer is of uniform height outer two layers are in a decline and of random height represented by perlin noise. the pillars are stuck together and are too heavy for telekenisis and timestop physics to be affected
or in a huge maze like the ones in botw, when chest opens all pillars go down

(raycast on each corner of pillar and take the minimum y value)

spike ability (comby ish):
combo 1: spike shoot out at low radius and low radians (like it does now)
combo 2: spikes continue growing straight onwards for some time
combo 3: scythe / arch wave like Sirius from battlerite
(increase scale instead of moving the pillars, scale pivot at ground not in the middle)


telekenisis:
uppgrade 1: hold and throw single object (like it does now)
uppgrade 2: stach objects rotating around the character?
found in a chest in a ruin where all objecs in a radius is in hovering rotating without gravity and a fixed angular velocity without angular drag. could be applied to player objects aswell. when the chest it opened all objects gain gravity again
what messed things up on old code: telekenisis parrent of enemy rigidbody


time stop:
uppgrade 1: stop time for one object (like botw)
uppgrade 2: stop time for an area
uppgrade 3: stop time completely
found in a chest in a ruin where all objecs in a radius is fixed in place. attacking them makes them add velocity untill they fly away like botw, when the chest it opened all objects stop beeing in the fixed state


water abilities
Cooldown eathbending pillar count up amount of pillars. (ser inte bättre ut, imo)
Earthquake should slow enemies? (den gör det automatiskt typ, men att köra enemy.slowfor(float seconds))


Equipment uppgrades that gets dropped should effect abilities.
Equipments that drop out of chanse (meaning you already have an equipment of that type) should have roughly the same stats.
Equipment on ground effect: bob up and down above ground
Equipment ui cooldown
Equipment ui images


Enemies:

Hur attentiv enemien är, ska uppdateras med enemy state.
Now the enemy heard something:
/// If chase_transform != player_transform: act curious
/// Else: try to kill the player
fix agent (so that they can move reliably). then so that they can move smart


OTHER:

Dirt ground and wheat like (rise, lol) https://www.youtube.com/watch?v=fB8TyLTD7EE&ab_channel=LeagueofLegends

Fire like in https://twitter.com/t3ssel8r/status/1330794650605547520

scarecrow enemy på wheat
veroni+water+ice

Broad stuff:
world generation
play testing/debugging/refining
procedual animations
ai navigation
addition of equipments
hover show upgrades
upgrades working

Minor things:
Fire on ground visuals gets updated.

Shaders:
dirt shader:
voronoi algorithm
add shadow

crystal shader:
offset pixel like you do in water shader

smoke shader:
Like explosion shader but you know, smoke.

Design:
Pyramid
Nertrampat gräss
Leaf animation texture
Text font.
Ui images for hand and all new abilities.
Boss healthbar.
Pause meny with save start game etc.

General cool things:
sand dunes
Burn ability, much like earthbending ultimate but instead of pillars it is fire.
När vatten kväver elden visa rök.
Boss has chest ontop of it that does not open.
Snow biome with hexagional ice platforms in water.