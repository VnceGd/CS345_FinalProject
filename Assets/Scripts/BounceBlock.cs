using UnityEngine;

public class BounceBlock : MonoBehaviour
{
    public float bounceForce = 15f;

    private void OnCollisionEnter(Collision collision)
    {
        Rigidbody collidedBody = collision.gameObject.GetComponent<Rigidbody>();
        if (collidedBody)
        {
            collidedBody.velocity = Vector3.up * bounceForce;
        }
    }
}
