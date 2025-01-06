using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Camera Settings")]
    public Transform cameraTransform; // Ссылка на камеру
    public float moveSpeed = 1f; // Скорость перемещения камеры
    public float rotationSpeed = 5f; // Скорость вращения камеры
    public float zoomSpeed = 5f; // Скорость масштабирования
    public float smoothTransitionTime = 0.5f; // Время плавного перехода

    [Header("Selection")]
    public LayerMask selectableLayer; // Слой интерактивных объектов
    [SerializeField] private PlayerInteraction playerInteraction;


    [Header("Exploration Settings")]
    public Transform explorationStartPoint;
    public Transform handCardView; // Фиксированная точка для просмотра карт

    [Header("Camera Rotation Limits")]
    public float minVerticalAngle = -90f; // Минимальный угол наклона
    public float maxVerticalAngle = 60f;  // Максимальный угол наклона

    private bool isTransitioning = false;

    private PlayerControls controls;
    private SelectionableObject highlightedObject; // Текущий выделенный объект

    private bool isInHandCardView = false; // Находимся ли в режиме просмотра карт
    private bool isRotateMode = false; 
    private Vector3 targetPosition; // Целевая позиция камеры
    private Quaternion targetRotation; // Целевое вращение камеры

    private Vector2 moveInput;
    private Vector2 rotateInput;
    private float zoomInput;

    private Camera mainCamera;
    private Collider[] barriers; // Динамически найденные барьеры

    private Vector3 currentVelocity; // Для плавного движения камеры
    private Vector3 rotationVelocity; // Для плавного вращения



    private void Awake()
    {
        controls = new PlayerControls();
    }

    private void OnEnable()
    {
        if(controls == null) controls = new PlayerControls();
        controls.Enable();

        controls.Camera.Move.started += ctx => OnMove(ctx.ReadValue<Vector2>());
        controls.Camera.Move.canceled += ctx => OnMove(Vector2.zero);
        controls.Camera.Rotate.performed += ctx => OnRotate(ctx.ReadValue<Vector2>());
        controls.Camera.EnableRotateMode.performed += ctx => OnRotateMode(true);
        controls.Camera.EnableRotateMode.canceled += ctx => OnRotateMode(false);
        controls.Camera.Zoom.performed += ctx => OnZoom(ctx.ReadValue<float>());
        controls.Camera.Zoom.canceled += ctx => OnZoom(0f);

        controls.Camera.SwitchToHandCardView.performed += _ => OnSwitchToHandCardView();
        controls.Camera.SwitchToExploration.performed += _ => OnSwitchToExploration();
    }

    private void OnDisable()
    {
        controls.Disable();
    }


    private void Start()
    {
        mainCamera = Camera.main;
        targetPosition = cameraTransform.position;
        targetRotation = cameraTransform.rotation;

        // Находим барьеры динамически
        barriers = FindBarriers();
    }

    private Collider[] FindBarriers()
    {
        int barrierLayer = LayerMask.NameToLayer("Barrier");
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        var barrierColliders = new System.Collections.Generic.List<Collider>();

        foreach (var obj in allObjects)
        {
            if (obj.layer == barrierLayer)
            {
                Collider collider = obj.GetComponent<Collider>();
                if (collider != null)
                {
                    barrierColliders.Add(collider);
                }
            }
        }

        return barrierColliders.ToArray();
    }


    private void Update()
    {
        if (isInHandCardView || isTransitioning)
        {
            return; // Блокируем управление камерой
        }



        // Обрабатываем ввод для движения камеры
        Vector3 targetMove = cameraTransform.position + (cameraTransform.forward * moveInput.y + cameraTransform.right * moveInput.x) * moveSpeed * Time.deltaTime;
        cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetMove, 0.1f);
        //moveInput = Vector3.Lerp(moveInput, Vector3.zero, 5f*Time.deltaTime);
        // Ограничиваем движение камерой барьерами
        ClampCameraToBoundaries();

        // Остальные действия: вращение, масштабирование, подсветка объектов
        if(isRotateMode) HandleRotation();
        HandleZoom();
        HandleSelection();
    }

    private void ClampCameraToBoundaries()
    {
        // Берём пересечение всех границ барьеров
        Bounds combinedBounds = new Bounds();
        bool initialized = false;

        foreach (var barrier in barriers)
        {
            if (!initialized)
            {
                combinedBounds = barrier.bounds;
                initialized = true;
            }
            else
            {
                combinedBounds.Encapsulate(barrier.bounds);
            }
        }

        // Ограничиваем положение камеры
        Vector3 clampedPosition = cameraTransform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, combinedBounds.min.x, combinedBounds.max.x);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, combinedBounds.min.y, combinedBounds.max.y);
        clampedPosition.z = Mathf.Clamp(clampedPosition.z, combinedBounds.min.z, combinedBounds.max.z);

        cameraTransform.position = clampedPosition;
    }

    private void HandleSelection()
    {
        if (highlightedObject != null)
        {
            // Убираем выделение, если объект больше не подсвечивается
            highlightedObject.UnSelection();
            highlightedObject = null;
        }

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, selectableLayer))
        {
            SelectionableObject selectedObject = hit.collider.GetComponent<SelectionableObject>();
            if (selectedObject != null && selectedObject != highlightedObject)
            {
                // Подсвечиваем объект
                highlightedObject = selectedObject;
                highlightedObject.Selection();
            }

            // Обработка нажатия
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                OnSelected(selectedObject);
            }
        }
    }

    private void OnSelected(SelectionableObject so)
    {
        playerInteraction.HandleSelection(so);
    }

    private void OnMove(Vector2 input)
    {

        moveInput = input;
    }

    private void OnRotate(Vector2 input)
    {
        rotateInput = input;
    }

    private void OnZoom(float input)
    {
        zoomInput = input;
    }

    private void OnSwitchToHandCardView()
    {
        isInHandCardView = true;
        if (!isTransitioning)
        {
            isTransitioning = true;
            StartCoroutine(SmoothMoveToPoint(handCardView.position, handCardView.rotation, () =>
            {
                isTransitioning = false;
            }));
        }
    }

    private void OnRotateMode(bool isRotateMode)
    {
        this.isRotateMode = isRotateMode;
    }


    private void OnSwitchToExploration()
    {
        
        if (!isTransitioning)
        {
            isTransitioning = true;
            StartCoroutine(SmoothMoveToPoint(explorationStartPoint.position, explorationStartPoint.rotation, () =>
            {
                isTransitioning = false;
                isInHandCardView = false;
            }));
        }
    }

    private void HandleRotation()
    {
        float horizontalRotation = rotateInput.x * rotationSpeed * Time.deltaTime;
        float verticalRotation = -rotateInput.y * rotationSpeed * Time.deltaTime;

        Vector3 currentEulerAngles = cameraTransform.eulerAngles;
        currentEulerAngles.x = Mathf.Clamp(currentEulerAngles.x + verticalRotation, minVerticalAngle, maxVerticalAngle);

        cameraTransform.eulerAngles = currentEulerAngles;
        cameraTransform.Rotate(Vector3.up, horizontalRotation, Space.World);

    }

    private void HandleZoom()
    {
        cameraTransform.Translate(Vector3.forward * zoomInput * zoomSpeed * Time.deltaTime, Space.Self);
    }

    private IEnumerator SmoothMoveToPoint(Vector3 targetPosition, Quaternion targetRotation, System.Action onComplete = null)
    {
        float elapsedTime = 0f;
        Vector3 startPosition = cameraTransform.position;
        Quaternion startRotation = cameraTransform.rotation;

        while (elapsedTime < smoothTransitionTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / smoothTransitionTime;

            cameraTransform.position = Vector3.Lerp(startPosition, targetPosition, t);
            cameraTransform.rotation = Quaternion.Lerp(startRotation, targetRotation, t);

            yield return null;
        }

        cameraTransform.position = targetPosition;
        cameraTransform.rotation = targetRotation;

        onComplete?.Invoke();
    }
}
