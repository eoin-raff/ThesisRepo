using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudController : MonoBehaviour
{ 
    private ParticleSystem cloudSystem;
   
    public Color color;
    public Color lining;
    bool painted = false;
    public int numberOfParticles;
    public float minSpeed;
    public float maxSpeed;
    public float distance;

    private Vector3 startPosition;
    private float speed;

    // Start is called before the first frame update
    void Start()
    {
        cloudSystem = GetComponent<ParticleSystem>();
        Spawn();
    }

    /// <summary>
    /// Spawn the cloud at a random position within the bounds of the CloudManager, with a random speed
    /// </summary>
    private void Spawn()
    {
        float x = UnityEngine.Random.Range(-0.5f, 0.5f);
        float y = UnityEngine.Random.Range(-0.5f, 0.5f);
        float z = UnityEngine.Random.Range(-0.5f, 0.5f);
        transform.localPosition = new Vector3(x, y, z);
        speed = UnityEngine.Random.Range(minSpeed, maxSpeed);
        startPosition = transform.position;
    }

    private void Paint()
    {
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[cloudSystem.particleCount];
        cloudSystem.GetParticles(particles);
        //We can't paint particles until the game is playing, so we might need to try a few times before it will run sucessfully
        if (particles.Length > 0)
        {
            for (int i = 0; i < particles.Length; i++)
            {
                //Make clouds darker at the bottom (lining) and lighter at the top (color)
                particles[i].startColor = Color.Lerp(lining, color, particles[i].position.y / cloudSystem.shape.scale.y);
            }
            painted = true;
            cloudSystem.SetParticles(particles, particles.Length);
        }
    }
    // Update is called once per frame
    void Update()
    {
        //Try to paint the clouds
        if (!painted)
        {
            Paint();
        }
        // Move the cloud
        transform.Translate(0, 0, speed);

        // If it has travelled the max allow distance, respawn it.
        if (Vector3.Distance(transform.position, startPosition) > distance)
        {
            Spawn();
        }
    }
}
