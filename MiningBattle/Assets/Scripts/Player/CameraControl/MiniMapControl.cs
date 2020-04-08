using UnityEngine;

public class MiniMapControl : MonoBehaviour
{
    public RectTransform SelectSquareImage;
    public RectTransform Panel;

    private Camera _mainCamera;

    private Camera _miniMapCamera;
    
    void Awake()
    {
        _mainCamera = Camera.main;
        _miniMapCamera = GetComponent<Camera>();
    }

    // Start is called before the first frame update
    void Start()
    {
        DrawCameraBorder();
    }

    // Update is called once per frame
    void Update()
    {
        if (!PauseMenu.GameIsPaused)
        {
            ClickOnMiniMap();
            DrawCameraBorder();
        }
    }

    private void ClickOnMiniMap()
    {
        Vector3 offset = Panel.position;
        float scale = Panel.localScale.x;
        Vector3 clickPosition = (Input.mousePosition - offset) / scale;
        
        if (Input.GetMouseButton(0) && _miniMapCamera.pixelRect.Contains(clickPosition))
        {
            Ray ray = _miniMapCamera.ScreenPointToRay(clickPosition );
            Vector3 worldPoint = ray.GetPoint(-ray.origin.z / ray.direction.z);
            _mainCamera.transform.position = new Vector3(worldPoint.x, worldPoint.y, _mainCamera.transform.position.z);
        }
    }
    
    private Vector3 FromViewPortToWorldPoint(Vector2 ratio)
    {
        Ray ray = _mainCamera.ViewportPointToRay(ratio);
        return ray.GetPoint(-ray.origin.z / ray.direction.z);
    }

    private void DrawCameraBorder()
    {
        Vector2 offset = Panel.position;
        Vector2 scale = Panel.localScale;
        
        Vector2 worldDownLeft = FromViewPortToWorldPoint(new Vector2(0, 0));
        Vector2 worldUpRight = FromViewPortToWorldPoint(new Vector2(1, 1));
        Vector2 worldCenter = (worldDownLeft + worldUpRight) / 2;
        
        Vector2 screenDownLeft = _miniMapCamera.WorldToScreenPoint(worldDownLeft);
        Vector2 screenUpRight = _miniMapCamera.WorldToScreenPoint(worldUpRight);
        Vector2 centre =  _miniMapCamera.WorldToScreenPoint(worldCenter) * scale + offset;
        
        float sizeX = Mathf.Abs(screenDownLeft.x - screenUpRight.x);
        float sizeY = Mathf.Abs(screenDownLeft.y - screenUpRight.y);

        SelectSquareImage.position = centre;
        SelectSquareImage.sizeDelta = new Vector2(sizeX, sizeY);
    }
}
