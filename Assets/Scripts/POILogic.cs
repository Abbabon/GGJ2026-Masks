using UnityEngine;
using System.Collections;

public class POILogic : MonoBehaviour
{
    public enum StartMode
    {
        AutoStart,
        WaitForTrigger
    }

    [Header("Mode")]
    [SerializeField] private StartMode startMode = StartMode.AutoStart;

    [Header("Visuals")]
    [SerializeField] private GameObject buildingBuilt;
    [SerializeField] private GameObject buildingRuined;

    [Header("VFX")]
    [SerializeField] private ParticleSystem particleEffects;

    [Header("Timing")]
    [SerializeField] private float startDelay = 1f;
    [SerializeField] private float vfxLeadTime = 0.2f;
    [SerializeField] private float swapDelay = 0.4f;

    private bool isRuined = false;
    private bool isTransitioning = false;

    void Start()
    {
        buildingBuilt.SetActive(true);
        buildingRuined.SetActive(false);

        if (startMode == StartMode.AutoStart)
        {
            StartCoroutine(AutoStartRoutine());
        }
    }

    private IEnumerator AutoStartRoutine()
    {
        yield return new WaitForSeconds(startDelay);
        TriggerEffect();
    }

    /// <summary>
    /// Public trigger – can be called from UI Button, code, or events
    /// </summary>
    public void TriggerEffect()
    {
        if (isRuined || isTransitioning)
            return;

        StartCoroutine(TransitionRoutine());
    }

    private IEnumerator TransitionRoutine()
    {
        isTransitioning = true;

        // Safety: force particles to render on top
        var renderer = particleEffects.GetComponent<ParticleSystemRenderer>();
        renderer.sortingLayerName = "Characters";
        renderer.sortingOrder = 10;

        particleEffects.transform.position = transform.position;
        particleEffects.Play();

        // Particles start BEFORE swap
        yield return new WaitForSeconds(vfxLeadTime);
        yield return new WaitForSeconds(swapDelay);

        buildingBuilt.SetActive(false);
        buildingRuined.SetActive(true);

        isRuined = true;
        isTransitioning = false;
    }
}
