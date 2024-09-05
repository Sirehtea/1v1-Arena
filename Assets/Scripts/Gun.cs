using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Add this for UI components

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

    public Text ammoDisplay;

    //public AudioSource audioSource;
    public AudioSource shootSound;   // Sound when shooting
    public AudioSource reloadSound;  // Sound after reloading
    public AudioSource inspectGunSound; // Sound when firing with no ammo

    void Start()
    {
        if (currentAmmo == 99)
        {
            currentAmmo = maxAmmo;
        }

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
        muzzleFlash.Play();

        currentAmmo--;
        UpdateAmmoDisplay();

        if (shootSound != null)
        {
            shootSound.Play();
        }

        RaycastHit hit;
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range))
        {
            Debug.Log(hit.transform.name);

            Character character = hit.transform.GetComponent<Character>();
            if (character != null)
            {
                character.TakeDamage(damage);
            }

            Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
        }
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
}
