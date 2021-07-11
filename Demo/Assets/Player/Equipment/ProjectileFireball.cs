using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileFireball : MonoBehaviour
{
    private Rigidbody rigid_body;
    private float speed = 0.5f;
    private float lifetime;
    // Start is called before the first frame update
    void Start()
    {
        rigid_body = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        lifetime += Time.deltaTime;
        if (lifetime >= 0.5)
        {
            transform.localScale = new Vector3(6, 6, 6);
            if (lifetime >= 1.5)
            {
                Destroy(gameObject);
            }
        }
        else 
        {
            rigid_body.MovePosition(transform.position + transform.forward * speed);
        }
        
    }
}
