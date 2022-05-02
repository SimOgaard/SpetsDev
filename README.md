## Update:
* [When unity 2021/2022 long support comes out, uppgrade](https://unity3d.com/unity/qa/lts-releases)


## High priority checklist: (things bellow gets put here to be queued)


Each triangle for each chunk should be evaluated to be a biome based on noise, we can use RaycastHit.triangleIndex to instanciate prefabs.



We need more than one pixel on each side since we distort water reflection image a lot, also if we want to add quarts/crystals/glass that offsets pixels by min(maxOffset, offsetAmplitude * distanceToGround)

We want to rework the chunk ground mesh system, it is badly written:
1. Rework Settings. We want:
    NoiseSettings   ->  spawnSettings   ->  biomeSettings   ->  worldGenerationSettings

    WorldGenerationManager should take in a WorldGenerationSettings object that defines how singular plain chunks are constructed, a global seed that offsets all seeds with this value, how large and what should be in the spawn area, the general difficulity and difficulity curve of the game as time and distance from origo increase, noise that defines where and what biomes should spawn and to what blend they should have (see it as a output from a neural network for every chunk triangle with weighted biome values like [0.2, 0.01, 0.9, 0.5] where each index is a specific biome), and multiple BiomeSettings. These BiomeSettings need to have biome specific materials, how each ground triangle should be generated and what should spawn on that biome. This requires BiomeSettings to have multiple SpawnSettings each for each object, this setting defines how frequent, what and where the object should spawn. All of these require a underlying NoiseSettings that represents a singular layer of noise like FastNoiseLite with added functionality like smoothing and blending, they should also keep a initilized version at runtime that is hidden in inspector so nothing has to be done when sampeling noise.

This would be a cool video:
You are in a wheat field sneeking with the trampled wheat trail going parralell up twords the top of the camera. You are hiding and after a second or so stop sneeking and do the minecraft peace sign. then you start running and after halfway to running take out your sword to cut wheat. Then you stop and do some combos.
For this to be done you need to rework world creation system, triangle swap, fix wheat visual, fix static collider ground mesh visual with dirty/gravely/clay visual, finnish character, build on sword weapon, add cut grass, cut wheat, burnt grass, burnt wheat.

dictionary<int, triangle> vs itterating the whole array
CreatePlaneJob should create a mesh that have no overlapping vertices
Since this mesh is constant throughout the whole game, we should be able to remove a triangle 

to create larger structures/buildings https://www.youtube.com/watch?v=0bcZb-SsnrA&ab_channel=BUasGames

* Triangles of type x after deletion and addition of same type x gets turnt upside down. So if i delete 100 grass and 10 flower, then go from nothing to flower 10 flowers will be upside down:

directory to array doesnt keep the sort

there is therefor a difference between:
```
// removes selected triangle from its submesh
void RemoveTriangle()
{
    // show that this mesh was updated
    groundSubtypesChanged[trianglesTypesIndex[triangleIndex]] = true;

    // remove triangle from last dict/submesh
    groundSubtypes[trianglesTypesIndex[triangleIndex]].Remove(triangleCornerIndex);
    groundSubtypes[trianglesTypesIndex[triangleIndex]].Remove(triangleCornerIndex + 1);
    groundSubtypes[trianglesTypesIndex[triangleIndex]].Remove(triangleCornerIndex + 2);
}

// adds a triangle to specified submesh at triangle point
void AddTriangle()
{
    // show that this mesh was updated
    groundSubtypesChanged[meshManipulation.changeToIndex] = true;

    // place new triangle in new submesh
    groundSubtypes[meshManipulation.changeToIndex][triangleCornerIndex] = triangles[triangleCornerIndex];
    groundSubtypes[meshManipulation.changeToIndex][triangleCornerIndex + 1] = triangles[triangleCornerIndex + 1];
    groundSubtypes[meshManipulation.changeToIndex][triangleCornerIndex + 2] = triangles[triangleCornerIndex + 2];
}
```

and 

```
public struct CreatePlaneJob : IJob
{
    [ReadOnly] public Vector2Int planeSize;
    [ReadOnly] public Vector2 quadSize;
    [ReadOnly] public int quadCount;
    [ReadOnly] public int vertexCount;
    [ReadOnly] public int triangleCount;

    [WriteOnly] public MeshData meshData;

    public void Execute()
    {
        float halfWidth = (planeSize.x * quadSize.x) * .5f;
        float halfLength = (planeSize.y * quadSize.y) * .5f;

        for (int i = 0; i < quadCount; i++)
        {
            int x = i % planeSize.x;
            int z = i / planeSize.x;

            float left = (x * quadSize.x) - halfWidth;
            float right = (left + quadSize.x);

            float bottom = (z * quadSize.y) - halfLength;
            float top = (bottom + quadSize.y);

            int v = i * 4;

            meshData.vertices[v + 0] = new Vector3(left, 0, bottom);
            meshData.vertices[v + 1] = new Vector3(left, 0, top);
            meshData.vertices[v + 2] = new Vector3(right, 0, top);
            meshData.vertices[v + 3] = new Vector3(right, 0, bottom);

            int t = i * 6;

            meshData.triangles[t + 0] = v + 0;
            meshData.triangles[t + 1] = v + 1;
            meshData.triangles[t + 2] = v + 2;
            meshData.triangles[t + 3] = v + 2;
            meshData.triangles[t + 4] = v + 3;
            meshData.triangles[t + 5] = v + 0;

            meshData.normals[v + 0] = Vector3.up;
            meshData.normals[v + 1] = Vector3.up;
            meshData.normals[v + 2] = Vector3.up;
            meshData.normals[v + 3] = Vector3.up;

            meshData.uv[v + 0] = new Vector2(0, 0);
            meshData.uv[v + 1] = new Vector2(0, 1);
            meshData.uv[v + 2] = new Vector2(1, 1);
            meshData.uv[v + 3] = new Vector2(1, 0);
        }
    }
}
```

* Pixel perfect camera for all resolutions using dpi and aspect ratio
* Update resolution of clouds dependent on screen resolution
* add damage taken under enemy health bar bottom left like elden ring

# PLAYER:
Make player character fluid and finnished.
* We want the player to be able to:
    * Sneak stealthfully with no sound, little visibility, but when touching enemy be seen. You preform a sneak by holding right ctrl or on controller L3. You can slide into a sneak from running stance without making sound. Sneaking should be zelda breath of the wild animation. You should be able to sneek with and without weapons drawn. You should be able to break sneek with a dash to any direction instantly into either walking or running depending on if you hold run button or not, going into a sprint will lengthen and quicken the dash. You should also be able to start walking by preforming a tiny hopp with a small button press or a high jump by holding it in. Or to running speed by preforming a longer jump by starting to sprint and jump at the same time, the height is just a bit higher but still dependent on how long you are holding the jump button, however the jump will be further in the direction of your movement when running. Attacking while sneaking will result in a high damage low speed attack, if enemy is not aware deal even more damage.
    * Walk with little sound and normal visibility with or without weapons drawn. On controller you can choose the speed you walk. You should be able to dash to any direction instantly into either walking or running depending on if you hold run button or not, going into a sprint will lengthen and quicken the dash. You should also be able to preform a tiny hopp with a small button press or a high jump by holding it in. Or get into running speed by preforming a longer jump by starting to sprint and jump at the same time, the height is just a bit higher than if you would be jumping into a walk but still dependent on how long you are holding the jump button, however the jump will be further in the direction of your movement than if you were going into walking. Weapon attacks are default in this state. 
    * Run with a lot of sound and visibility with or without weapons drawn. You preform a run by holding right shift or on controller X. So you will always be required to dash before running. Attacking after running will preform a slower, heavier and longer reaching attack that deal more damage if weapons are not drawn instantly draw them and attack at the same button press.
    * Dash with some sound initially depending on dash type (running/walking). You preform a run by pressing right shift or on controller X press. Attacking during a dash will preform a quicker, light and longer reaching attack if weapons are not drawn instantly draw them and attack at the same button press.
    * Jump with some sound initially and a lot of sound at landing all depending on jump type (running/walking), during jump have high visibility. Preform a jump with or without weapons drawn. Attacking during a jump will preform a slower, heavier and longer reaching attack that deal more damage if weapons are not drawn instantly draw them and attack at the same button press.
    * Not attacking for a while will holster your weapons, this delay gets shortened if you just defeated/ran away from danger, but it is never instant. Having a weapon holstered will increase your speed ever so slightly to match the more accurate runnig/walking/crouching animations.
    * Drawing a weapon will require you to attack once. This is done instantly without requireing another button press to preform the attack if we are running/dashing/jumping.
    * Picking stuff upp can be preformed at any time no matter the situation, there is no animation just a small icon in the bottom right corner telling you what you picked upp. Or if its a more essential item a small meny in bottom middle of your screen.
    * Darksouls like targeting where at the press of middle mouse or on console R3 a single enemy closest to you in the direction you are facing gets targeted. Pressing the button again will make the taget dissapear. Flicking R the target visually travel to the next closest so its easy to follow what you are locked on to, the enemy it travels to is the closest enemy to that enemy in the direction you flicked. If the enemy dies the next closest enemy to that enemy gets targeted without taking into considiration the direction you are flicking. Targeting should be completely voluntary and if you choose not to use darksouls like targeting automatic targeting should be preformed like https://www.youtube.com/watch?v=yGci-Lb87zs&ab_channel=t3ssel8r.

* player should have a health bar not hearts

* dash like sekiro (no iframes, just a quick way to get to running speed)
* jump also like sekriro (no iframes) ?!?!?!!?

https://64.media.tumblr.com/a9ead6db48fb68bdf6ac996322955666/e79456eee010c491-30/s540x810/fd7599da16640d7f2c9927ab9343dd158b130e34.gifv
https://www.google.com/url?sa=i&url=https%3A%2F%2Finsigniagame.tumblr.com%2Fpost%2F177615337006%2Fhweat&psig=AOvVaw2YWdDbosHSP2SQOUcZvM6B&ust=1651422038482000&source=images&cd=vfe&ved=0CAwQjRxqFwoTCNiYuNGYvPcCFQAAAAAdAAAAABAi
https://uploads.dailydot.com/c76/29/tumblr_mrcdipCOP41scncwdo1_500.gif?auto=compress&fm=gif&ixlib=php-3.3.0

# Other:
* unity 2021 lts
* compute shader to create ground mesh
* random init state seed for system and unity random
* different static grounds like rocky and grassy and sandy and clay and mud etc, right now you only have grassy
* Be able to choose if nothing (no triangle) AND everything (any triangle) should be changed in triangle mesh manipulation
* Remove clouds earlier on sun rise/fall (clouds should grow during morning and shrink during sunset from the middle of the cloud. same for moon)
* Fix how it should look between sunset and moon rise (darkest value should be lowest when moon is out NOT when sun just went down)
* Script som hanterar grass cutting (med specifika funktioner som ändrar materialet beroende på tex radius) (matematiska funktioner som representerar flera olika former som koner när du ittererar över 2d array (func(x, y) => true/false)) (något system för att representera komplexa former, kanske två colliders i en xor funktion) (collider inside collider with xor)
* The actual solid ground should be able to be muddy/stony
* limit the amount of files in resources, you do not need them on runtime


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