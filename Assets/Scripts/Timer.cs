using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    public GameManager gameManager;
    public Slider timerSlider;

    public float timeLimit = 5f;
    public bool isTimerRunning; // 타이머 상태 확인
    private float timeRemaining; // 남은 시간

    void Start()
    {
        InitializeSlider(); // Slider 초기화
        // StartTimer(); // 제시 경로 지워진 후 시작할거임
    }

    // Slider 초기화
    public void InitializeSlider()
    {
        timerSlider.maxValue = timeLimit; // 최대값을 제한 시간으로 설정
        timerSlider.value = timeLimit; // 시작 값은 최대값
        isTimerRunning = false;
    }

    // 타이머 시작
    public void StartTimer()
    {
        if (!gameManager.isGameStop)
        {
            Debug.Log("StartTimer");
            timeRemaining = timeLimit;
            isTimerRunning = true;
            Debug.Log(isTimerRunning);
        }
    }

    // 매 프레임 남은 시간을 업데이트
    void Update()
    {
        if (isTimerRunning)
        {
            timeRemaining -= Time.deltaTime; // 시간 감소
            if (timeRemaining <= 0)
            {
                timeRemaining = 0; // 시간 0으로 고정
                isTimerRunning = false; // 타이머 멈춤
                TimerExpired(); // 시간 만료 처리
            }

            UpdateSliderUI(); // Slider 업데이트
        }
    }

    // Slider UI 업데이트
    private void UpdateSliderUI()
    {
        if (timerSlider != null)
        {
            timerSlider.value = timeRemaining; // 남은 시간으로 Slider 값 설정
        }
    }

    // 타이머 만료 처리
    private void TimerExpired()
    {
        if (!gameManager.isGameOver) // 이미 마지막 문제를 푼 경우는 타이머 버려
        {
            Debug.Log("시간 초과! 다음 문제로 넘어갑니다.");
            gameManager.StartNextQuestion(); // 다음 문제로
        }
    }
}
