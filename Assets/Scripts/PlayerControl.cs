using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour {

    public static PlayerControl main;
    
    void Awake ( ) {
        if (main == null) main = this;
    }

    void Update ( ) {
        if ( transform.position.x > TerrainControl.main.size ) {
            transform.position = new Vector3(transform.position.x - TerrainControl.main.size, transform.position.y, transform.position.z);
            TerrainControl.main.UpdateTerrains(1,0);
        }
        else if (transform.position.x < 0){
            transform.position = new Vector3(transform.position.x + TerrainControl.main.size, transform.position.y, transform.position.z);
            TerrainControl.main.UpdateTerrains(-1,0);
        }
        if (transform.position.z > TerrainControl.main.size)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z - TerrainControl.main.size);
            TerrainControl.main.UpdateTerrains(0,1);
        }
        else if (transform.position.x < 0){
            transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + TerrainControl.main.size);
            TerrainControl.main.UpdateTerrains(0,-1);
        }
        transform.Translate(Vector3.right*TerrainControl.main.size*Time.deltaTime/5f);
    }

}