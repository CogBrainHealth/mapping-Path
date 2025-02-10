using UnityEngine;

public class CircleHandler : MonoBehaviour
{
    public Vector2Int coordinates; // Circle의 그리드 좌표
    public TouchControl touchControl;

    private void Start()
    {
        // Circle들의 좌표 초기화
        coordinates = new Vector2Int(Mathf.FloorToInt(transform.position.x),
                                     Mathf.FloorToInt(transform.position.y));
    }

    public Vector2Int GetCoordinates()
    {
        return coordinates;
    }
}