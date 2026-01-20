using UnityEngine;

namespace AIAction.Core
{
    public class CameraFollow : MonoBehaviour
    {
        [Header("Target")]
        public Transform target;
        
        [Header("Follow Settings")]
        public Vector3 offset = new Vector3(0, 2, -10);
        public float smoothSpeed = 5f;
        
        [Header("Bounds (Optional)")]
        public bool useBounds = false;
        public float minX = -100f;
        public float maxX = 100f;
        public float minY = -10f;
        public float maxY = 10f;

        private void Start()
        {
            // Auto-find player if not assigned
            if (target == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    target = player.transform;
                }
            }
        }

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 desiredPosition = target.position + offset;
            
            // Apply bounds if enabled
            if (useBounds)
            {
                desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
                desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);
            }
            
            // Smooth follow
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
            transform.position = smoothedPosition;
        }
    }
}
