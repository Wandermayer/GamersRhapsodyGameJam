using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour {
	
	public Map[] maps;
	public int mapIndex;

	public Transform tilePrefab;

	public Transform NavmeshFloor;
	public Transform NavMeshMaskPrefab;

	public Vector2 MaxMapSize;
	
	[Range(0,1)]
	public float outlinePercent;


	public float tileSize;
	
	List<Coord> allTileCoords;
	Queue<Coord> shuffledTileCoords;
	Queue<Coord> shuffledOpenTileCoords;
	Transform[,] tileMap;

	Map currentMap;

	void Awake() {
		FindObjectOfType<Spawner> ().OnNewWave += OnNewWave;
	}

	void OnNewWave(int waveNumber){
		mapIndex = waveNumber - 1;
		GenerateMap ();

	}
	
	public void GenerateMap() {
		currentMap = maps [mapIndex];
		tileMap = new Transform[currentMap.mapSize.x, currentMap.mapSize.y];
		//System.Random prng = new System.Random (currentMap.seed);
		GetComponent<BoxCollider>().size = new Vector3 (currentMap.mapSize.x * tileSize, 0.5f, currentMap.mapSize.y * tileSize); 
		
		//Generating Co-ordinates
		allTileCoords = new List<Coord> ();
		for (int x = 0; x < currentMap.mapSize.x; x ++) {
			for (int y = 0; y < currentMap.mapSize.y; y ++) {
				allTileCoords.Add(new Coord(x,y));
			}
		}
		shuffledTileCoords = new Queue<Coord> (Utility.ShuffleArray (allTileCoords.ToArray (), currentMap.seed));

		//Generating parent mapHolder
		string holderName = "Generated Map";
		if (transform.FindChild (holderName)) {
			DestroyImmediate (transform.FindChild (holderName).gameObject);
		}
		
		Transform mapHolder = new GameObject (holderName).transform;
		mapHolder.parent = transform;

		//Spawning tiles for the ground
		for (int x = 0; x < currentMap.mapSize.x; x ++) {
			for (int y = 0; y < currentMap.mapSize.y; y ++) {
				Vector3 tilePosition = CoordToPosition(x,y);
				Transform newTile = Instantiate (tilePrefab, tilePosition, Quaternion.Euler (Vector3.right * 90)) as Transform;
				newTile.localScale = Vector3.one * (1 - outlinePercent) * tileSize;
				newTile.parent = mapHolder;
				tileMap[x,y] = newTile;
			}
		}

		//Spawning obstacles
		//bool[,] obstacleMap = new bool[(int)currentMap.mapSize.x,(int)currentMap.mapSize.y];
		

		List<Coord> allOpenCoords = new List<Coord> (allTileCoords);

	

		shuffledOpenTileCoords = new Queue<Coord> (Utility.ShuffleArray (allOpenCoords.ToArray (), currentMap.seed));

		//Generating NavMesh mask
		Transform maskLeft = Instantiate (NavMeshMaskPrefab, Vector3.left * (currentMap.mapSize.x + MaxMapSize.x) / 4f * tileSize, Quaternion.identity) as Transform;
		maskLeft.parent = mapHolder;
		maskLeft.localScale = new Vector3 ((MaxMapSize.x - currentMap.mapSize.x) / 2f, 1, currentMap.mapSize.y) * tileSize;

		Transform maskRight = Instantiate (NavMeshMaskPrefab, Vector3.right * (currentMap.mapSize.x + MaxMapSize.x) / 4f * tileSize, Quaternion.identity) as Transform;
		maskRight.parent = mapHolder;
		maskRight.localScale = new Vector3 ((MaxMapSize.x - currentMap.mapSize.x) / 2f, 1, currentMap.mapSize.y) * tileSize;

		Transform maskTop = Instantiate (NavMeshMaskPrefab, Vector3.forward * (currentMap.mapSize.y + MaxMapSize.y) / 4f * tileSize, Quaternion.identity) as Transform;
		maskTop.parent = mapHolder;
		maskTop.localScale = new Vector3 (MaxMapSize.x, 1, (MaxMapSize.y-currentMap.mapSize.y) / 2f) * tileSize;

		Transform maskBottom = Instantiate (NavMeshMaskPrefab, Vector3.back * (currentMap.mapSize.y + MaxMapSize.y) / 4f * tileSize, Quaternion.identity) as Transform;
		maskBottom.parent = mapHolder;
		maskBottom.localScale = new Vector3 (MaxMapSize.x, 1, (MaxMapSize.y-currentMap.mapSize.y) / 2f) * tileSize;

		NavmeshFloor.localScale = new Vector3 (MaxMapSize.x, MaxMapSize.y) * tileSize;
		
	}
	
	bool MapIsFullyAccessible(bool[,] obstacleMap, int currentObstacleCount) {
		bool[,] mapFlags = new bool[obstacleMap.GetLength(0),obstacleMap.GetLength(1)];
		Queue<Coord> queue = new Queue<Coord> ();
		queue.Enqueue (currentMap.mapCenter);
		mapFlags [currentMap.mapCenter.x, currentMap.mapCenter.y] = true;
		
		int accessibleTileCount = 1;
		
		while (queue.Count > 0) {
			Coord tile = queue.Dequeue();
			
			for (int x = -1; x <= 1; x ++) {
				for (int y = -1; y <= 1; y ++) {
					int neighbourX = tile.x + x;
					int neighbourY = tile.y + y;
					if (x == 0 || y == 0) {
						if (neighbourX >= 0 && neighbourX < obstacleMap.GetLength(0) && neighbourY >= 0 && neighbourY < obstacleMap.GetLength(1)) {
							if (!mapFlags[neighbourX,neighbourY] && !obstacleMap[neighbourX,neighbourY]) {
								mapFlags[neighbourX,neighbourY] = true;
								queue.Enqueue(new Coord(neighbourX,neighbourY));
								accessibleTileCount ++;
							}
						}
					}
				}
			}
		}
		
		int targetAccessibleTileCount = (int)(currentMap.mapSize.x * currentMap.mapSize.y - currentObstacleCount);
		return targetAccessibleTileCount == accessibleTileCount;
	}
	
	Vector3 CoordToPosition(int x, int y) {
		return new Vector3 (-currentMap.mapSize.x / 2f + 0.5f + x, 0, -currentMap.mapSize.y / 2f + 0.5f + y) * tileSize;
	}

	public Transform GetTileFromPosition(Vector3 position){
		int x = Mathf.RoundToInt(position.x / tileSize + (currentMap.mapSize.x - 1) / 2f);
		int y = Mathf.RoundToInt(position.z / tileSize + (currentMap.mapSize.y - 1) / 2f);
		x = Mathf.Clamp (x, 0, tileMap.GetLength (0) - 1);
		y = Mathf.Clamp (y, 0, tileMap.GetLength (1) - 1);

		return tileMap [x, y];

	}
	
	public Coord GetRandomCoord() {
		Coord randomCoord = shuffledTileCoords.Dequeue ();
		shuffledTileCoords.Enqueue (randomCoord);
		return randomCoord;
	}

	public Transform GetRandomOpenCoord(){
		Coord randomCoord = shuffledOpenTileCoords.Dequeue ();
		shuffledOpenTileCoords.Enqueue (randomCoord);
		return tileMap [randomCoord.x, randomCoord.y];
	}

	[System.Serializable]
	public struct Coord {
		public int x;
		public int y;
		
		public Coord(int _x, int _y) {
			x = _x;
			y = _y;
		}
		
		public static bool operator ==(Coord c1, Coord c2) {
			return c1.x == c2.x && c1.y == c2.y;
		}
		
		public static bool operator !=(Coord c1, Coord c2) {
			return !(c1 == c2);
		}

	}

		[System.Serializable]
		public class Map{

			public Coord mapSize;
			//[Range(0,1)]
			//public float obstaclePercent;
			public int seed;
			//public float minObstacleHeight;
			//public float maxObstacleHeight;
			//public Color foregroundColor;
			//public Color bgColor;

			public Coord mapCenter{
				get{
					return new Coord(mapSize.x / 2, mapSize.y / 2);
				}
			}




	}
}

