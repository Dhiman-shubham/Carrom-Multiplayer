using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StrikerController_backup : MonoBehaviour
{
    Vector3 initalPosition;
    public float MAX_SHOT_PULL_DIST = 0.65f;
    public int SHOT_FORCE_MULTIPLYER = 20;
    public float MIN_SHOT_PULL_DIST = 0.24f;
    public float MIN_STOP_VELOCITY = 0.1f;

    [SerializeField]
    Slider strikerPositionSlider;

    [SerializeField]
    GameObject strikerPower;

    [SerializeField]
    GameObject strikerBaseGlow;

    [SerializeField]
    GameObject strikerOverlapAlert;

    bool strikerSelected;
    RaycastHit2D raycastHit;

    Rigidbody2D strikerRigidbody;

    Collider2D strikerCollider;

    delegate void callbackDelegate();

    enum StrikerState { Idle, Aim, Engaged, Blocked }

    [SerializeField]
    StrikerState currentStrikerState;

    private Vector2 strickerPos, currentPos, pullDirection, currentStrikeForce;
    private float pullDistance;

    void UpdateStrikerState(StrikerState newVal)
    {
        switch (newVal)
        {
            case StrikerState.Idle:
                callbackDelegate resetToIdle = () =>
                {
                    toggleObjectAfterChecking(strikerBaseGlow, true);
                    toggleObjectAfterChecking(strikerPower, false);
                    strikerCollider.isTrigger = true;
                    pullDistance = 0;
                    strikerPositionSlider.interactable = true;
                    strikerPositionSlider.value = 0;
                    strikerOverlapAlert.SetActive(false);
                };
                if (currentStrikerState == StrikerState.Engaged)
                {
                    StartCoroutine(stopWatch(2.0f, () =>
                    {
                        resetToIdle();
                        this.transform.position = initalPosition;
                        currentStrikerState = newVal;
                    }));
                }
                else
                {
                    resetToIdle();
                    currentStrikerState = newVal;
                }
                break;
            case StrikerState.Aim:
                toggleObjectAfterChecking(strikerBaseGlow, false);
                toggleObjectAfterChecking(strikerPower, true);
                strickerPos = new Vector2(this.transform.position.x, this.transform.position.y);
                currentStrikerState = newVal;
                strikerPositionSlider.interactable = false;
                break;
            case StrikerState.Engaged:
                if (currentStrikerState != StrikerState.Aim)
                {
                    return;
                }
                toggleObjectAfterChecking(strikerBaseGlow, false);
                toggleObjectAfterChecking(strikerPower, false);
                strikerCollider.isTrigger = false;
                strikerRigidbody.AddForce(currentStrikeForce, ForceMode2D.Impulse);
                currentStrikerState = newVal;
                break;
            case StrikerState.Blocked:
                currentStrikerState = newVal;
                strikerOverlapAlert.SetActive(true);
                break;
            default:
                Debug.LogError("No exceution for " + newVal);
                break;
        }
    }

    void Start()
    {
        //initalizing 
        strikerRigidbody = GetComponent<Rigidbody2D>();
        strikerCollider = GetComponent<Collider2D>();
        UpdateStrikerState(StrikerState.Idle);

        initalPosition = this.transform.position;

        //Add Listener for mainStriker Position
        strikerPositionSlider.onValueChanged.AddListener(handleStrikerSliderChange);
    }

    private void Update()
    {
        if (Input.GetMouseButton(0) && currentStrikerState == StrikerState.Idle)
        {
            raycastHit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector3.forward);

            if (raycastHit.collider)
            {
                if (raycastHit.transform.GetComponent<StrikerController_backup>())
                {
                    UpdateStrikerState(StrikerState.Aim);
                }
            }
        }
        if (Input.GetMouseButtonUp(0) && currentStrikerState == StrikerState.Aim)
        {
            if (pullDistance < MIN_SHOT_PULL_DIST)
            {
                UpdateStrikerState(StrikerState.Idle);
            }
            else
            {
                UpdateStrikerState(StrikerState.Engaged);
            }
        }
        processStrikerState();
    }

    void processStrikerState()
    {
        switch (currentStrikerState)
        {
            case StrikerState.Idle:
                break;
            case StrikerState.Aim:
                currentPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                pullDistance = Mathf.Clamp(Vector2.Distance(strickerPos, currentPos), 0, MAX_SHOT_PULL_DIST);
                pullDirection = (strickerPos - currentPos).normalized;
                currentStrikeForce = pullDirection * pullDistance * SHOT_FORCE_MULTIPLYER;

                float angle = Mathf.Atan2(pullDirection.y, pullDirection.x) * Mathf.Rad2Deg - 90;
                transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                strikerPower.transform.localScale = new Vector3(1, 1, 1) * pullDistance;
                break;
            case StrikerState.Engaged:
                if (strikerRigidbody.velocity.sqrMagnitude < MIN_STOP_VELOCITY * MIN_STOP_VELOCITY)
                {
                    UpdateStrikerState(StrikerState.Idle);
                }
                break;
            default:
                break;
        }
    }

    IEnumerator stopWatch(float time, callbackDelegate cb)
    {
        yield return new WaitForSeconds(time);
        cb.Invoke();
    }
    void handleStrikerSliderChange(float value)
    {
        if (currentStrikerState == StrikerState.Idle || currentStrikerState == StrikerState.Blocked)
        {
            transform.position = new Vector3(value, initalPosition.y, initalPosition.z);
        }
    }
    void toggleObjectAfterChecking(GameObject obj, bool val)
    {
        if (obj.activeSelf != val)
        {
            obj.SetActive(val);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (currentStrikerState != StrikerState.Idle)
        {
            return;
        }

        if (other.CompareTag("ticker"))
        {
            UpdateStrikerState(StrikerState.Blocked);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("ticker"))
        {
            UpdateStrikerState(StrikerState.Idle);
        }
    }
}
