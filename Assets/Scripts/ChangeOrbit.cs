using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// This class implements rotation of quark along it's orbit and transitions between random orbits.
/// </summary>
public class ChangeOrbit : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Maximum allowed orbit radius.")]
    private float maxOrbitRadius = 20f;

    [SerializeField]
    [Tooltip("Minimal allowed orbit radius.")]
    private float minOrbitRadius = 7.0f;

    [SerializeField]
    [Tooltip("Orbital velocity.")]
    private float orbitalVelocity = 300f;

    [SerializeField]
    [Tooltip("Time for quark to stay on the same orbit.")]
    private float timeOnTheSameOrbit = 10f;

    [SerializeField]
    [Tooltip("Velocity for orbit transition.")]
    private float orbitChangeVelocity = 100.0f;
    
    [SerializeField]
    private bool isPolarized = false;

    private bool isSmallOrbits = false;

    [SerializeField]
    private float currentRadius = 1.0f;

    private float targetRadius;

    private Tween tween;

    /// <summary>
    /// Current speed of the quark.
    /// </summary>
    [SerializeField]
    public float EstimatedSpeed { get; private set;} = 0.0f;

    /// <summary>
    /// Current orbit radius of the quark.
    /// </summary>
    public float CurrentOrbitRadius { get; private set; }

    /// <summary>
    /// Maximum possible radius for the quark.
    /// </summary>
    public float MaxRadius { get { return maxOrbitRadius; } }

    /// <summary>
    /// Is quark polarized right now.
    /// </summary>
    public bool IsPolarized { get { return isPolarized; } }

    /// <summary>
    /// Is quark in small orbits mode.
    /// </summary>
    public bool IsSmallOrbits { get { return isSmallOrbits; } }

    public float rad;
    public Vector3 tempRotVector;
    public int polarizationAxis = 2;

    /// <summary>
    /// Enable or disable small orbits mode for this quark.
    /// </summary>
    public void SetSmallRadius(bool isSmall)
    {
        if (isSmallOrbits == isSmall)
        {
            return;
        }

        StopAllCoroutines();

        maxOrbitRadius = isSmall ? (maxOrbitRadius / 10.0f) : (maxOrbitRadius * 10.0f);
        isSmallOrbits = isSmall;

        StartCoroutine(ChangeOrbitWithDelay(orbitChangeVelocity * 3.0f));
    }

    /// <summary>
    /// Enable or polarization small orbits mode for this quark.
    /// </summary>
    public void SetPolarization(bool polarize)
    {
        if (polarize == isPolarized)
        {
            return;
        }

        StopAllCoroutines();

        transform.position = polarize ? new Vector3(transform.position.x, transform.position.y, 0.0f) : new Vector3(transform.position.x, 0.0f, transform.position.z);
        isPolarized = polarize;

        StartCoroutine(ChangeOrbitWithDelay(orbitChangeVelocity * 3.0f));
    }

    private void Start()
    {
        //StartCoroutine(ChangeOrbitWithDelay(orbitChangeVelocity));

        // Enable polarization
        isPolarized = true;
        transform.position =  new Vector3(transform.position.x, transform.position.y, 0.0f);

        // Disable small orbits mode
        isSmallOrbits = false;
        StartCoroutine(ChangeOrbitWithDelay(orbitChangeVelocity * 3.0f));
    }

    private IEnumerator ChangeOrbitWithDelay(float changeVelocity)
    {
        
        while (true)
        {
            // Get new radius value
            targetRadius = RandomGaussian.Range(minOrbitRadius, maxOrbitRadius);
            // Calculate transition time based on velocity
            float difference = Mathf.Abs(transform.position.magnitude - targetRadius);
            float t = difference / changeVelocity;

            // Initiate new orbit transition
            tween?.Kill();
            // Smoothly blend current radius to a new value
            tween = DOTween.To(() => currentRadius, v => currentRadius = v, targetRadius, t);
            print("hello");
            tween.SetEase(Ease.InOutQuad);

            yield return new WaitForSeconds(timeOnTheSameOrbit);
        }
    }
    float counter = 0.0f;
    float axisChangeTime = .5f;
    // Vector3 tempRotVector = Vector3.zero;
    private void Update()
    {
        rad = EstimatedSpeed;
        Vector3 pos = transform.position;
        
        
        if(isPolarized){
            if(polarizationAxis == 0){
                tempRotVector = new Vector3(1.0f, 0.0f, 0.0f);
                transform.position = new Vector3(0.0f, transform.position.y, transform.position.z);
            }
            else if(polarizationAxis == 1){
                tempRotVector = new Vector3(0.0f, 1.0f, 0.0f);
                transform.position = new Vector3(transform.position.x, 0.0f, transform.position.z);

            }
            else if(polarizationAxis == 2){
                tempRotVector = new Vector3(0.0f, 0.0f, 1.0f);
                transform.position = new Vector3(transform.position.x , transform.position.y, 0.0f);

            }
        }
        else if(!isPolarized && counter < axisChangeTime)
            counter += Time.deltaTime;
        else{
            tempRotVector = Random.insideUnitCircle.normalized;
            counter = 0.0f;
            axisChangeTime = Random.Range(0.3f, 0.7f);
        }

        Vector3 rotationVector = tempRotVector;
        // Vector3 rotationVector = isPolarized ? Vector3.forward : Vector3.up;

        // First rotate current position along rotationVector with specified orbital speed
        // float speed = orbitalVelocity * QuarkSettings.Instance.GlobalSpeedMultiplier;
        float speed = orbitalVelocity;
        Vector3 rotated = Quaternion.AngleAxis(speed / pos.magnitude * Time.deltaTime, rotationVector) * pos;
        // Vector3 rotated = Quaternion.AngleAxis(speed * Time.deltaTime, rotationVector) * pos;


        // Then calculate new position based on direction from old rotation position and orbit radius
        Vector3 newPosition = rotated.normalized * currentRadius;
        // Estimate current seed and orbit radius for public getters
        EstimatedSpeed = Vector3.Distance(newPosition, transform.position) / Time.deltaTime;
        if(EstimatedSpeed < 6.5)
            axisChangeTime = .1f;
        CurrentOrbitRadius = newPosition.magnitude;
        transform.position = newPosition;

        if(isPolarized){
            if(polarizationAxis == 0){
                transform.position = new Vector3(0.0f, transform.position.y, transform.position.z);
            }
            else if(polarizationAxis == 1){
                transform.position = new Vector3(transform.position.x, 0.0f, transform.position.z);

            }
            else if(polarizationAxis == 2){
                transform.position = new Vector3(transform.position.x , transform.position.y, 0.0f);

            }
        }
    }
}
