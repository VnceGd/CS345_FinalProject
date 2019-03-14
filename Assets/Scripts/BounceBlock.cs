using UnityEngine;

public class BounceBlock : MonoBehaviour
{
    public float bounceForce = 20f;

    private void OnCollisionEnter(Collision collision)
    {
        Rigidbody collidedBody = collision.gameObject.GetComponent<Rigidbody>();
        if (collidedBody)
        {
            //collidedBody.AddForce(Vector3.up * bounceForce * 50f);
            collidedBody.velocity = new Vector3(collidedBody.velocity.x, bounceForce, collidedBody.velocity.z);
            Debug.Log("We're bouncing");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody otherBody = other.gameObject.GetComponent<Rigidbody>();
        if (otherBody)
        {
            otherBody.velocity = new Vector3(otherBody.velocity.x, bounceForce, otherBody.velocity.z);
        }
    }
}
