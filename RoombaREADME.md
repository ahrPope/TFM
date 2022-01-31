# Prefabs
### Roomba Test

- Roomba model
- RigidBody
- Collisions

### Roomba Working

- Roomba Test
- Input controller

### Spawn

- Terrain spawner with generateTerrain Script. Place where you want in any scene.

### Waste

### YBot

- Human model
- Animations
- RigidBody

### YBot1

- Simpler model

# Scenes

- Terrain: Main Scene for Roomba in Park, generates the terrain with spawn prefab.

![image](https://user-images.githubusercontent.com/62309228/125615785-1114e3ca-8e39-48f8-bee7-505485f22ea0.png)

- spawnOffice: Main Scene for Roomba in Office. Created an object with generateOffice script.

![image](https://user-images.githubusercontent.com/62309228/125615896-e94762b5-5b0f-4561-845c-da26f2b46295.png)


- Test_Scenes:
  - ClassTry: YBot with animations + instantiate humans.
  - CollisionTest: Roomba and waste collisions.
  - Demo_Scene: Linear and automatic path tests.
  - Human: Class human2 scene.
  - ModernOffice_Interior_SampleLayout: Office with Working Roomba and YBot
  - Office: Simpler office with Roomba, YBot and Waste
  - Parque_Prueba: Roomba sceneario with collisions with waste that respawns once collided and a predefined path. Can also be changed to manual control.
  - PrefabsTest: One of the first tests of spawning YBot with input movement and rotation.
  - Roomba: First scenario with roomba and waste without collisions.

# Scripts

- Terrain:
  - generateOffice: 
        
    Spawns a layout which is read from office.txt. 
    That layout should be ColumnLenght x RowLength long which has to be defined in the txt. 
    It also defines the amount of Roombas wanted and the amount of wastes.
    
  - generateTerrain:

    A more complex spawner with more prefabs for the park terrain such as: pavement, trees and benches. 
    It also creates a transparent cube that surronds the park and some barriers so the roombas do not leave. 
    Reads the string from terrain.txt.
    
- Some testing scripts + controllers:

  - animationStateController: YBot animations when moving.
  - Character, Human and Human2: first test classes for YBot which instantiates and have the basic primitives such as move, rotate and moveHand.
  - CharacterCollision: Used in Roomba to control if there is a collision with waste, many changes done previously.
  - Controller: Controller used in Roomba with Run and Turn functions using RigidBody.
  - FollowPath, MovementPath, FollowPlayer: Creation of a path to make routines.
  - InstanciatePrefab: First tests to instantiate and change names of objects.
  - PlayerController: First user input controller using bad habits such as changing the position instead of velocity. Only useful if you want to move the camera at the same speed as the object.
