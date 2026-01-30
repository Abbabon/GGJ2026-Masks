using System;
using UnityEngine;
using UnityEngine.Events;

public class Actionable : MonoBehaviour
{
    [Header("Action (hold to complete)")]
    [SerializeField] float actionDurationSeconds = 1f;
    public UnityEvent onActionComplete;

    bool isActionActive;
    float actionTimer;
    bool actionCompletedThisHold;
    private Transform _fillScaler;
    private GameObject _fill;
    private POILogic _poiLogic;

    void Start()
    {
        _fill = transform.Find("BarFill").gameObject;
        _fillScaler = transform.Find("BarFill/BarBG/BarFill_Container");
        _fill.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("POI"))
        {
            _poiLogic = other.gameObject.GetComponent<POILogic>();
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("POI"))
        {
            _poiLogic = null;
        }
    }

    public float ActionDurationSeconds
    {
        get => actionDurationSeconds;
        set => actionDurationSeconds = value;
    }

    /// <summary>Start the action (e.g. button pressed). Hold for ActionDurationSeconds to fire onActionComplete.</summary>
    public void ActionStart()
    {
        if (_poiLogic == null || _poiLogic.isRuined)
        {
            Debug.Log("POI not in initial state");
            return;
        }
        _fill.SetActive(true);
        isActionActive = true;
        actionCompletedThisHold = false;
        _fillScaler.localScale *= new Vector2(0, 1);
        Debug.Log("START");
    }

    /// <summary>Stop the action (e.g. button released). Resets hold progress.</summary>
    public void ActionStop()
    {
        isActionActive = false;
        actionTimer = 0f;
        _poiLogic = null;
        _fillScaler.localScale *= new Vector2(0, 1);
        actionCompletedThisHold = false;
        _fill.SetActive(false);
    }

    

    void Update()
    {
        if (isActionActive)
        {
            actionTimer += Time.deltaTime;
            _fillScaler.localScale = new Vector2(actionTimer / actionDurationSeconds, _fillScaler.localScale.y);
            if (actionTimer >= actionDurationSeconds && !actionCompletedThisHold)
            {
                if (_poiLogic)
                {
                    _poiLogic.TriggerEffect();
                }
                
                this.ActionStop();
                onActionComplete?.Invoke();
            }
        }
    }
}
