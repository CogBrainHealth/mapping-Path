using System.Collections;
using System.Collections.Generic;
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
    public GameObject userPathData;
    public GameObject gameOutroPanel;
    public GameObject incorrectPop;

    public TextMeshProUGUI questionStatusText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI pathText;
    public TextMeshProUGUI messageTitle;
    public TextMeshProUGUI message;
    public ScrollRect scrollRect;
    public Image scoreGage;

    public float totalQuestions = 10; // 총 문제 수

    private Coroutine pathCoroutine; // 현재 실행 중인 코루틴 저장

    public float pathDisplayTime;  // 경로를 보여줄 시간

    public int currentQuestion = 0; // 현재 문제 번호

    public float score; // 기본 제공 스코어

    public float matchingPath=0;
    public float accuracy = 0;
    //public float correctRate;
    //public float avgErrors = 0;
    //public float avgTiming = 0f;
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

    //void Update()
    //{
    //    Vector2 pos = scrollRect.normalizedPosition;
    //    pos.y = Mathf.Clamp(pos.y, -500f, 500f);  // 세로 스크롤 범위 제한
    //    scrollRect.normalizedPosition = pos;
    //}

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
                pathGenerator.startPoint.transform.localScale = new Vector3(0.2f, 0.2f, 1);
                pathGenerator.userPoint.transform.localScale = new Vector3(0.4f, 0.4f, 1);
                pathGenerator.endPoint.transform.localScale = new Vector3(0.3f, 0.3f, 1);

                Camera.main.transform.position = new Vector3(0, 0.7f, -10);
                Camera.main.orthographicSize = 3f;
                pathDisplayTime = 0.8f;
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
                pathGenerator.startPoint.transform.localScale = new Vector3(0.3f, 0.3f, 1);
                pathGenerator.userPoint.transform.localScale = new Vector3(0.5f, 0.5f, 1);
                pathGenerator.endPoint.transform.localScale = new Vector3(0.4f, 0.4f, 1);

                Camera.main.transform.position = new Vector3(1.5f, 2.4f, -10);
                Camera.main.orthographicSize = 4.2f;
                pathDisplayTime = 0.9f;
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
                pathGenerator.startPoint.transform.localScale = new Vector3(0.4f, 0.4f, 1);
                pathGenerator.userPoint.transform.localScale = new Vector3(0.6f, 0.6f, 1);
                pathGenerator.endPoint.transform.localScale = new Vector3(0.5f, 0.5f, 1);
                Camera.main.transform.position = new Vector3(0, 1.2f, -10);
                Camera.main.orthographicSize = 5.5f;
                pathDisplayTime = 1f;
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
            GameComplete(); 
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
            dataManager.SaveInput(currentQuestion, userPath); // 사용자 입력 경로 저장

            if (PathsMatch(userPath, correctPath) == 100)
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
                pathGenerator.userPoint.transform.localPosition
                    = pathGenerator.startPoint.transform.localPosition;
                dataManager.OnIncorrectAnswer(PathsMatch(userPath, correctPath));
                incorrectPop.SetActive(true);
            }
        }
    }

    private float PathsMatch(List<Vector2Int> userPath, List<Vector2Int> correctPath)
    {
        // 초기화
        accuracy = 0;
        matchingPath = 0;

        for (int i = 0; i < userPath.Count; i++)
        {
            if (userPath[i] == correctPath[i])
            {
                matchingPath++;
            }
        }

        accuracy = (matchingPath / (pathGenerator.pathLength + 1)) * 100;

        return accuracy;
    }

    public 

    void UpdateQuestionUI()
    {
        if (questionStatusText != null)
            questionStatusText.text = $"{currentQuestion} / {totalQuestions}";
    }

    public float Scoring()
    {
        //float correctCount = dataManager.stageDataList.Count(StageData => StageData.isCorrect);
        //correctRate = (correctCount / totalQuestions) * 100;
        score = 0;

        for (int i = 0; i < totalQuestions; i++)
        {
            // 정확도 점수
            if (dataManager.stageDataList[i].accuracy < 10) score += 0;
            else if (dataManager.stageDataList[i].accuracy < 30) score += 3;
            else if (dataManager.stageDataList[i].accuracy < 50) score += 5;
            else if (dataManager.stageDataList[i].accuracy < 70) score += 7;
            else if (dataManager.stageDataList[i].accuracy < 90) score += 9;
            else score += 10;

            if (dataManager.stageDataList[i].isCorrect)
            {
                // 복잡도 보너스 점수
                if (i < 3) score += 1;
                else if (i < 6) score += 3;
                else score += 5;

                // 응답 시간 점수
                Debug.Log(i + "번 걸린 시간: " + dataManager.stageDataList[i].takenTime);
                if (dataManager.stageDataList[i].takenTime > 3.69) score += 0;
                else if (dataManager.stageDataList[i].takenTime > 3.01) score += 0.5f;
                else if (dataManager.stageDataList[i].takenTime > 2.51) score += 1;
                else if (dataManager.stageDataList[i].takenTime > 2.01) score += 1.5f;
                else if (dataManager.stageDataList[i].takenTime > 1.5) score += 2;
            }
            Debug.Log(i + "번: " + score);
        }

        //avgTiming /= totalQuestions; // 걸린 시간 평균

        return score;
    }
    
    public void GameComplete()
    {
        Debug.Log("게임 종료");

        //오브젝트 비활성화 
        Map2.SetActive(false);
        Map3.SetActive(false);
        Map4.SetActive(false);
        Map6.SetActive(false);
        pathGenerator.HidePoint();
        gameOutroPanel.SetActive(true);
        //userPathData.SetActive(true);

        //scoreText.text = $"{Scoring():F1}";
        //Scoring();

        //string scoreTextContent = $"닉네임: {dataManager.nickName}\n" +
        //                          $"나이: {dataManager.Age}" +
        //                          $" / 성별: {dataManager.Gender}\n\n" +
        //                          $"정답률: {correctRate}\n" +
        //                          $"avgTime: {avgTiming}\n\n";
        //string pathTextContent = "";

        //for (int i = 0; i < totalQuestions; i++)
        //{
        //    // scoreTextContent += $"Q{i+1}: {dataManager.stageDataList[i].attempts} | " +
        //    scoreTextContent += $"Q{i+1} 걸린 시간: {dataManager.stageDataList[i].takenTime}\n";
        //}

        //foreach (var kvp in dataManager.userPaths)
        //{
        //    Debug.Log($"문제 {kvp.Key}:");
        //    pathTextContent += $"문제{kvp.Key}\n";
        //    foreach (var path in kvp.Value)
        //    {
        //        Debug.Log("경로: " + string.Join(" -> ", path));
        //        pathTextContent += $"경로: {string.Join("-> ", path)}\n";
        //    }
        //}

        //pathText.text = pathTextContent;

        Scoring();
        Debug.Log(score);

        if (score > 25.27)
        {
            messageTitle.text += "20대 (25.27점 ~ 26점)";

            message.text += "길찾기 게임을 통해 기억력을 검사했어요!\n\n" +
                             "최고 수준의 기억력!\n" +
                             dataManager.nickName + "님은 20대 수준의 기억력을 가지고 있어요!\n" +
                             "한 번 본 경로를 거의 완벽하게 기억하고 재현할 수 있는 능력을 가지고 있네요.\n\n" +
                             "이 수준을 유지하려면?\n" +
                             "- 경로를 기억할 때 시각적 이미지(나무, 건물, 표지판 등)를 떠올려 보세요.\n" +
                             "- 이동한 경로를 나중에 종이에 그려보며 복습하는 것도 좋은 방법입니다!";
        }

        else if (score > 24.09)
        {
            messageTitle.text += "30대 (24.09점 ~ 25.27점)";

            message.text += "길찾기 게임을 통해 기억력을 검사했어요!\n\n" +
                             "우수한 기억력!\n" +
                             dataManager.nickName + "님은 30대 수준의 기억력을 가지고 있어요!\n" +
                             "경로를 빠르게 파악하고 기억하는 능력이 뛰어나며, 실수 없이 재현할 가능성이 높아요.\n\n" +
                             "이 능력을 더 키우려면?\n" +
                             "- 이동 경로를 머릿속으로 떠올리며 복습하는 습관을 들여보세요.\n" +
                             "- 도착 후 '어떤 길을 어떻게 왔는지' 떠올려보며 점검하는 연습이 효과적입니다.";
        }

        else if (score > 19.41)
        {
            messageTitle.text += "40대 (19.41점 ~ 24.09점)";

            message.text += "길찾기 게임을 통해 기억력을 검사했어요!\n\n" +
                             "균형 잡힌 기억력!\n" +
                             dataManager.nickName + "님은 40대 수준의 기억력을 가지고 있어요!\n" +
                             "전체 경로를 한 번에 기억하기보다는, 주요 지점(랜드마크 중심)으로 기억하는 경향이 있어요.\n\n" +
                             "좀 더 향상시키려면?\n" +
                             "- 이동하면서 '내가 지난 장소에서 어떤 특징이 있었는지' 떠올리는 습관을 길러보세요.\n" +
                             "- 길을 걸은 후, 일정 시간이 지난 뒤 머릿속으로 경로를 다시 떠올려보는 연습을 추천합니다.";
        }

        else if (score > 13.16)
        {
            messageTitle.text += "50대 (13.16점 ~ 19.41점)";

            message.text += "길찾기 게임을 통해 기억력을 검사했어요!\n\n" +
                             "훈련하면 더 좋아질 수 있어요!\n" +
                             dataManager.nickName + "님은 50대 수준의 기억력을 가지고 있어요!\n" +
                             "길을 찾을 때 여러 번 생각해야 하거나, 중간에 일부 경로를 잊을 가능성이 있어요.\n\n" +
                             "더 나은 기억력을 원한다면?\n" +
                             "- 이동할 때 경로를 하나의 이야기(스토리)로 만들어보세요!\n" +
                             "  예: '큰 나무를 지나면 왼쪽에 있는 빨간 건물을 보고, 그다음 횡단보도를 건넌다.'\n" +
                             "  이처럼 스토리로 연결하면 기억이 더 오래 유지됩니다!";

        }

        else if (score > 8.84)
        {
            messageTitle.text += "60대 (8.84점 ~ 13.16점)";

            message.text += "길찾기 게임을 통해 기억력을 검사했어요!\n\n" +
                             "조금 더 자주 훈련하면 좋아질 수 있어요!\n" +
                             dataManager.nickName + "님은 60대 수준의 기억력을 가지고 있어요!\n" +
                             "경로를 기억하는 데 시간이 걸릴 수 있으며, 일부 정보가 빠르게 사라질 수 있어요.\n\n" +
                             "훈련을 위해?\n" +
                             "- 작은 구간별로 이동 후, 10초 동안 멈춰서 '내가 방금 어디를 지나왔는지; 떠올려보세요.\n" +
                             "- 종이에 경로를 직접 그려보는 것도 기억력 강화에 효과적입니다!";

        }

        else
        {
            messageTitle.text += " 70대 (4점 ~ 8.84점)";

            message.text += "길찾기 게임을 통해 기억력을 검사했어요!\n\n" +
                             "조금 더 연습하면 기억력이 향상될 거예요!\n" +
                             dataManager.nickName + "님은 70대 수준의 기억력을 가지고 있어요!\n" +
                             "경로를 기억하는 것이 다소 어려울 수 있지만, 반복 학습을 통해 충분히 좋아질 수 있어요.\n\n" +
                             "훈련을 위해?\n" +
                             "- 짧고 간단한 경로부터 시작해 기억하는 연습을 해보세요.\n" +
                             "- 예를 들어, 집에서 편의점까지의 길을 기억한 뒤, 거리를 점점 늘려가는 방식이 효과적입니다!";
        }

        score /= 152;

        scoreText.text += score * 100;
        scoreGage.fillAmount = score;
        Debug.Log(score);

        isGameOver = true; // 마지막 문제에서 시간초과 시 또 호출 되는 것을 방지
        pathGenerator.gameObject.SetActive(false);
        touchControl.gameObject.SetActive(false); // 얘네 말고 다른 애들도 비활성화해놓기
    }

    public void Exit()
    {
        Application.Quit();
    }
}
