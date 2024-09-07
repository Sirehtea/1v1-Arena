using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Gun : MonoBehaviour
{
    public float damage = 10f;
    public float range = 100f;
    public float fireRate = 15f;

    public int maxAmmo = 6;
    private int currentAmmo = 99;
    public float reloadTime = 2.25f;
    private bool isReloading = false;

    public Camera fpsCam;
    public ParticleSystem muzzleFlash;
    public GameObject impactEffect;

    private float nextTimeToFire = 0f;

    public Animator animator;
    public Animator inspectAnimation;
    public Animator shootAnimation;

    public Text ammoDisplay;

    public AudioSource shootSound;
    public AudioSource reloadSound;
    public AudioSource inspectGunSound;

    public TrailRenderer bulletTrail; // Add the bullet trail renderer
    public Transform bulletSpawnPoint; // Where the bullets will be fired from
    public float bulletSpeed = 100f; // Speed at which the bullet travels

    // Accuracy and movement detection variables
    public float maxSpread = 1f; // Maximum inaccuracy while moving
    public float jumpFallSpreadMultiplier = 1.5f; // Extra inaccuracy while jumping or falling
    private bool isMoving = false; // Track if the player is moving
    private bool isJumpingOrFalling = false; // Track if the player is in the air

    private CharacterController characterController; // To hold the reference to the parent CharacterController

    // Reference to the MouseLook script for recoil
    private MouseLook mouseLook;

    void Start()
    {
        if (currentAmmo == 99)
        {
            currentAmmo = maxAmmo;
        }

        characterController = GetComponentInParent<CharacterController>();

        if (characterController == null)
        {
            Debug.LogError("No CharacterController found in parent objects! Please attach the script to the correct game object.");
        }

        // Get the MouseLook script from the camera for recoil
        mouseLook = fpsCam.GetComponent<MouseLook>();

        UpdateAmmoDisplay();
    }

    private void OnEnable()
    {
        isReloading = false;
        animator.SetBool("Inspecting", false);
        animator.SetBool("Reloading", false);
        UpdateAmmoDisplay();
    }

    void Update()
    {
        if (isReloading)
        {
            return;
        }

        isMoving = CheckPlayerMovement();

        if (characterController != null)
        {
            isJumpingOrFalling = !characterController.isGrounded;
        }

        if (currentAmmo <= 0 || Input.GetKeyDown(KeyCode.R) && currentAmmo < maxAmmo)
        {
            StartCoroutine(Reload());
            return;
        }

        if (Input.GetButtonDown("Fire1") && Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + 1f / fireRate;
            Shoot();
        }

        if (Input.GetKey(KeyCode.Mouse3))
        {
            StartCoroutine(PlayInspect());
        }
    }

    IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log("RELOADING");

        animator.SetBool("Reloading", true);

        yield return new WaitForSeconds(0.25f);

        // Play reload sound
        if (reloadSound != null)
        {
            reloadSound.Play();
        }

        yield return new WaitForSeconds(reloadTime + 0.3f);

        animator.SetBool("Reloading", false);

        yield return new WaitForSeconds(0.25f);

        currentAmmo = maxAmmo;
        isReloading = false;

        UpdateAmmoDisplay();
    }

    void Shoot()
    {
        StartCoroutine(PlayShoot());
        muzzleFlash.Play();

        currentAmmo--;
        UpdateAmmoDisplay();

        // Apply spread based on whether the player is moving or in the air
        Vector3 shootDirection = fpsCam.transform.forward;

        if (isMoving)
        {
            shootDirection.x += Random.Range(-maxSpread, maxSpread);
            shootDirection.y += Random.Range(-maxSpread, maxSpread);
        }

        if (isJumpingOrFalling)
        {
            shootDirection.x += Random.Range(-maxSpread * jumpFallSpreadMultiplier, maxSpread * jumpFallSpreadMultiplier);
            shootDirection.y += Random.Range(-maxSpread * jumpFallSpreadMultiplier, maxSpread * jumpFallSpreadMultiplier);
        }

        // Apply recoil to the camera
        if (mouseLook != null)
        {
            float recoilX = Random.Range(-2f, 2f);  // Random horizontal recoil
            float recoilY = Random.Range(-5f, 0f);  // Random vertical recoil (mostly upwards)
            mouseLook.ApplyRecoil(recoilX, recoilY);
        }

        RaycastHit hit;
        if (Physics.Raycast(fpsCam.transform.position, shootDirection, out hit, range))
        {
            Debug.Log(hit.transform.name);

            // Apply damage if character was hit
            Character character = hit.transform.GetComponent<Character>();
            if (character != null)
            {
                character.TakeDamage(damage);
            }

            // Instantiate bullet trail and impact effects
            TrailRenderer trail = Instantiate(bulletTrail, bulletSpawnPoint.position, Quaternion.identity);
            StartCoroutine(SpawnTrail(trail, hit.point, hit.normal, true));

            Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
        }
        else
        {
            // If no hit, still show a trail towards an arbitrary long distance
            TrailRenderer trail = Instantiate(bulletTrail, bulletSpawnPoint.position, Quaternion.identity);
            StartCoroutine(SpawnTrail(trail, fpsCam.transform.position + shootDirection * range, Vector3.zero, false));
        }
    }

    IEnumerator SpawnTrail(TrailRenderer trail, Vector3 hitPoint, Vector3 hitNormal, bool madeImpact)
    {
        Vector3 startPosition = trail.transform.position;
        float distance = Vector3.Distance(trail.transform.position, hitPoint);
        float remainingDistance = distance;

        while (remainingDistance > 0)
        {
            trail.transform.position = Vector3.Lerp(startPosition, hitPoint, 1 - (remainingDistance / distance));
            remainingDistance -= bulletSpeed * Time.deltaTime;
            yield return null;
        }

        trail.transform.position = hitPoint;

        // If impact was made, instantiate the impact effect
        if (madeImpact)
        {
            Instantiate(impactEffect, hitPoint, Quaternion.LookRotation(hitNormal));
        }

        // Destroy the trail after some time
        Destroy(trail.gameObject, trail.time);
    }

    void UpdateAmmoDisplay()
    {
        if (ammoDisplay != null)
        {
            ammoDisplay.text = "AMMO: " + currentAmmo + "/" + maxAmmo;
        }
    }

    IEnumerator PlayInspect()
    {
        Debug.Log("INSPECTING");

        inspectAnimation.SetBool("Inspecting", true);

        yield return new WaitForSeconds(0.24f);

        inspectAnimation.SetBool("Inspecting", false);

        if (inspectGunSound != null)
        {
            inspectGunSound.Play();
        }
    }

    private bool CheckPlayerMovement()
    {
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) ||
            Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
        {
            return true; // Player is moving
        }

        return false; // Player is standing still
    }

    IEnumerator PlayShoot()
    {
        Debug.Log("SHOOTING");

        if (shootSound != null)
        {
            shootSound.Play();
        }

        shootAnimation.SetBool("Shooting", true);

        yield return new WaitForSeconds(1f);

        shootAnimation.SetBool("Shooting", false);
    }
}
