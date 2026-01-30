using UnityEngine;

namespace GodMode
{
    /// <summary>
    /// Simple WASD movement for the test Heretic.
    /// </summary>
    public class SimpleMovement : MonoBehaviour
    {
        [SerializeField] private float _speed = 5f;

        private void Update()
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            
            // Add deadzone to filter noise
            if (Mathf.Abs(h) < 0.1f) h = 0f;
            if (Mathf.Abs(v) < 0.1f) v = 0f;
            
            Vector3 dir = new Vector3(h, v, 0).normalized;

            if (dir.magnitude > 0) // Only move if there's actual input
            {
                transform.position += dir * _speed * Time.deltaTime;
            }
        }
    }
}
