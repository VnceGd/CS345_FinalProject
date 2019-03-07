using UnityEngine;

public class BounceBlock : MonoBehaviour
{
    public float bounceForce = 20f;

    private void OnCollisionEnter(Collision collision)
    {
        Rigidbody collidedBody = collision.gameObject.GetComponent<Rigidbody>();
        if (collidedBody)
        {
            //collidedBody.AddForce(Vector3.up * bounceForce);
            collidedBody.velocity = new Vector3(collidedBody.velocity.x,bounceForce,collidedBody.velocity.z);
        }
    }
}
