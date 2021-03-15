using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainControl : MonoBehaviour {

    public List<TerrainLoader> terrainList;
    public Dictionary<Vector3, TerrainLoader> terrains;
    public int size = 256;
    public int cellX = 1, cellZ = 1;

    public GameObject player;

    public static TerrainControl main;
	void Awake  () {
        if (main == null) main = this;
        Application.targetFrameRate = 60;
	}

    // Load and set start terrain data
    void Start ( ) {
        terrains = new Dictionary<Vector3, TerrainLoader>( );
        int index = 0;
        for ( int x=-1; x<=1; x++ ){
            for (int z=-1; z<=1; z++ ) {
                Vector3 key = new Vector3(x, 0, z);
                TerrainLoader currentTerrain = terrainList[index];
                currentTerrain.cellX = cellX+x;
                currentTerrain.cellZ = cellZ+z;
                currentTerrain.transform.position = new Vector3(x * size, 0, z * size);
                currentTerrain.Reload();
                terrains.Add(key, terrainList[index]);
                index++;
            }
        }

    }

    public void UpdateTerrains(int offsetX,int offsetZ) {
        StartCoroutine ( UpdateTerrainsCoroutine ( offsetX,offsetZ ) );
    }

    IEnumerator UpdateTerrainsCoroutine (int offsetX, int offsetZ) {
        cellX += offsetX;
        cellZ += offsetZ;
        Debug.Log("Updating terrains...");
        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                Vector3 key = new Vector3(x, 0, z);
                Vector3 keyNext = new Vector3(x - offsetX, 0, z - offsetZ);
                Vector3 keyNextClamp = keyNext;
                if (keyNextClamp.x > 1) keyNextClamp.x = -1;
                if (keyNextClamp.z > 1) keyNextClamp.z = -1;
                if (keyNextClamp.x < -1) keyNextClamp.x = 1;
                if (keyNextClamp.z < -1) keyNextClamp.z = 1;

                // Check if next terrain for this tile is loaded and replace it
                if (keyNext.x >= -1 && keyNext.x <= 1 && keyNext.z >= -1 && keyNext.z <= 1)
                {
                    Debug.Log("Moving terrain " + key + " to " + keyNext);
                    terrains[key].transform.position = keyNext * size;
                    TerrainLoader temp = terrains[key];
                    terrains[key] = terrains[keyNext];
                    terrains[keyNext] = temp;
                }
                // If next terrain is out, moves this tile as new terrain
                else
                {
                    Debug.Log("Loading new terrain " + key);
                    terrains[key].transform.position = keyNextClamp * size;
                    terrains[key].cellX = cellX - x;
                    terrains[key].cellZ = cellZ + z;
                    terrains[key].Reload();
                }
                /*float error = 1 +
                    10 * (Mathf.Abs(terrains[key].cellX - cellX) +
                    Mathf.Abs(terrains[key].cellZ - cellZ));
                    terrains[key].terrain.heightmapPixelError = error;*/
                // yield return null;
            }
        }
        yield return null;
    }

}
