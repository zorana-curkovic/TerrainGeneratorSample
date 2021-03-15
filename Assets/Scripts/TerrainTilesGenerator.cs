using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TerrainState {

    public Terrain Terrain;
    
}

public class TerrainTilesGenerator : MonoBehaviour
{

    public Terrain stTerrain;
    public Texture2D heightMap;
    public Texture2D alphaMap;
    public Material terrainMat;
    public Texture2D[] layerDiffuses;

        

    public bool terrainInstancing;
    
    private TerrainState terrainState;
    private List<Terrain> terrains;
    private Transform mainCamera;
    private void Start()
    {
        mainCamera = Camera.main.transform;
        //terrainState = new TerrainState();  
        terrains = new List<Terrain>(); 
       
    //     int count = stTerrain.terrainData.alphamapTextures.Length;
    //     Debug.Log($"Alpha maps Count {count}");
       
    //    for (int i=0; i<4;++i)
    //    {
    //         Terrain t = GenerateFourTerrainTile(i);
    //         t.groupingID = 0;
    //         terrains.Add(t);
    //    }

    //     terrains[0].SetNeighbors(null, null, terrains[1], terrains[2]);
    //     terrains[1].SetNeighbors(terrains[0], null, null, terrains[3]);
    //     terrains[2].SetNeighbors(null, terrains[0], terrains[3], null);
    //     terrains[3].SetNeighbors(terrains[2], terrains[1], null, null);

    //    for (int i=0; i<2;++i)
    //     DestroyTerrainTile(i);

    }

    private RenderTexture alphaRT;
    private void LoadSplatTexture(TerrainData data)
    {
        
        alphaRT = new RenderTexture(alphaMap.width, alphaMap.height, 0, RenderTextureFormat.ARGB32);
        RectInt rect = new RectInt(0,0,alphaMap.width,alphaMap.height);
        Graphics.SetRenderTarget(alphaRT);
        RenderTexture currentActiveRT = RenderTexture.active;
        Graphics.Blit(alphaMap, currentActiveRT);
        data.CopyActiveRenderTextureToTexture(TerrainData.AlphamapTextureName, 0, rect, new Vector2Int(0, 0), true);
    }
   

    void CreateTile(int i)
    {
        Terrain t = GenerateFourTerrainTile(i);
        if (t == null)
            return;
        
        t.groupingID = 0;
        terrains.Add(t);
        if (i == 0)
            t.SetNeighbors(null, null, null, null);
        else if (i>0)
        {
            t.SetNeighbors(null, null, null, terrains[i-1]);
            terrains[i-1].SetNeighbors(null, t, null, terrains[i-1].bottomNeighbor);
        }
        
        Debug.Log($"Neighbours: {t.bottomNeighbor},{t.topNeighbor},{t.leftNeighbor},{t.rightNeighbor}");
        t.Flush();
        
        // Debug
        Debug.Log($"Alpha maps Count / alpaTexturesLength Terrain[{i}] = {t.terrainData.alphamapTextureCount}/{t.terrainData.alphamapTextures.Length}");
        Debug.Log($"Terrain Heightmap resolution  = {t.terrainData.heightmapResolution}");
        Debug.Log($"Terrain contains normal texture = {t.normalmapTexture}");
        Debug.Log($"Shader on terrain {t.materialTemplate.shader.name} ");

        float f = t.materialTemplate.GetFloat("_EnableInstancedPerPixelNormal");
        Debug.Log($"flag instanced per pixel {f}");
    }

    Terrain GenerateFourTerrainTile(int i)
    {
        TerrainData tData = new TerrainData();
        
        tData.size = new Vector3(10,30, 10);
        GameObject go = Terrain.CreateTerrainGameObject(tData);
        Terrain t = go.GetComponent<Terrain>();
        
        // 1. Handle the material and instancing
        if (terrainMat != null)
            t.materialTemplate = terrainMat;
        else 
            t.materialTemplate = new Material(Shader.Find("Universal Render Pipeline/Terrain/Lit"));
        
        if (terrainInstancing)
            t.drawInstanced = false;
        
        
        
        // 2. Name and position
        
        go.name = "TerrainGenerated_"+i.ToString();
        go.transform.position = new Vector3(16* 10 * i, 0, -16 *10 *i);
        
        
        // 4. Layers
        
        TerrainLayer[] layers = new TerrainLayer[3];
        if (layerDiffuses.Length < 3)
        {
            Debug.LogError("Set those diffuse textures");
            return null;
        }
        for (int l = 0; l < 3; ++l)
        {
            layers[l] = new TerrainLayer();
            if (layerDiffuses[l] != null)
                layers[l].diffuseTexture = layerDiffuses[l];
        }
        // 5. Splat textures configuration
        tData.terrainLayers = layers;
        Debug.Log(tData.alphamapTextureCount);
        
        float[, ,] alphamaps = new float[tData.alphamapWidth, tData.alphamapHeight, layers.Length];
        // set the actual textures in each tile here.
        // first two indexes are coordinates, the last is the alpha blend of this particular layer.
        tData.SetAlphamaps(0, 0, alphamaps);
        //tData.SetHeights(1,1, new float[5,5] {{10,10,10,10,10},{10,10,10,10,10},{10,10,10,10,10},{10,10,10,10,10},{10,10,10,10,10}});
        LoadSplatTexture(tData);
        
 
        // 6. Heightmap

        tData.heightmapResolution = 512;
        Graphics.CopyTexture(heightMap, tData.heightmapTexture);
        tData.DirtyHeightmapRegion(new RectInt(0, 0, tData.heightmapResolution, tData.heightmapResolution), TerrainHeightmapSyncControl.HeightAndLod);
        tData.SyncHeightmap();
        //t.Flush();
        if (terrainInstancing)
            t.drawInstanced = true;
        return t;
    }

    void DestroyTerrainTile(int i)
    {        
        Object.Destroy(terrains[i]);
        Object.Destroy(terrains[i].gameObject);
        //Object.Destroy(terrains[i].terrainData);
        Terrain.SetConnectivityDirty();

        // foreach (Texture2D texture in terrains[i].terrainData.alphamapTextures) {
        //     Object.Destroy(texture);
        // }

        terrains.RemoveAt(i);
    } 

    int counter = 0;
    float timer=0;
    float coolTime=1f;

    private void Update() {

        if (timer <= 0)
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                Debug.LogError("Creating tile");
                timer = coolTime;
                if (counter<4) {
                    CreateTile(counter);
                    counter++;
                }
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                timer = coolTime;
                if (counter > 0)
                {
                    Debug.Log($"Destroy {counter-1} /{terrains.Count}");
                    DestroyTerrainTile(counter-1);
                    counter--;
                }
            }
            
            if (Input.GetKeyDown(KeyCode.F))
            {
                if (terrains.Count > 0)
                {
                    Debug.LogError("Locating tile with camera");
                    Vector3 tile1pos = terrains[0].transform.position;
                    Vector3 offset = tile1pos + new Vector3(0, 50, 0);
                    mainCamera.position = offset;
                    mainCamera.LookAt(tile1pos);
                }
            }
        }
        else 
        {
            timer -= Time.deltaTime;
        }
    }








    void DestroySequence()
    {
        // Destroy objects.
        TerrainData terrainData = terrainState.Terrain.terrainData;
        GameObject terrainGameObject = terrainState.Terrain.gameObject;

        foreach (Texture2D texture in terrainState.Terrain.terrainData.alphamapTextures) {
            Object.Destroy(texture);
        }

        Object.Destroy(terrainData);
        Object.Destroy(terrainGameObject);

        Terrain.SetConnectivityDirty();
    }
}
