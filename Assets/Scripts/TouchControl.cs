using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TouchControl : MonoBehaviour
{
    public LineRenderer lineRenderer; // 사용자가 그린 정보 시각적 표현 
    public GameManager gameManager; // 사용자 입력 정보 전달 용도 참조 
    public pathGenerator pathGenerator; // 사용자가 입력 시작 시에 정답 경로 지우는 용도로 참조
    
    public bool isInputActive = false; // 입력 중인지 여부 
    public float detectionRadius = 0.5f; // Circle을 감지할 거리

    public List<Vector2Int> userPath = new List<Vector2Int>();
    public List<CircleHandler> circlesList = new List<CircleHandler>();

    void Start()
    {
        // 게임 시작 시 CircleHandler 객체들을 찾고 리스트에 저장
        circlesList.AddRange(FindObjectsOfType<CircleHandler>());
    }

    void Update()
    {
        if (pathGenerator.lineRenderer.positionCount == 0)
        {
            // 터치 입력 처리
            if (Input.touchCount > 0)
            {
                HandleTouchInput();
            }

            // 마우스 입력 처리 
            else if (Input.GetMouseButton(0))
            {
                HandleMouseInput();
            }

            // 입력 종료 감지
            if (Input.GetMouseButtonUp(0) || (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Ended))
            {
                if (isInputActive) // 입력 활성 상태일 때만 검사
                {
                    isInputActive = false; // 입력 비활성화
                    CheckUserPath(); // 경로 검사
                }
            }
        }
    }

    // 터치 입력 처리 
    private void HandleTouchInput()
    {
        Touch touch = Input.touches[0]; // 첫 번째 터치 정보 가져오기 
        Vector2 touchPos = Camera.main.ScreenToWorldPoint(touch.position); // 터치 위치를 화면좌표 -> 월드 좌표로 변환 

        if (touch.phase == TouchPhase.Began) // 터치가 시작된 순간 
        {
            gameManager.incorrectPop.SetActive(false);
            // pathGenerator.HidePath(); 아예 입력 못하게 
            StartInput(); // 입력 시작 처리
        }
        else if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary) // 터치가 이동하거나 화면에 손가락을 유지할 경우 
        {
            ContinueInput(touchPos); // 입력 경로 업데이트 
        }
    }

    // 마우스 입력 처리 
    private void HandleMouseInput()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition); // 마우스 위치를 화면좌표 -> 월드 좌표로 변환 

        if (Input.GetMouseButtonDown(0)) // 마우스 버튼을 누르는 순간 
        {
            gameManager.incorrectPop.SetActive(false);
            // pathGenerator.HidePath();
            StartInput(); // 입력 시작 처리 
        }
        else if (Input.GetMouseButton(0)) // 마우스 드래그 중인 경우 
        {
            ContinueInput(mousePos);// 입력 경로 업데이트
        }
    }

    // 입력 시작 처리 
    private void StartInput()
    {
        isInputActive = true;
        userPath.Clear();
        lineRenderer.positionCount = 0;
    }

    // 입력 경로 업데이트 
    private void ContinueInput(Vector2 position)
    {
        // 입력 활성화 상태인 경우만 처리
        if (isInputActive)
        {
            lineRenderer.positionCount++; // 점 개수 증가
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, position); // 새로운 점 위치 설정
            pathGenerator.userPoint.transform.localPosition = position;

            // userPath 업데이트
            foreach (CircleHandler circle in circlesList)
            {
                float distance = Vector2.Distance(position, circle.transform.position);
                if (distance <= detectionRadius)
                {
                    Vector2Int gridPosition = circle.GetCoordinates();
                    if (!userPath.Contains(gridPosition)) userPath.Add(gridPosition); // 중복 경로 방지 
                }
            }
        }
    }

    // 사용자 입력 경로 확인 
    private void CheckUserPath()
    {
        Debug.Log("입력 경로: " + string.Join(" -> ", userPath));
        gameManager.CheckPath(userPath);

        // 정답/오답 여부에 따라 추가 처리
    }
}