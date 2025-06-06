using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SwarmController : MonoBehaviour
{
    public Transform player;

    public static SwarmController Instance;
    public Transform swarmCenter;
    public List<Transform> swarmBalls = new List<Transform>();

    private Dictionary<Transform, Vector3> orbitalAxes = new Dictionary<Transform, Vector3>();
    private Dictionary<Transform, float> orbitalSpeeds = new Dictionary<Transform, float>();
    private Dictionary<Transform, float> orbitalOffsets = new Dictionary<Transform, float>();

    public float formationRadius = 1.3f;

    public float orbitalSpeed = 500f;

    public float cohesionStrength = 2f;

    public float separationRadius = 2f;

    public float separationForce = 2f;

    private Vector3 sharedOrbitalAxis;

    private void Awake()
    {
        Instance = this;

        sharedOrbitalAxis = Vector3.up;
    }
    public void addToSwarm(Transform ball)
    {
        if (!swarmBalls.Contains(ball))
        {
            swarmBalls.Add(ball);

            Renderer rend = ball.GetComponent<MeshRenderer>();
            float randomShape = Random.value;
            rend.material.color = randomShape < 0.3 ? Color.black : randomShape > 0.7 ? Color.white : Color.black;

            Vector3 randomAxis = Random.insideUnitSphere.normalized;
            orbitalAxes[ball] = randomAxis;

            orbitalSpeeds[ball] = orbitalSpeed * (1f + Random.Range(-15f, 15f));

            orbitalOffsets[ball] = Random.Range(0f, 360f);
        }
    }

    private void Update()
    {
        formSwarm();

        float distanceToPlayer = (player.position - swarmCenter.position).magnitude;

        
    }
    
    private void shootPlayer()
    {
        if (swarmBalls.Count == 0) { return; }

        Transform chosenBall = swarmBalls[Random.Range(0, swarmBalls.Count)];
        swarmBalls.Remove(chosenBall);

        if (orbitalAxes.ContainsKey(chosenBall)) { orbitalAxes.Remove(chosenBall); }
        if (orbitalSpeeds.ContainsKey(chosenBall)) { orbitalSpeeds.Remove(chosenBall); }
        if (orbitalOffsets.ContainsKey(chosenBall)) { orbitalOffsets.Remove(chosenBall); }

        Vector3 shootDirection = (player.position - chosenBall.position).normalized;

        float shootVelocity = 7f;
        Vector3 velocity = shootDirection * shootVelocity;

        Rigidbody rb = chosenBall.GetComponent<Rigidbody>();

        if (rb == null)
        {
            rb = chosenBall.AddComponent<Rigidbody>();
            rb.useGravity = false;
        }
        chosenBall.gameObject.AddComponent<SphereCollider>();
        rb.isKinematic = false;

        rb.AddForce(shootDirection * shootVelocity, ForceMode.VelocityChange);

        Destroy(chosenBall.gameObject, 5f);
    }    

    private void formSwarm()
    {
        foreach (Transform swarmBall in swarmBalls)
        {
            if (swarmBall == null) { continue; }

            Vector3 baseOrbitalPosition = CalculateOrbitalPosition(swarmBall);

            // Separation force
            Vector3 separationMove = CalculateSeparationForce(swarmBall);

            // Cohesion force to maintain spherical formation
            Vector3 toCenter = swarmCenter.position - baseOrbitalPosition;
            Vector3 cohesionMove = toCenter.normalized * cohesionStrength;

            // Combine forces and apply movement
            Vector3 finalMove = (cohesionMove + separationMove) * Time.deltaTime;
            Vector3 newPosition = baseOrbitalPosition + finalMove;

            swarmBall.position = newPosition;

        }
    }    

    private Vector3 CalculateOrbitalPosition(Transform swarmBall)
    {
        Vector3 orbitAxis = orbitalAxes[swarmBall];

        float uniqueSpeed = orbitalSpeeds[swarmBall];

        float uniqueOffset = orbitalOffsets[swarmBall];

        float angle = (Time.time * uniqueSpeed + uniqueOffset) % 360f;

        Quaternion rotation = Quaternion.AngleAxis(angle, orbitAxis);

        Vector3 orbitalOffset = rotation * (Vector3.forward * formationRadius);

        return swarmCenter.position + orbitalOffset;
    }

    private Vector3 CalculateSeparationForce(Transform ball)
    {
        Vector3 separationMove = Vector3.zero;
        foreach (Transform otherBall in swarmBalls)
        {
            if (otherBall == ball) continue;

            Vector3 separation = ball.position - otherBall.position;
            float distance = separation.magnitude;

            if (distance < separationRadius)
            {
                separationMove += separation.normalized / (distance + 0.1f) * separationForce;
            }
        }
        return separationMove;
    }

}