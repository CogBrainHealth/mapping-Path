using System.Collections.Generic;
using UnityEngine;

public class pathGenerator : MonoBehaviour
{
    public GameManager gameManager;
    public Timer timer;

    public int pathLength; // 경로의 길이
    public int mapSize; // 지도의 크기
    public bool isMap3; // 3X3인 경우 경로 변;

    List<Vector2Int> path = new List<Vector2Int>(); // 경로 리스트

    Vector2Int currentPosition; // 현재 위치

    // 경로 그릴 애들
    public LineRenderer lineRenderer;
    GameObject startPoint;
    GameObject endPoint;

    void Awake()
    {
        // 오브젝트 찾기
        startPoint = GameObject.Find("StartPoint");
        endPoint = GameObject.Find("EndPoint");
        lineRenderer = GetComponent<LineRenderer>();
    }

    public List<Vector2Int> GeneratePath()
    {
        //Debug.Log("호출");
        if (gameManager.currentQuestion > 0)
        {

        }
        while (true) // 유효한 경로 생성 못했을 경우 다시 검색하기 위해 
        {
            // 경로 초기화
            path.Clear();

            // 시작 위치 초기화
            InitStartPosition();
            path.Add(currentPosition);
            // Debug.Log($"시작 위치: {currentPosition}");

            // 지도 초기화
            //Debug.Log("지도 초기화 시작");
            startPoint.transform.localPosition = new Vector2(currentPosition.x, currentPosition.y);
            //Debug.Log("메롱띠");
            lineRenderer.positionCount = 1; // 시작점 포함
            lineRenderer.SetPosition(0, new Vector2(currentPosition.x, currentPosition.y));

            bool isValidPath = true;

            for (int i = 0; i < pathLength; i++)
            {
                // 이동 가능한 방향을 찾고, 그 방향을 nextMove로 반환
                Vector2Int nextMove;

                if (!TryGetValidMove(out nextMove)) // 유효한 경로가 없을 경우 -> false 반환
                {
                    Debug.LogWarning("유효한 경로를 찾지 못했습니다. 경로 생성을 다시 시작합니다.");
                    isValidPath = false;
                    break;

                }

                // 유효한 경로가 있을 경우 -> true -> possibleMove 중 랜덤으로 nextMove에 할당 됨
                currentPosition += nextMove;
                path.Add(currentPosition);

                // 화면 경로에 새 점 추가
                lineRenderer.positionCount++;
                lineRenderer.SetPosition(lineRenderer.positionCount - 1, new Vector2(currentPosition.x, currentPosition.y));
            }

            if (isValidPath)
            {
                endPoint.transform.localPosition = new Vector2(currentPosition.x, currentPosition.y);
                Debug.Log("정답 경로: " + string.Join(" -> ", path));
                return path;
            }
        }
    }

    public void HidePath()
    {
        // 경로를 지우고, StartPoint와 EndPoint만 남긴다.
        lineRenderer.positionCount = 0;
        timer.StartTimer();
    }

    void InitStartPosition()
    {
        int x;
        int y;

        if (isMap3)
        {
            // 모든 좌표에서 랜덤으로 시작 위치 설정 (범위 제한)
            x = Mathf.Clamp(Random.Range(0, mapSize), 0, mapSize);
            y = Mathf.Clamp(Random.Range(0, mapSize), 0, mapSize);
            currentPosition = new Vector2Int(x, y);
        }
        else
        {
            // 모든 좌표에서 랜덤으로 시작 위치 설정 (범위 제한)
            x = Mathf.Clamp(Random.Range(-mapSize/2, mapSize/2), -mapSize/2, mapSize/2);
            y = Mathf.Clamp(Random.Range(-mapSize/2, mapSize/2), -mapSize/2, mapSize/2);
            currentPosition = new Vector2Int(x, y);
        }
    }

   bool TryGetValidMove(out Vector2Int move)
    {
        List<Vector2Int> possibleMoves = new List<Vector2Int>
        {
            new Vector2Int(0, 1),  // 위
            new Vector2Int(0, -1), // 아래
            new Vector2Int(-1, 0), // 왼쪽
            new Vector2Int(1, 0)   // 오른쪽
        };

        // 유효하지 않은 움직임 필터링
        possibleMoves.RemoveAll(move =>
        {
            Vector2Int nextPosition = currentPosition + move;

            // 이미 방문했거나 테두리를 벗어난 경우
            if (isMap3)
            {
                return path.Contains(nextPosition) ||
                       nextPosition.x < 0 || nextPosition.x > mapSize ||
                       nextPosition.y < 0 || nextPosition.y > mapSize;
            }
            else
            {
                return path.Contains(nextPosition) ||
                       nextPosition.x < -mapSize/2 || nextPosition.x > mapSize/2 ||
                       nextPosition.y < -mapSize/2 || nextPosition.y > mapSize/2;
            }
        });

        if (possibleMoves.Count > 0)
        {
            move = possibleMoves[Random.Range(0, possibleMoves.Count)];
            return true;
        }
        else
        {
            move = Vector2Int.zero;
            return false;
        }
    }

    // GameExit에서 실행
    public void HidePoint()
    {
        startPoint.SetActive(false);
        endPoint.SetActive(false);
    }
}