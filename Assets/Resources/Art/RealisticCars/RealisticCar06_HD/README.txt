Hello, in order to get car driving and camera controllers work, You should firstly import Standard Assets (Free) from Asset Store into project
Epecially those components:

1. Cameras 
2. Cross platform input
3. Vehicles


And after that You can import RealisticCar_HD and it should work well by default.
If there will be any problem with Car scripts or anything, make sure You have imported those assets first.
And then reimport Car package or reopen "Demo_Driving" scene, it will refresh all scripts and should work.


If you would have any questions, feel free to ask at: storepolycraft@gmail.com


If You want to change anything with the Car controller, follow the CarGuidelines from Unity Vehicles Standard Asset.

In Demo_Driving scene only one car has enabled Car User Control script. 
So by default, player can controll only one Car, but if You want controll any other car, 
just enable all Car Control scripts in selected prefab and set tag as a Player 
and untagg previous car and disable Car User Control script, so camera can find which car is controlled by player at the time.

Car has 2 openable doors . Just select "Car06_Door_Front_Left" or any other door under:
"RealisticCar06_HD_Complete / Car06_Body" and rotate it in Y axis.
Doors don't have any animations or scripts applied for opening and closing, the same is about Windshield wipers, Steering Wheel and
Speedometer Pointers, If You want to make them live, You need to make animations or move them via scripts. 


Right Handed version uses the same vehicle body, wheels and all exterior elements that Left Handed version.
Just interior is replaced by "RealisticCar06_HD_Interior_RightHanded.fbx" in prefab. Package contains one prefab with Right Handed interior already.


Car controller is included in prefabs with "_Driving_" in name.
Car prefabs with "_Static_" in name are only static models, with LOD's implemented.

If You want to use this car in mobile game, You can use LOD1 or LOD2. 

Car model is aligned properly facing positive Z axis. Each car wheel and stering wheel is separated and alighend properly. 
You can find separated wheel model in meshes folder, just in case if You would like to use this wheel in some other cars or anything. 

All textures including "Metallic" in name have smothness/glossines in alpha channel.


If You want to make your own material using those textures, please apply Metallic Textures into Metallic slots and for Smothness please set "Metallic Alpha"

Main Body part includes 2 materials. One for Car paint (RealisticCar06_Body - You can adjust color, metallic and smothness freely. 
Also You can make Your own custom paint texture using Car06_BodyPaint_Albedo) and second for everything else 
(Car06_Parts - Color, metallic and smothness are defined by textures.

For body material You can apply any color You want in Albedo and set Metallic and Smothness as You want. I have made some materials with diffrent colors. 
If You need You can just copy one of them, change color or anything else and apply it the body part of the car.

All prefabs have LOD's applied and set properly, so if You want to change material in body, You have to apply it for all LOD levels. 
If You apply diffrent material only for LOD0, model will change its material for old one when zoomed out. The same thing is about Wheels.

Car model also includes meshes for Lights Emissive. All light groups are separated objects (Normal Lights, Brake Lights, Turn Lights etc.) 
and you can enable any of them by enabling mesh renderer. Brake Lights works with standard script from Car controller.

Collision meshes are included and properly set into prefabs.

Body parts and wheels have custom shadow caster implemented to save performance of shadow casting. 


All Glasses uses texture for different transparency. Back Windows are less transparent than Front windows and lights glasses etc.