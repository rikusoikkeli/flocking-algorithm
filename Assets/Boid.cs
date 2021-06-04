using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
    Collider2D boidCollider;
    public Collider2D BoidCollider { get { return boidCollider; } }
    
    float cohesionStrength = 0.004f;
    float alignmentStrength = 0.02f;
    float avoidanceStrength = 0.5f;
    float avoidanceRadius = 10;
    float visibilityRadius = 30;
    float boidSpeed = 20;
    float maxSpeed = 40;
    float obstacleAvoidanceStrength = 0.5f;


    List<Collider2D> getNeighbouringBoids(float dist)
    {
        List<Collider2D> neighbouringBoids = new List<Collider2D> { };
        Collider2D[] colliders = Physics2D.OverlapCircleAll(this.transform.position, dist);
        foreach (Collider2D collider in colliders)
        {
            if (this.boidCollider != collider && collider.gameObject.name.Contains("boid"))
            {
                neighbouringBoids.Add(collider);
            }
        }
        return neighbouringBoids;
    }


    List<Collider2D> getNeighbouringObstacles(float dist)
    {
        List<Collider2D> neighbouringObstacles = new List<Collider2D> { };
        Collider2D[] colliders = Physics2D.OverlapCircleAll(this.transform.position, dist);
        foreach (Collider2D collider in colliders)
        {
            if (this.boidCollider != collider && !collider.gameObject.name.Contains("boid"))
            {
                neighbouringObstacles.Add(collider);
            }
        }
        //Debug.Log("Returning " + neighbouringObstacles.Count + " obstacles!");
        return neighbouringObstacles;
    }


    void drawRaycast()
    {
        RaycastHit2D hitInfo = Physics2D.Raycast(transform.position, transform.up);
        if (hitInfo.collider != null)
        {
            Debug.DrawLine(transform.position, hitInfo.point, Color.red);
        } else
        {
            Debug.DrawLine(transform.position, transform.position + transform.up * 100, Color.green);
        }
    }



    // Start is called before the first frame update
    void Start()
    {
        boidCollider = GetComponent<Collider2D>();
        // Raycasts do not hit their starting colliders
        Physics2D.queriesStartInColliders = false;
    }

    public void Move()
    {
        List<Collider2D> neighbours = this.getNeighbouringBoids(visibilityRadius);
        drawRaycast();


        var offset = this.transform.up * boidSpeed;
        if (this.name == "boid0")
        {
            //Debug.Log("transform.up: " + transform.up);
        }

        // cohesion - pysyvät lähdellä

        var cohesionMove = new Vector3();
        var flockCentre = new Vector3();
        foreach (var neighbour in neighbours)
        {
            flockCentre += neighbour.transform.position;
        }
        if (neighbours.Count != 0)
        {
            flockCentre /= neighbours.Count;
            cohesionMove = flockCentre - this.transform.position;
        }

        //if (this.name == "boid0")
        //    Debug.Log("cohesionMove = " + cohesionMove + " and " + "cohesionMove * cohesionStrength = " + cohesionMove * 100);

        offset += cohesionMove * cohesionStrength;




        // alignment - menevät samaan suuntaan

        var alignmentMove = new Vector3();
        var numAgents = 0;
        foreach (var neighbour in neighbours)
        {
            // transform.up ilmaisee spriten osoittamaa suuntaa
            // se on yksikkövektori, jolloinka 45 asteen kulma on (0.7, 0.7)
            alignmentMove += neighbour.transform.up;
            numAgents++;
        }
        if (numAgents != 0)
        {
            alignmentMove /= numAgents;
        }
        offset += alignmentMove * alignmentStrength;





        // avoidance - eivät kosketa
        // tämä tulee päivittää niin, että mitä lähempänä, sitä enemmän haluaa väistää

        List<Collider2D> neighboursToAvoid = this.getNeighbouringBoids(avoidanceRadius);
        var avoidanceMove = new Vector3();
        foreach(var neighbour in neighboursToAvoid)
        {
            var deltaX = neighbour.transform.position.x - this.transform.position.x;
            var deltaY = neighbour.transform.position.y - this.transform.position.y;
            var distance = Mathf.Sqrt( Mathf.Pow(deltaX, 2) + Mathf.Pow(deltaY, 2) );
            if (distance < 0.001f)
            {
                distance = 0.001f;
            }
            var repellance = (2 / Mathf.Pow(distance, 2));
            //if (this.name == "boid0")
            //    Debug.Log("repellance: " + repellance);

            avoidanceMove -= new Vector3(deltaX, deltaY, 0) * repellance;
        }

        offset += avoidanceMove * avoidanceStrength;




        // väistetään esteitä

        List<Collider2D> obstacles = this.getNeighbouringObstacles(visibilityRadius);
        if (obstacles.Count > 0)
        {
            var obstacleMove = new Vector3();
            foreach (var obstacle in obstacles)
            {
                var deltaX = obstacle.ClosestPoint(this.transform.position).x - this.transform.position.x;
                var deltaY = obstacle.ClosestPoint(this.transform.position).y - this.transform.position.y;
                var distance = Mathf.Sqrt(Mathf.Pow(deltaX, 2) + Mathf.Pow(deltaY, 2));
                Debug.Log("distance: " + distance);
                if (distance < 0.001f)
                {
                    distance = 0.001f;
                }
                var repellance = (2 / Mathf.Pow(distance, 2));

                obstacleMove -= new Vector3(deltaX, deltaY, 0) * repellance;
            }

            //Debug.Log("Changing offset: " + offset + " -> " + (offset+obstacleMove*obstacleAvoidanceStrength));
            offset += obstacleMove * obstacleAvoidanceStrength;
        }








        // määritetään spriten osoittama suunta
        transform.up = offset;

        // lisätään lopullinen siirtymä
        if (offset.magnitude > maxSpeed)
        {
            //if (this.name == "boid0")
            //    Debug.Log("Normalising offset from: " + offset + " -> " + offset.normalized*maxSpeed);

            offset = offset.normalized * maxSpeed;
        }
        //if (this.name == "boid0")
            //Debug.Log("offset: " + offset);
            //Debug.Log("offset magnitude: " + offset.magnitude);

        transform.position += (offset * Time.deltaTime);
    }

}



// miksi boidit, mikäli niitä on useampi, lähtevät aina oikeaan yläkulmaan?


