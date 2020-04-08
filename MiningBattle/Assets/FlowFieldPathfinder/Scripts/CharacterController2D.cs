using UnityEngine;

public class CharacterController2D : MonoBehaviour
{
    #region PublicMethods
    
    public void Move(Vector3 deltaMovement)
    {
        Vector3 newHorizontalPosition = transform.position + new Vector3(deltaMovement.x, 0, 0);
        Vector3 newVerticalPosition = transform.position + new Vector3(0, deltaMovement.y, 0);

        World.BlockType verticalBlockType = World.Instance.GetBlockType(newVerticalPosition);
        World.BlockType horizontalBlockType = World.Instance.GetBlockType(newHorizontalPosition);
        if (verticalBlockType != World.BlockType.VOID && verticalBlockType != World.BlockType.DAEGUNIUM)
            deltaMovement.y = 0;
        if (horizontalBlockType != World.BlockType.VOID && horizontalBlockType != World.BlockType.DAEGUNIUM)
            deltaMovement.x = 0;
        
        transform.Translate(deltaMovement, Space.World);

    }

    #endregion
}