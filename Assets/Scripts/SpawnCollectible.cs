using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnCollectible : MonoBehaviour
{
    [SerializeField] private GameObject collectible;
    [SerializeField] private float delay = 0.5f;
    [SerializeField] private float min_X, max_X;

    void Start()
    {
        StartCoroutine(Spawn());
    }

    public IEnumerator Spawn()
    {
        while (true)
        {
            float random_X = Random.Range(min_X, max_X);
            Vector3 position = new Vector3(random_X, collectible.transform.position.y, 1);
            GameObject spawned_item = Instantiate(collectible, position, collectible.transform.rotation);

            yield return new WaitForSeconds(delay);

            Destroy(spawned_item, 5.0f);
        }
    }
}
