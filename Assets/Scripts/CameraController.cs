using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Boundaries")]
    public float minX = -10f;
    public float maxX = 10f;

    [Header("Start Position")]
    public float startX = -10f;

    private float fixedY;
    private float fixedZ;

    private bool isDragging = false;
    private Vector3 dragOrigin; // เปลี่ยนมาเก็บตำแหน่งเริ่มลากแบบ Vector3

    void Start()
    {
        fixedY = transform.position.y;
        fixedZ = transform.position.z;
        transform.position = new Vector3(startX, fixedY, fixedZ);
    }

    void Update()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        HandleMouseInput();
#else
        HandleTouchInput();
#endif
    }

    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            dragOrigin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        if (Input.GetMouseButton(0) && isDragging)
        {
            Vector3 difference = dragOrigin - Camera.main.ScreenToWorldPoint(Input.mousePosition);
            MoveCamera(transform.position.x + difference.x);
            dragOrigin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        if (Input.GetMouseButtonUp(0)) isDragging = false;
    }

    void HandleTouchInput()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                isDragging = true;
                dragOrigin = Camera.main.ScreenToWorldPoint(touch.position);
            }

            if ((touch.phase == TouchPhase.Moved) && isDragging)
            {
                Vector3 difference = dragOrigin - Camera.main.ScreenToWorldPoint(touch.position);
                MoveCamera(transform.position.x + difference.x);
                dragOrigin = Camera.main.ScreenToWorldPoint(touch.position);
            }

            if (touch.phase == TouchPhase.Ended) isDragging = false;
        }
    }

    void MoveCamera(float targetX)
    {
        float clampedX = Mathf.Clamp(targetX, minX, maxX);
        transform.position = new Vector3(clampedX, fixedY, fixedZ);
    }
}