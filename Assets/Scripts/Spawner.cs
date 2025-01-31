using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public float spawnRate = 15f;
    public Transform spawnPoint;
    public Transform spawnPoint2;
    public Transform spawnPoint3;

    public bool canSpawnAtPoint1 = true;
    public bool canSpawnAtPoint2 = true;
    public bool canSpawnAtPoint3 = true;

    public float spawnCount = 3;


    public bool canSpawn = true;
    // Start is called before the first frame update
    void Awake()
    {
        // spawner has enemy health component since it can be destroyed
        // get own health component


        // update all trasnforms 
        // do a shift on the z axis of -.01f so that the enemy renders in front of spawner
        spawnPoint.position = new Vector3(spawnPoint.position.x, spawnPoint.position.y, spawnPoint.position.z - .01f);
        spawnPoint2.position = new Vector3(spawnPoint2.position.x, spawnPoint2.position.y, spawnPoint2.position.z - .01f);
        spawnPoint3.position = new Vector3(spawnPoint3.position.x, spawnPoint3.position.y, spawnPoint3.position.z - .01f);
    }

    // Update is called once per frame
    void Update()
    {
        // use coroutine for precise timing
        if (canSpawn && spawnCount > 0) {
            canSpawn = false;
            StartCoroutine(spawnEnemy());
        } 
    }

    public void DecreaseSpawnerCount(float spawnerNumber) {
        spawnCount--;
        if (spawnCount <= 0) {
            Destroy(gameObject);
        }

        if (spawnerNumber == 1) {
            canSpawnAtPoint1 = false;
        } else if (spawnerNumber == 2) {
            canSpawnAtPoint2 = false;
        } else if (spawnerNumber == 3) {
            canSpawnAtPoint3 = false;
        }
    }

    private IEnumerator spawnEnemy() {
        yield return new WaitForSeconds(spawnRate);
            

        if (canSpawnAtPoint1) {
            Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
        }
        if (canSpawnAtPoint2) {
            Instantiate(enemyPrefab, spawnPoint2.position, Quaternion.identity);
        }
        if (canSpawnAtPoint3) {
            Instantiate(enemyPrefab, spawnPoint3.position, Quaternion.identity);
        }
        canSpawn = true;
        
    }
}
