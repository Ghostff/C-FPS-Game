using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Gun : MonoBehaviour {

	public int gunDamage = 1;                                           // Set the number of hitpoints that this gun will take away from shot objects with a health script
	public float fireRate = 0.25f;                                      // Number in seconds which controls how often the player can fire
	public float weaponRange = 50f;                                     // Distance in Unity units over which the player can fire
	public float hitForce = 100f;                                       // Amount of force which will be added to objects with a rigidbody shot by the player
	public Transform gunEnd;                                            // Holds a reference to the gun end object, marking the muzzle location of the gun
	public bool rapidFire = true;
    public int clipSize;
    public int accuracy;
    public int spray;
    public GameObject updatePane;
    public GameObject crossHair;
    public int AmmoPack = 4;



    private int clipSizeBuffer;
    private bool zoomed = false;
    private int maxRange = 100;
    private Text pane;
    private Image target;
	private Camera fpsCam;                                              // Holds a reference to the first person camera
	private WaitForSeconds shotDuration = new WaitForSeconds(0.07f);    // WaitForSeconds object used by our ShotEffect coroutine, determines time laser line will remain visible
	private AudioSource gunAudio;                                       // Reference to the audio source which will play our shooting sound effect
	private LineRenderer laserLine;                                     // Reference to the LineRenderer component which will display our laserline
	private float nextFire;                                             // Float to store the time the player will be allowed to fire again, after firing




    void forge()
    {

    }

	void Start () 
	{
        pane = updatePane.GetComponent<Text>();
        target = crossHair.GetComponent<Image>();
        clipSizeBuffer = clipSize;

        pane.text = AmmoDetails(clipSize, AmmoPack);

		// Get and store a reference to our LineRenderer component
		laserLine = GetComponent<LineRenderer>();
        
		// Get and store a reference to our AudioSource component
		gunAudio = GetComponent<AudioSource>();

		// Get and store a reference to our Camera by searching this GameObject and its parents
		fpsCam = GetComponentInParent<Camera>();
        
	}

    string AmmoDetails(int clip_size, int ammo)
    {
        return clip_size + " / " + ammo;
    }

    int GunDamageFromRange(int distance, string tag)
    {
        int damage;
        if (distance < maxRange)
        {
            int compensate = distance / 3;
            damage = gunDamage - compensate;
        }
        else
        {
            int range = gunDamage - distance;
            damage = range * 2;
        }

        if (tag == "head")
        {
            damage *= 20;
        }

        return damage;
    }

	void Shoot()
	{
        if (clipSize > 0)
        {
            clipSize--;
            // Update the time when our player can fire next
            nextFire = Time.time + fireRate;

            // Start our ShotEffect coroutine to turn our laser line on and off
            StartCoroutine(ShotEffect());

            // Create a vector at the center of our camera's viewport
            Vector3 rayOrigin = fpsCam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0.0f));

            // Declare a raycast hit to store information about what our raycast has hit
            RaycastHit hit;

            // Set the start position for our visual effect for our laser to the position of gunEnd
            laserLine.SetPosition(0, gunEnd.position);

            // Check if our raycast has hit anything
            if (Physics.Raycast(rayOrigin, fpsCam.transform.forward, out hit, 200f))
            {
                // Set the end position for our laser line 
                laserLine.SetPosition(1, hit.point);

                // Get a reference to a health script attached to the collider we hit
                Shootable health = hit.collider.GetComponent<Shootable>();

                // If there was a health script attached
                if (health != null && health.tag == "mortal")
                {
                    // Call the damage function of that script, passing in our gunDamage variable
                    health.Damage(GunDamageFromRange((int)Vector3.Distance(hit.transform.position, gunEnd.position), hit.collider.tag));
                }

                // Check if the object we hit has a rigidbody attached
                if (hit.rigidbody != null)
                {
                    // Add force to the rigidbody we hit, in the direction from which it was hit
                    hit.rigidbody.AddForce(-hit.normal * hitForce);
                }
            }
            else
            {
                // If we did not hit anything, set the end of the line to a position directly in front of the camera at the distance of weaponRange
                laserLine.SetPosition(1, rayOrigin + (fpsCam.transform.forward * weaponRange));
            }


        }


	}

	void Update () 
	{

        if (Input.GetButton("Zoom"))
        {
            zoomed = true;
            fpsCam.fieldOfView = 40;
            target.enabled = false;
            transform.localPosition = new Vector3(0.0103f, -0.171f, 0.3912073f);
            transform.localRotation = Quaternion.Euler(-3.55f, 0f, 0f);
            transform.localScale = new Vector3(2.29f, 2.3f, 1f);
        }
        else
        {
            if (zoomed)
            {
                zoomed = false;
                fpsCam.fieldOfView = 60;
                target.enabled = true;
                transform.localPosition = new Vector3(0.187f, -0.247f, 0.75f);
                transform.localRotation = Quaternion.Euler(-0.228f, -5.216f, 0.021f);
                transform.localScale = new Vector3(2.5f, 2.5f, 2f);
            }
        }

        if (Input.GetButtonDown("Reload") && clipSize < clipSizeBuffer && AmmoPack > 0)
        {
            clipSize = clipSizeBuffer;
            AmmoPack--;
        }
		// Check if the player has pressed the fire button and if enough time has elapsed since they last fired
		if (rapidFire && Input.GetButton("Fire1") && Time.time > nextFire) 
		{
            Shoot();
		}
		if ( ! rapidFire && Input.GetButtonDown("Fire1") && Time.time > nextFire) 
		{
            Shoot();
		}
        pane.text = AmmoDetails(clipSize, AmmoPack);

	}


	private IEnumerator ShotEffect()
	{
		// Play the shooting sound effect
		gunAudio.Play ();

		// Turn on our line renderer
		laserLine.enabled = true;

		//Wait for .07 seconds
		yield return shotDuration;

		// Deactivate our line renderer after waiting
		laserLine.enabled = false;
	}
}