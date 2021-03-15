using System.Collections;
using System.IO;
using UnityEngine;


public class TerrainLoader : MonoBehaviour
{
    public int heightMapSize = 257;
    public int cellX = 0, cellZ = 0;
    public string pathBase = "Assets/Resources/Terrains/terrain", path;
    public Terrain terrain;

    public void Awake() {
        terrain = GetComponent<Terrain>();
    }

    public void Reload ( ) {
        StartCoroutine(ReloadCoroutine());
    }

    IEnumerator ReloadCoroutine ( ) {
        path = pathBase + "_x";
        if (cellX < 10) path += "0";
        path += cellX + "_y";
        if (cellZ < 10) path += "0";
        path += cellZ + ".r16";
        FileStream file = File.OpenRead(path);
        yield return null;
        BinaryReader reader = new BinaryReader(file);
        yield return null;

        //heightMapSize = (int)Mathf.Sqrt(file.Length / 2);
        float[,] heights = new float[heightMapSize, heightMapSize];
        yield return null;

        //Debug.Log("Updating terrain " + cellX + "," + cellZ);
        float timeStamp = Time.realtimeSinceStartup;
        int doneX=0, doneZ=0;
        for (int x = 0; x < heightMapSize; x++) {
            for (int z = 0; z < heightMapSize; z++) {
                heights[x, z] = (float)reader.ReadUInt16() / 0xFFFF;
                if ( Time.realtimeSinceStartup - timeStamp >= 0.2f ) {
                    yield return null;
                    terrain.terrainData.SetHeightsDelayLOD(doneX, doneZ, heights);
                    yield return null;
                    doneX = x; doneZ = z;
                    heights = new float[heightMapSize-x, heightMapSize-z];
                    yield return null;
                    timeStamp = Time.realtimeSinceStartup;
                }
            }
        }

        terrain.terrainData.SetHeightsDelayLOD(doneX, doneZ, heights);        
        yield return null;

        file.Close();
        yield return null;

        terrain.ApplyDelayedHeightmapModification();
        yield return null;

        //Debug.Log("Finished terrain " + cellX + "," + cellZ + 
        //    " control in " + TerrainControl.main.cellX + "," + TerrainControl.main.cellZ );
    }

}
