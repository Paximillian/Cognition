This asset is a very simple to use, very dynamic implementation of an object pool manager for both online and offline use.

An object pool is a solution used to optimize game performence by initializing certain often used objects in your game ahead of time and recycling them.
This asset simplifies the process to achieve this, and provides easy access in both offline and online games.

Setup instructions:

1) Offline object pooling:
	1. Create an object and attach the ObjectPoolManager script to it.
	2. You can set any amount of pools for your current scene, each pool is defined by a tag. The tag is the name of the pool, and how you reference it in your code. We'll get to this later.
	3. For each pool you can have any number of sources, a source is the prefab you will use to populate the pool, meaning, the object you'll receive from this pool. Having multiple sources means you'll have a random chance to draw a prefab when you pull an object from this pool.
	4. For each source you can set an initial amount of objects that will be instantiated once the scene loads.

2) Online object pooling:
	1. Create an object and attach the NetworkObjectPoolManager script to it.
	2. You can set any amount of pools for your current scene, each pool is defined by a tag. The tag is the name of the pool, and how you reference it in your code. We'll get to this later.
	3. For each pool you can have any number of sources, a source is the prefab you will use to populate the pool, meaning, the object you'll receive from this pool. Having multiple sources means you'll have a random chance to draw a prefab when you pull an object from this pool.
	4. For each source you can set an initial amount of objects that will be instantiated once the scene loads.
	5. Make sure to add all the prefabs you entered as sources for the object pool manager to your NetworkManager's Spawn Info.



Usage instructions:

1) Offline usage:
	1. To retrieve an object from your pool, all you have to do in your script is make a call to ObjectPoolManager.PullObject() with the tag you provided in the inspector as the string parameter.
	2. This returns a gameobject that you can use straight away, accessing all the relevant components to your needs as per usual. Make sure to set its position after you pull your object.
	3. To destroy an object and put it back in the pool, simply set its active flag to false using gameObject.SetActive(false), the pooling system will take it from there.

2) Online usage:
	1. To retrieve an object from your pool, all you have to do in your script is make a call to NetworkObjectPoolManager.PullObject() with the tag you provided in the inspector as the string parameter. 
Make sure to only run this code on the server side of your game. This will not work on the client, but if done from the server, it will sync like any other object you'd spawn.
	2. This returns a gameobject that you can use straight away, accessing all the relevant components to your needs as per usual. Make sure to set its position after you pull your object.
	3. To destroy an object and put it back in the pool, simply set its active flag to false using gameObject.SetActive(false), the pooling system will take it from there. 
Pooled objects often are a shoot and forget, and so the responsibility to set the active to false lies with all clients, but the one to actually disable the object and put it back in the pool is only the server.


KNOWN ISSUES:
- When loading the sample scenes for the first time, the object pool tags get jambled up for some reason. Simply change the first pool's tag to "Object1" and the 2nd to "Object2" and everything should work smoothly.
- In some cases an error saying that the tag wasn't set can occur when the tag was correctly set, this is caused by conflicting script execution order, to fix this, simply go to Edit->Project Settings->Script Execution Order and set the object pool script to execute before the script raising the error.
This can also be solved by changing the Start() method in the ObjectPoolManager classes to Awake(), though that fails for the networked version as the isServer identity that is checked on initialization is only set after awake is done apparently. Stick to the first method to fix this issue.


Credits:
- Mor Zilberman
- Tzah Mama
- Dimtry Katz
- Guy Dunski
- Eitan Acton