using UnityEngine;

public class CameraControl : MonoBehaviour
{
    #region PrivateVariables

    private Vector3 _cornerMinPosition;
    private Vector3 _cornerMaxPosition;
    
    private Vector3 _playerBasePosition;

    private Camera _camera;
    
    #endregion

    #region PublicVariables

    public float cameraSpeed = 10.0f;
    public float cameraBorderThickness = 10f;

    public float scrollSpeed = 200f;

    public float minCameraZ = -30f;
    public float maxCameraZ = -2f;

    public Transform cornerMax;
    public Transform cornerMin;

    public Transform playerBase;
    
    #endregion

    #region PrivateMethods

    private void Awake()
    {
        _cornerMaxPosition = cornerMax.position;
        _cornerMinPosition = cornerMin.position;
        
        _playerBasePosition = playerBase.position;
        
        _camera = Camera.main;
    }

    private Vector3 FromViewPortToWorldPoint(Vector2 ratio)
    {
        Ray ray = _camera.ViewportPointToRay(ratio);
        return ray.GetPoint(-ray.origin.z / ray.direction.z);
    }

    
    private void LateUpdate()
    {
        Vector2 worldDownLeft = FromViewPortToWorldPoint(new Vector2(0, 0));
        Vector2 worldUpRight = FromViewPortToWorldPoint(new Vector2(1, 1));
        if (PauseMenu.GameIsPaused)
            return;

        var position = transform.position;
        var moveDistance = cameraSpeed * Time.deltaTime;
        
        if ((Input.GetKey(PlayerInputsManager.Instance.CameraForwardKey) || Input.mousePosition.y >= Screen.height - cameraBorderThickness) &&
            worldUpRight.y + moveDistance < _cornerMaxPosition.y)
            position.y += moveDistance;
        if ((Input.GetKey(PlayerInputsManager.Instance.CameraBackwardKey) || Input.mousePosition.y <= cameraBorderThickness) &&
            worldDownLeft.y - moveDistance > _cornerMinPosition.y)
            position.y -= moveDistance;
        if ((Input.GetKey(PlayerInputsManager.Instance.CameraRightKey) || Input.mousePosition.x >= Screen.width - cameraBorderThickness) &&
            worldUpRight.x + moveDistance < _cornerMaxPosition.x)
            position.x += moveDistance;
        if ((Input.GetKey(PlayerInputsManager.Instance.CameraLeftKey) || Input.mousePosition.x <= cameraBorderThickness) &&
            worldDownLeft.x - moveDistance > _cornerMinPosition.x)
            position.x -= moveDistance;
        if (Input.GetKey(PlayerInputsManager.Instance.CameraToBaseKey))
        {
            position.x = _playerBasePosition.x;
            position.y = _playerBasePosition.y;
        }

        var scroll = Input.GetAxis("Mouse ScrollWheel");

        position.z += scroll * scrollSpeed * Time.deltaTime;
        position.z = Mathf.Clamp(position.z, minCameraZ, maxCameraZ);

        Vector2 minOverFlow = (Vector2) _cornerMinPosition - worldDownLeft;
        if (minOverFlow.x > 0)
            position.x += minOverFlow.x;
        if (minOverFlow.y > 0)
            position.y += minOverFlow.y;

        Vector2 maxOverFlow = worldUpRight - (Vector2) _cornerMaxPosition;
        if (maxOverFlow.x > 0)
            position.x -= maxOverFlow.x;
        if (maxOverFlow.y > 0)
            position.y -= maxOverFlow.y;

        transform.position = position;
    }

    #endregion

    #region PublicMethods

    public void SetPlayerBase(GameObject _playerBase)
    {
        playerBase = _playerBase.transform;
        _playerBasePosition = playerBase.position;
    }

    #endregion
}