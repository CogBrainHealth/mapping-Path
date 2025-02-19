using System.Collections;
using System.Collections.Generic;
using System.Linq; // Count 함수 사용을 위해 필요
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public enum Difficulty { Easy, Okay, Medium, Hard } // 난이도 Enum
    public Difficulty currentDifficulty;         // 현재 난이도

    public pathGenerator pathGenerator; // 난이도에 따른 초기화 및 정답 경로를 지우기 위한 참조
    public TouchControl touchControl; // 사용자 입력 경로를 지우기 위한 참조
    public Timer timer; // 타이머 조절
    public DataManager dataManager; // 데이터 관리

    public GameObject Map2;
    public GameObject Map3;
    public GameObject Map4;
    public GameObject Map6;

    public Animator gameIntroPanel;
    public GameObject gameOutroPanel;
    public GameObject incorrectPop;

    public TextMeshProUGUI questionStatusText;
    public TextMeshProUGUI scoreText;

    public float totalQuestions = 10; // 총 문제 수

    private Coroutine pathCoroutine; // 현재 실행 중인 코루틴 저장

    public float pathDisplayTime;  // 경로를 보여줄 시간

    public int currentQuestion = 0; // 현재 문제 번호

    public float score; // 기본 제공 스코어

    public float accuracy;
    public float avgErrors = 0;
    public float avgTiming = 0f;
    public float Complexity;


    // public float weightAccuracy = 0.9f; // 오답률 가중치
    //public float weightErrors = 0.5f; // 오류 입력 가중치?
    //public float weightTime = 0.3f; // 걸린 시간 가중치

    public bool isGameOver = false;

    private List<Vector2Int> correctPath; // 정답 경로 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameIntroPanel.SetBool("isShow", true);
    }

    public void Skip()
    {
        gameIntroPanel.SetBool("isShow", false);
    }

    public void StartGame()
    {
        ApplyDifficultySettings(); // 난이도 적용
        UpdateQuestionUI();
        StartNextQuestion();
    }

    void ApplyDifficultySettings()
    {
        // 난이도에 따라 설정
        switch (currentDifficulty)
        {
            case Difficulty.Easy: // 2x2 맵
                Map2.SetActive(true);
                Map3.SetActive(false);
                Map4.SetActive(false);
                Map6.SetActive(false);
                pathGenerator.isMap3 = false;
                pathGenerator.mapSize = 2;
                pathGenerator.pathLength = 4;
                pathGenerator.endPoint.transform.localScale = new Vector3(0.2f, 0.2f, 1);
                Camera.main.transform.position = new Vector3(0, 0.5f, -10);
                Camera.main.orthographicSize = 2.6f;
                pathDisplayTime = 1f;
                Complexity = 1;
                break;

            case Difficulty.Okay: // 3x3 맵
                Map2.SetActive(false);
                Map3.SetActive(true);
                Map4.SetActive(false);
                Map6.SetActive(false);
                pathGenerator.isMap3 = true;
                pathGenerator.mapSize = 3; // 다시 생각 
                pathGenerator.pathLength = 5;
                pathGenerator.endPoint.transform.localScale = new Vector3(0.3f, 0.3f, 1);
                Camera.main.transform.position = new Vector3(1.5f, 2.2f, -10);
                Camera.main.orthographicSize = 3.8f;
                pathDisplayTime = 1f;
                Complexity = 2;
                break;

            case Difficulty.Medium: // 4x4 맵
                Map2.SetActive(false);
                Map3.SetActive(false);
                Map4.SetActive(true);
                Map6.SetActive(false);
                pathGenerator.isMap3 = false;
                pathGenerator.mapSize = 4;
                pathGenerator.pathLength = 7;
                pathGenerator.endPoint.transform.localScale = new Vector3(0.5f, 0.5f, 1);
                Camera.main.transform.position = new Vector3(0, 0.7f, -10);
                Camera.main.orthographicSize = 5f;
                pathDisplayTime = 2f;
                Complexity = 3;
                break;

            case Difficulty.Hard: // 6x6 맵
                Map2.SetActive(false);
                Map3.SetActive(false);
                Map4.SetActive(false);
                Map6.SetActive(true);
                pathGenerator.isMap3 = false;
                pathGenerator.mapSize = 6;
                pathGenerator.pathLength = 10;
                Camera.main.transform.position = new Vector3(0, 0.5f, -10);
                Camera.main.orthographicSize = 6.2f;
                pathDisplayTime = 2.5f;
                break;
        }
    }

    public void StartNextQuestion()
    {
        //Debug.Log(currentQuestion);
        if (currentQuestion < totalQuestions)
        {
            currentQuestion++;

            if (currentQuestion > 3 && currentQuestion <= 6)
            {
                currentDifficulty = Difficulty.Okay;
            }

            else if (currentQuestion > 6)
            {
                currentDifficulty = Difficulty.Medium;
            }

            ApplyDifficultySettings();

            dataManager.StartNewStage(); // 현재 스테이지 데이터 초기화
            // timer.StartTimer(); // 제시 경로 지워진 후 ㅇㅇ
            timer.InitializeSlider();
            UpdateQuestionUI();
            correctPath = pathGenerator.GeneratePath(); // 정답 경로 생성 

            // 이전 코루틴이 실행 중이면 중단
            // 경로 아주 짧게 제시되는 문제 해결
            if (pathCoroutine != null)
            {
                StopCoroutine(pathCoroutine);
            }

            // 새 코루틴 시작
            pathCoroutine = StartCoroutine(ShowPathForDuration(pathDisplayTime));
        }
        else
        {
            Debug.Log("모든 문제를 완료했습니다.");
            GameExit(); 
        }
    }

    IEnumerator ShowPathForDuration(float duration)
    {
        // 이전에 그린 답 지우기 
        touchControl.lineRenderer.positionCount = 0;

        // 일정 시간 동안 경로를 보여준다.
        yield return new WaitForSeconds(duration);

        // 사용자가 아직 그리지 않았고(사용자가 그렸으면 0일 것), 경로 표시 시간이 끝나면 경로를 지운다.
        if (pathGenerator.lineRenderer.positionCount > 0)
            pathGenerator.HidePath();
    }

    // 정답을 체크하고 결과를 출력
    public void CheckPath(List<Vector2Int> userPath)
    {
        if (!isGameOver)
        {
            dataManager.OnUserAttempt(); // 시도 횟수 증가

            if (PathsMatch(userPath, correctPath))
            {
                Debug.Log("정답!");
                dataManager.OnCorrectAnswer();
                StartNextQuestion(); // 다음 문제로 진행
            }
            else
            {
                Debug.Log("오답!");
                // 재시도 로직 또는 다른 처리
                touchControl.lineRenderer.positionCount = 0;
                dataManager.OnIncorrectAnswer();
                incorrectPop.SetActive(true);
            }
        }
    }

    private bool PathsMatch(List<Vector2Int> userPath, List<Vector2Int> correctPath)
    {
        if (userPath.Count != correctPath.Count) return false;

        for (int i = 0; i < userPath.Count; i++)
        {
            if (userPath[i] != correctPath[i])
                return false;
        }

        return true;
    }

    void UpdateQuestionUI()
    {
        if (questionStatusText != null)
            questionStatusText.text = $"{currentQuestion} / {totalQuestions}";
    }

    public float Scoring()
    {
        float correctCount = dataManager.stageDataList.Count(StageData => StageData.isCorrect);
        accuracy = (correctCount / totalQuestions) * 100;

        for (int i = 0; i < totalQuestions; i++)
        {
            //avgErrors += dataManager.stageDataList[i].errors;
            avgTiming += dataManager.stageDataList[i].takenTime;
        }
        //avgErrors /= totalQuestions;
        avgTiming /= totalQuestions; // 걸린 시간 평균 

        // 걸린 시간 평가
        //if (avgTiming <= 2) avgTiming = 0;
        //else if (avgTiming <= 3) avgTiming = 10;
        //else avgTiming = 20;

        Debug.Log($"{correctCount}");
        Debug.Log($"{accuracy} / {avgTiming}");
        // score = accuracy - avgErrors * 10 - avgTiming; // 추후 수정 
        return score;
    }
    
    public void GameExit()
    {
        Debug.Log("게임 종료");

        //오브젝트 비활성화 
        Map2.SetActive(false);
        Map3.SetActive(false);
        Map4.SetActive(false);
        Map6.SetActive(false);
        pathGenerator.HidePoint();

        gameOutroPanel.SetActive(true);
        //scoreText.text = $"{Scoring():F1}";
        Scoring();
        string scoreTextContent = $"닉네임: {dataManager.nickName}\n" +
                                  $"나이: {dataManager.Age}" +
                                  $" / 성별: {dataManager.Gender}\n\n" +
                                  $"정답률: {accuracy}\n" +
                                  $"avgTime: {avgTiming}\n\n";
        for (int i = 0; i < totalQuestions; i++)
        {
            // scoreTextContent += $"Q{i+1}: {dataManager.stageDataList[i].attempts} | " +
               scoreTextContent += $"Q{i + 1} 걸린 시간: {dataManager.stageDataList[i].takenTime}\n";
        }

        scoreText.text = scoreTextContent;

        isGameOver = true; // 마지막 문제에서 시간초과 시 또 호출 되는 것을 방지
        pathGenerator.gameObject.SetActive(false);
        touchControl.gameObject.SetActive(false); // 얘네 말고 다른 애들도 비활성화해놓기

    }

    public void TestExit()
    {

    }
}
