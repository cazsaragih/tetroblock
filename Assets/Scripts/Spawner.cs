using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour
{
    public GameObject[] blocks;

	public GameObject Spawn()
    {
        int randomIndex = Random.Range(0, blocks.Length);
        return (GameObject)Instantiate(blocks[randomIndex], transform.position, Quaternion.identity);
    }

    public GameObject Spawn(int index)
    {
        return (GameObject)Instantiate(blocks[index], transform.position, Quaternion.identity);
    }
}
