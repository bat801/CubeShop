using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Настройки камеры")]
    [SerializeField] private Transform target; // Центр магазина
    [SerializeField] private float distance = 15f;
    [SerializeField] private float height = 10f;
    [SerializeField] private float rotationSpeed = 2f;
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float minDistance = 5f;
    [SerializeField] private float maxDistance = 25f;

    private float currentAngle = 0f;
    private float currentDistance;
    private bool isDragging = false;
    private Vector3 lastMousePosition;

    void Start()
    {
        if (target == null)
        {
            // Создаем пустышку в центре
            GameObject center = new GameObject("CameraTarget");
            center.transform.position = Vector3.zero;
            target = center.transform;
        }

        currentDistance = distance;
        UpdateCameraPosition();
    }

    void Update()
    {
        // Вращение камеры
        if (Input.GetMouseButtonDown(1)) // ПКМ зажата
        {
            isDragging = true;
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(1))
        {
            isDragging = false;
        }

        if (isDragging)
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            currentAngle += delta.x * rotationSpeed * Time.deltaTime;
            lastMousePosition = Input.mousePosition;
            UpdateCameraPosition();
        }

        // Зум
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            currentDistance -= scroll * zoomSpeed;
            currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
            UpdateCameraPosition();
        }

        // Альтернативное вращение с клавиатуры (A/D)
        if (Input.GetKey(KeyCode.A))
        {
            currentAngle -= rotationSpeed * 2f * Time.deltaTime;
            UpdateCameraPosition();
        }
        if (Input.GetKey(KeyCode.D))
        {
            currentAngle += rotationSpeed * 2f * Time.deltaTime;
            UpdateCameraPosition();
        }
    }

    private void UpdateCameraPosition()
    {
        if (target == null) return;

        Quaternion rotation = Quaternion.Euler(0, currentAngle, 0);
        Vector3 offset = new Vector3(0, height, -currentDistance);
        Vector3 desiredPosition = target.position + rotation * offset;

        transform.position = desiredPosition;
        transform.LookAt(target.position);
    }

    // Для отладки - показать угол в редакторе
    private void OnDrawGizmosSelected()
    {
        if (target == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(target.position, 0.5f);
    }
}