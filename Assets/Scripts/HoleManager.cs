using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoleManager : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("ticker"))
        {
            Destroy(other.gameObject);
        }
        else
        {
            Rigidbody2D someRB = other.GetComponent<Rigidbody2D>();
            if (someRB)
            {
                someRB.velocity = new Vector2(0, 0);
            }
        }

    }
}
