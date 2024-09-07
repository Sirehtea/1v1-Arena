using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public float mouseSensitivity = 1000f;
    public Transform playerBody;

    public float xRotation = 0f;
    public float recoilX = 0f;
    public float recoilY = 0f;

    public float recoilRecoverySpeed = 10f; // Speed to recover from recoil
    public float maxRecoilX = 0f; // Max horizontal recoil
    public float maxRecoilY = 0.2f; // Max vertical recoil

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // Mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Apply recoil recovery (smoothly go back to no recoil when not shooting)
        recoilX = Mathf.Lerp(recoilX, 0f, Time.deltaTime * recoilRecoverySpeed);
        recoilY = Mathf.Lerp(recoilY, 0f, Time.deltaTime * recoilRecoverySpeed);

        // Apply mouse input combined with recoil (inverted recoilY to make camera move up instead of down)
        xRotation -= (mouseY - recoilY);  // Subtract recoilY to move the camera up
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Limit vertical look to prevent over-rotation

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * (mouseX + recoilX));
    }

    // Method to apply recoil
    public void ApplyRecoil(float recoilAmountX, float recoilAmountY)
    {
        recoilX += recoilAmountX;
        recoilY += recoilAmountY;

        // Clamp the recoil to prevent excessive movement
        recoilX = Mathf.Clamp(recoilX, -maxRecoilX, maxRecoilX);
        recoilY = Mathf.Clamp(recoilY, -maxRecoilY, maxRecoilY);
    }
}