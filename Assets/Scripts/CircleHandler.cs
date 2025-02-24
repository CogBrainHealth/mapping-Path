using UnityEngine;

public class CircleHandler : MonoBehaviour
{
    public Vector2Int coordinates; // Circle의 그리드 좌표
    public TouchControl touchControl;
    public Transform detectCircle;

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

    public void Smaller()
    {
        foreach (Transform circle in detectCircle)
        {
            circle.transform.localScale = new Vector3(0.5f, 0.5f, 1);
        }
    }

    public void Bigger(int x, int y)
    {
        Vector3 targetPosition = new Vector3(x, y, 0);
        
        foreach (Transform circle in detectCircle)
        {
            if (circle.transform.position == targetPosition)
            {
                circle.transform.localScale = new Vector3(0.8f, 0.8f, 1);
            }
        }

    }
}