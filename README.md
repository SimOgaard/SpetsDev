# Update:
* [When unity 2021/2022 long support comes out, uppgrade](https://unity3d.com/unity/qa/lts-releases)

# High priority checklist: (things bellow gets put here to be queued)

# 

# Shader
maybe you can snap unity_ObjectToWorld positions

[geo shadow snap on reflection is sometimes visible as an black edge, also straight edges.](https://media.discordapp.net/attachments/884159095500836914/992152051553411112/unknown.png).
[straigt edges are because ground shadow, since they do not excist here, however geo shadow snap on reflection still shows](https://cdn.discordapp.com/attachments/884159095500836914/992156671189205062/unknown.png)

why does it go "ayo wtf is TRANSFER_SHADOW/SHADOW_COORDS"? well because they are in the same cginc file... idk how to fix

make a geosnap setup cginc file so that we do not need to have the same shit in both geosnapshadow och geosnap och normalsnap. (du kan säkerligen köra SnapThis(inout o) och sedan lägga declareshadowmap(o) utanför)

use newly created geosnap setup cginc on billboard shader to snap all textures to pixel perfect camera coordinates too!
OLD TEXT: add snap to grid to sprite billboard shader so that the pixel streatch gets removed (https://cdn.discordapp.com/attachments/884159095500836914/991072980572442664/unknown.png)

we do not snap rotation, for example: our character can have non straight edges when running "straight" because "straight" is like 44.9 degrees instead of 45

because middle of object stays the same when scaling, we do not get the desired pixel snap, so snap scaling too!

multiple meshes get different world centers so they snap independent of eachothers. this can create flickering/inconsistent models when moving. look to player body capsule and head cube. this could be fixed by having a script on each static child, that on initilization, snaps the child in local space.

pixel perfect ui for all resolutions, even pixel fractions, will be difficult if canvas is ontop of camera and not screen

use blue noise instead of rand(float3), fastnoiselite noisetype: value might be just that 

make flatshading work with geo and geosnap 

make water use toonshader as to get cloud and shadow support on water, you might have to warp them so a cube shadow isnt a cube shadow, but a jiggly one

make clouds, water, etc easier to custimise by not having to change their values in code, The difficult part is not requiering 10000x different shader compilers. 

weird line side to side when directional ligt, object side and camera direction are parrarell to each others. only at some resolutions. https://cdn.discordapp.com/attachments/884159095500836914/989183712321233008/unknown.png 

update normalshading and flat shading to get:
a light value (float 4) of:
global environment light (colored)
directional light (colored) with no cascades (since it is ortho) and its shadows (colored)
add ontop of that all point lights

finally either:
use the final light alpha value to get color gradient of each material, and then add the colored light to it
OR
use the final light alpha value to get color gradient x value of each material, then rgb to get y value of texture
OR
use light cookie or other texture (different for each light) to create a color and alpha gradient (like any other material) and add that colored light to it, (would achive band look)
https://www.youtube.com/watch?v=0xJqzUHJ2fI&t=5s

instead of color gradient beeing a texture, create a class that is a list of a list of colors and convert it to a texture and pass to material, list of list of colors because we want the option to make it a 2D texture with night day that gets lerped between in shader, make the texture clamped so that if it is only a 1D image you lerp between the two same colors. The class should also hold curve functionality. And in inspector display a texture that is a example.

switch between moon and sun cloud and light properties.

# Compute buffers
how does constant buffers work, or how do i definy a structuredbuffer of lenth 1 ie just a struct

# World generation
get multiple static meshes to work for a singular biome

we need to be able to change underlying triangle submesh depending on triangle height and normal

remove duplicates of BiomeMaterials and FoliageSettings etc while keeping the order

create a uniformed distributed raycast for each chunk, get biome ray hit, select random prefab from that biome, check spawn condition, spawn it

add masked noise to noise mesh displacement

Each triangle for each chunk should be evaluated to be a biome based on noise, we can use RaycastHit.triangleIndex to instanciate prefabs.

noise for biome can have the same noise for two biomes but one have larger falloff (cubed) or specified value threshold so that  

WorldGenerationManager should take in a WorldGenerationSettings object that defines how singular plain chunks are constructed, a global seed that offsets all seeds with this value, add a option to make all seeds random, OBS! make shure all seeds including .net, unity and fastnoiselite are changed and used, how large and what should be in the spawn area, the general difficulity and difficulity curve of the game as time and distance from origo increase, noise that defines where and what biomes should spawn and to what blend they should have (see it as a output from a neural network for every chunk triangle with weighted biome values like [0.2, 0.01, 0.9, 0.5] where each index is a specific biome), and multiple BiomeSettings. These BiomeSettings need to have biome specific materials, how each ground triangle should be generated and what should spawn on that biome. This requires BiomeSettings to have multiple SpawnSettings each for each object, this setting defines how frequent, what and where the object should spawn. All of these require a underlying NoiseSettings that represents a singular layer of noise like FastNoiseLite with added functionality like smoothing and blending, they should also keep a initilized version at runtime that is hidden in inspector so nothing has to be done when sampeling noise.

deligates on specific triangles that give items or buffs when walked over, maybe the watery lands has speed flowers, poisioned swamp has posion resistance

distance field slimes with traingle change that paints triangles slime color

to create larger structures/buildings https://www.youtube.com/watch?v=0bcZb-SsnrA&ab_channel=BUasGames also make you able to build stuff

directory to array doesnt keep the sort

Be able to choose if nothing (no triangle) AND everything (any triangle) should be changed in triangle mesh manipulation

lägg till mer specifika funktioner som ändrar trianglarna beroende på tex radius, matematiska funktioner som representerar flera olika former som koner när du ittererar över 2d array (func(x, y) => true/false), något system för att representera komplexa former, kanske två colliders i en xor funktion, collider inside collider with xor, etc

# Abilities
earthbending rock needs to grab the material for the biome it hits

# Player
player should have a health bar not hearts
shake healthbar when a lot of damage is taken or given to enemy

* To make player character fluid and finnished, we want the player to be able to:
    * Sneak stealthfully with no sound, little visibility, but when touching enemy be seen. You preform a sneak by holding right ctrl or on controller L3. You can slide into a sneak from running stance without making sound. Sneaking should be zelda breath of the wild animation. You should be able to sneek with and without weapons drawn. You should be able to break sneek with a dash to any direction instantly into either walking or running depending on if you hold run button or not, going into a sprint will lengthen and quicken the dash. You should also be able to start walking by preforming a tiny hopp with a small button press or a high jump by holding it in. Or to running speed by preforming a longer jump by starting to sprint and jump at the same time, the height is just a bit higher but still dependent on how long you are holding the jump button, however the jump will be further in the direction of your movement when running. Attacking while sneaking will result in a high damage low speed attack, if enemy is not aware deal even more damage.
    * Walk with little sound and normal visibility with or without weapons drawn. On controller you can choose the speed you walk. You should be able to dash to any direction instantly into either walking or running depending on if you hold run button or not, going into a sprint will lengthen and quicken the dash. You should also be able to preform a tiny hopp with a small button press or a high jump by holding it in. Or get into running speed by preforming a longer jump by starting to sprint and jump at the same time, the height is just a bit higher than if you would be jumping into a walk but still dependent on how long you are holding the jump button, however the jump will be further in the direction of your movement than if you were going into walking. Weapon attacks are default in this state. 
    * Run with a lot of sound and visibility with or without weapons drawn. You preform a run by holding right shift or on controller X. So you will always be required to dash before running. Attacking after running will preform a slower, heavier and longer reaching attack that deal more damage if weapons are not drawn instantly draw them and attack at the same button press.
    * Dash with some sound initially depending on dash type (running/walking). You preform a run by pressing right shift or on controller X press. Attacking during a dash will preform a quicker, light and longer reaching attack if weapons are not drawn instantly draw them and attack at the same button press. dash like sekiro (no iframes, just a quick way to get to running speed)
    * Jump with some sound initially and a lot of sound at landing all depending on jump type (running/walking), during jump have high visibility. Preform a jump with or without weapons drawn. Attacking during a jump will preform a slower, heavier and longer reaching attack that deal more damage if weapons are not drawn instantly draw them and attack at the same button press. also like sekriro (no iframes)
    * Not attacking for a while will holster your weapons, this delay gets shortened if you just defeated/ran away from danger, but it is never instant. Having a weapon holstered will increase your speed ever so slightly to match the more accurate runnig/walking/crouching animations.
    * Drawing a weapon will require you to attack once. This is done instantly without requireing another button press to preform the attack if we are running/dashing/jumping.
    * Picking stuff upp can be preformed at any time no matter the situation, there is no animation just a small icon in the bottom right corner telling you what you picked upp. Or if its a more essential item a small meny in bottom middle of your screen.
    * Darksouls like targeting where at the press of middle mouse or on console R3 a single enemy closest to you in the direction you are facing gets targeted. Pressing the button again will make the taget dissapear. Flicking R the target visually travel to the next closest so its easy to follow what you are locked on to, the enemy it travels to is the closest enemy to that enemy in the direction you flicked. If the enemy dies the next closest enemy to that enemy gets targeted without taking into considiration the direction you are flicking. Targeting should be completely voluntary and if you choose not to use darksouls like targeting automatic targeting should be preformed like https://www.youtube.com/watch?v=yGci-Lb87zs&ab_channel=t3ssel8r.

# Enemies
add damage taken as a number under enemy health bar bottom left like elden ring

# Youtube
This would be a cool video:
You are in a wheat field sneeking with the trampled wheat trail going parralell up twords the top of the camera. You are hiding and after a second or so stop sneeking and do the minecraft peace sign. then you start running and after halfway to running take out your sword to cut wheat. Then you stop and do some combos.
For this to be done you need to rework world creation system, triangle swap, fix wheat visual, fix static collider ground mesh visual with dirty/gravely/clay visual, finnish character, build on sword weapon, add cut grass, cut wheat, burnt grass, burnt wheat.

# Optimization
are draw calls added if there are no triangles in submesh for that material?

# Pixelart
https://64.media.tumblr.com/a9ead6db48fb68bdf6ac996322955666/e79456eee010c491-30/s540x810/fd7599da16640d7f2c9927ab9343dd158b130e34.gifv
https://insigniagame.tumblr.com/post/177615337006/hweat
https://uploads.dailydot.com/c76/29/tumblr_mrcdipCOP41scncwdo1_500.gif?auto=compress&fm=gif&ixlib=php-3.3.0

# Ideas
Imagine if you could lerp between two target resolutions, and in doing so ofcourse change the zoom:
    private static float _zoom = 0.0f;
    /// <summary>
    /// Lerp value that controlls targetWidth and targetHeight 
    /// </summary>
    public static float zoom
    {
        get { return _zoom; }
        set { _zoom = Mathf.Clamp01(value); }
    }
Maybe this is used in towers when you are far above ground level. Or when channeling at a camp so you could move the camera far away from the player but it allso zoomed out so you could see and scout even more.
If you wanted to keep the resolution-ish you could create two of each texture, for example smaller grass or interact texture

you sit on the start randomly generated biome with the camera moved to a random location, the camera is static, but you can change the settings and see them beeing applied in real time, ie sound resolution etc. and on play the menu goes away and your character stands up

there should only be one of each biome, which is chosen at the start of the game. (for each biome choose next closest midpoint on cellular noise as the biome position, when all biomes are used, all other cells become end of world biome)

falling quads rotated twords camera. make the quad gravity dependent -9.82 * Time[0] each frame. but also wind determined (add 3d wind to shader). the quad should follow a animation tile set that given a wind vector, select a leaf sprite. could be used when hitting a tree or when a tree is falling/chopped down. maybe even grass cutting.

different static grounds like rocky and grassy and sandy and clay and mud etc, right now you only have grassy

# Things to add:

* Rain and lightning
* Svärd med smear interpolation https://youtu.be/1FrIBkuq0ZI?t=246 
* Procedural trees/stones/bricks either by mimicking what he does in blender https://www.youtube.com/watch?v=ERA7-I5nPAU&ab_channel=t3ssel8r or simple rotation ofsset on tree stump and leaf
* Apply a boyancy force to items under water dependent on their precalculated volumes
* Yxa som dras i marken som högger gräs när man drar den
* Able to cut down trees with hole tecnique to acheave wood cutting effect, when two meshes intersect show inside of tree, https://www.youtube.com/watch?v=cHhxs12ZfSQ 
* Spike ability höger gräset
* Target locking / hoaming på spikes
* Sand med waves (sand dunes)
* Water med ocean waves
* God rays
* volumetric fog/clouds https://youtu.be/A7tikkWLBE8?t=660 

## Bugs that needs to be fixed:

* Linux main_tex_st uv is different on water reflection. its mostly white and 1/6 of the top is correct 
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

sekiro like sword

earthbending:
uppgrade 1: only spawn in a straight line
uppgrade 2: choose start point and direction (rumble ult)
uppgrade 3. choose start point and follow mouse (cancellable)

found in a chest on top of a pillar out of reach, when you walk too close pillars comes upp around the chest surrounding you and enemies spaw, clearing the enemies makes the pillars go down again, the wall is of x layers of stone: inner layer is of uniform height outer two layers are in a decline and of random height represented by perlin noise. the pillars are stuck together and are too heavy for telekenisis and timestop physics to be affected, Basalt Eruption from dst
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