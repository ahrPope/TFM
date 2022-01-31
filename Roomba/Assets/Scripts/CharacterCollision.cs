using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCollision : MonoBehaviour
{

    //En este script se comprueban las colisiones con las basuras y con las paredes, si es con las basuras se llama al metodo para dar recompensa al agente y se destruye
    //Si es con un borde se le da un castigo al agente

    generateTerrain c;
    private void OnCollisionEnter(Collision other) {
        //Debug.Log(other.gameObject.name);
        if(other.gameObject.tag == "Waste")
        {
            GetComponent<RoombaAgent>().GiveReward();
            Destroy(other.gameObject);
            
            //c = GameObject.FindGameObjectWithTag("Spawn").GetComponent<generateTerrain>();
            //c.instantiateWaste();
            //int spawnPositionX = Random.Range(-2,2);
            //int spawnPositionZ = Random.Range(9,15);
            //Vector3 spawnPosition = new Vector3(spawnPositionX, 0.15f, spawnPositionZ);
            //other.gameObject.transform.position = spawnPosition;
            
        }
        if (other.gameObject.tag == "Border")
        {
            GetComponent<RoombaAgent>().GivePunish();

           
           

        }

    }
}
