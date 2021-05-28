using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeDestroy : MonoBehaviour
{
    public float destroyTime = 1;

    public bool waitTillGround;
    public bool spawnParticles;
    public GameObject particlePrefab;
    public bool shrink;
    float shrinkTimer;

    private void Start()
    {
        shrinkTimer = destroyTime;
    }

    void Update()
    {
        if(waitTillGround)
        {
            if (!Physics.Raycast(transform.position, Vector3.down, 1.0f))
                return;
        }

        Destroy(this.gameObject, destroyTime);

        if (spawnParticles)
        {
            spawnParticles = false;
            Instantiate(particlePrefab, transform);
        }

        if (shrink)
        {
            var scale = Mathf.Lerp(0,1, shrinkTimer/(destroyTime - 0.75f));

            transform.localScale = new Vector3(scale,scale,scale);

            shrinkTimer -= Time.deltaTime;
        }
    }
}
