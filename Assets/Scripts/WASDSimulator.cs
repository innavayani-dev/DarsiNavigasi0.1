using UnityEngine;

public class WASDSimulator : MonoBehaviour
{
    public float moveSpeed = 3f;

    private float rotationX = 0f;
    private float rotationY = 0f;

    void Start()
    {
#if UNITY_EDITOR
        Vector3 rot = transform.localRotation.eulerAngles;
        rotationY = rot.y;
        rotationX = rot.x;

        // Nonaktifkan semua varian TrackedPoseDriver dan AR Components di Editor
        // Supaya tidak nabrak error "[ARFoundationSupport] Could not acquire camera intrinsics"
        foreach (var comp in GetComponents<Behaviour>())
        {
            string compName = comp.GetType().Name;
            if (compName.Contains("TrackedPoseDriver") || 
                compName.Contains("ARCameraManager") || 
                compName.Contains("ARCameraBackground") || 
                compName.Contains("AROcclusionManager"))
            {
                comp.enabled = false;
            }
        }
#endif
    }

    void Update()
    {
#if UNITY_EDITOR
        // Gerakan WASD: W = Maju, S = Mundur, A = Kiri, D = Kanan relatif terhadap arah pandangan
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Mengubah arah input (WASD) sesuai arah rotasi kamera saat ini
        Vector3 moveDirection = new Vector3(horizontal, 0f, vertical);
        moveDirection = transform.TransformDirection(moveDirection);
        // Kunci sumbu Y agar tidak terbang saat berjalan sambil melihat ke atas
        moveDirection.y = 0f;
        
        transform.position += moveDirection.normalized * moveSpeed * Time.deltaTime;

        // Rotasi Kamera menggunakan Klik Kanan Mouse (Melihat 360 derajat)
        if (Input.GetMouseButton(1))
        {
            rotationX -= Input.GetAxis("Mouse Y") * 2f; // lookSpeed
            rotationY += Input.GetAxis("Mouse X") * 2f;

            rotationX = Mathf.Clamp(rotationX, -90f, 90f);

            transform.localRotation = Quaternion.Euler(rotationX, rotationY, 0f);
        }
#endif
    }
}
