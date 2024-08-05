using System;
using System.Collections.Generic;
using UnityEngine;
using Obi;

public class ObiSovlerAdd : MonoBehaviour
{
    public ObiSolver solver;
    public GameObject colliderPrefab;
    private Dictionary<ObiRope, List<GameObject>> ropeColliders = new Dictionary<ObiRope, List<GameObject>>();
    private Guid _guid;

    void Start()
    {
        foreach (ObiActor actor in solver.actors)
        {
            if (actor is ObiRope rope)
            {
                _guid = System.Guid.NewGuid();
                AddCollidersToRope(rope);
            }
        }
    }

    void Update()
    {
        foreach (var entry in ropeColliders)
        {
            UpdateColliders(entry.Key, entry.Value);
        }
    }

    void AddCollidersToRope(ObiRope rope)
    {
        var blueprint = rope.blueprint;
        List<GameObject> colliders = new List<GameObject>();
        var getRope = rope.GetComponent<Rope>();
        for (int i = 0; i < blueprint.activeParticleCount; i++)
        {
            // Instantiate a collider from the prefab
            GameObject colliderObject = Instantiate(colliderPrefab);
            var getParticle = colliderObject.GetComponent<ParticleCollideCheck>();
            getParticle.Init( _guid.ToString());
            getParticle.transform.SetParent(rope.transform);
           getRope.particleCollideChecks.Add(getParticle);
            // Store the collider object in a list for later use
            colliders.Add(getParticle.gameObject);
        }
        ropeColliders.Add(rope, colliders);
        UpdateColliders(rope,colliders);
    }

    void UpdateColliders(ObiRope rope, List<GameObject> colliders)
    {
        var blueprint = rope.blueprint;

        for (int i = 0; i < blueprint.activeParticleCount; i++)
        {
            int solverIndex = rope.solverIndices[i];
            Vector4 particlePosition = solver.positions[solverIndex];

            // Update the position of each collider to match the corresponding particle
            colliders[i].transform.position = solver.transform.TransformPoint(particlePosition);
        }
    }
}