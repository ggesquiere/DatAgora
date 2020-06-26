# Lego CITY COUNTER
Gamagora 2019 - Transformation of a Mesh into Lego<br>
**Antoine CHEDIN & Rendy LATBI**

## Implemented features :
- Customizable Grid for Mesh Analysis
- Mesh analysis and LegoMap export in Json
- Creating a Mesh from a LegoMap

## Illustrations: 
<img src="Assets/Img/Terrain.PNG" data-canonical-src="Assets/Img/Terrain.PNG" width="430" height="250" /> <img src="Assets/Img/Lego.PNG" data-canonical-src="Assets/Img/Lego.PNG" width="430" height="250" />

## Use of the Project :
- Import the Project on your PC
- Launching with Unity 2019.2.x
- Open the Lego.unity Stage
- Import your 3D model to be analyzed in Unity
- Check that it has Colliders (optional: add the tag "Ground" to your ground)
- Moving the GameObject LegoAnalyser to your 3D model
- Set up the Grid (associated LegoAnalyser script) > cf appropriate section
- Exporting in JSON
- Selecting the GameObject VoxelGenerator
- Setting up the generator with your JSON
- Creating Mesh

## Grid Setup (LegoAnalyser) :
- Choose the size of the map (legos plate on the ground).
- Choose the scale and height of the map so that it includes all the terrain to be analysed.
- Choose the LayerMask to intersect (All or a specific one if you do not need to analyze all).
- The SetGroundAt0 option is used to normalize the height of the ground.
- Choose the name of the file (will be located in Assets/JSON/).

## Detail of exported JSON files :
<img src="Assets/Img/JSON.PNG" data-canonical-src="Assets/Img/JSON.PNG" width="300" height="250" /><br>
- Map Size
- Map Scale
- Number of Legos
- Matrix which at each point associates its type and corresponding height.
