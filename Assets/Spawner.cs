using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public Boid prefab;
    public List<Boid> boidsList;

    // Start is called before the first frame update
    void Start()
    {
        var numOfAgents = 50;

        // Luo alueelle sattumanvaraisesti x agenttia
        for (int i = 0; i < numOfAgents; i++)
        {
            var agent = Instantiate(prefab, 
                new Vector2(Random.Range(1,100), Random.Range(1, 100)), 
                Quaternion.Euler(Vector3.forward * Random.Range(0,360)));
            agent.name = "boid" + i;
            boidsList.Add(agent);
        }

    }

    // Update is called once per frame
    void Update()
    {
        foreach (Boid boid in boidsList)
        {
            boid.Move();
        }
    }
}
