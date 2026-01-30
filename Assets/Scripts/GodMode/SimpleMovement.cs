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
            Vector3 dir = new Vector3(h, v, 0).normalized;

            transform.position += dir * _speed * Time.deltaTime;
        }
    }
}
