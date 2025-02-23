using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

// 스테이지 별 소요 시간 및 시도 횟수를 얻기 위한 스테이지 데이터 클래스
[System.Serializable]
public class StageData
{
    public int attempts; // 시도 횟수
    public int errors; // 오류 입력 횟수
    public bool isCorrect; // 정답 여부
    public float takenTime; // 정답을 맞췄을 때 소요 시간

    public StageData()
    {
        attempts = 0;
        errors = 0;
        isCorrect = false;
        takenTime = 0f;
    }
}

public class DataManager : MonoBehaviour
{
    public GameManager gameMaganer;
    public Timer timer;

    public TMP_InputField nickNameInput;
    public TMP_InputField AgeInput;
    public ToggleGroup GenderInput;

    public string nickName;
    public string Age;
    public string Gender;
    public Dictionary<int, List<List<Vector2Int>>> userPaths
                  = new Dictionary<int, List<List<Vector2Int>>>();// 입력 경로 저장

    public List<StageData> stageDataList = new List<StageData>(); // 스테이지별 데이터 리스트
    public StageData currentStageData; // 현재 스테이지 데이터 객체

    public void Submit()
    {
        nickName = nickNameInput.text;
        Age = AgeInput.text;
        Toggle SellectedGender = GenderInput.ActiveToggles().FirstOrDefault();
        if (SellectedGender == null)
        {
            Debug.LogError("활성화된 Toggle이 없습니다. 성별을 선택하세요.");
            Gender = "None";
        }
        else
        {
            Gender = SellectedGender.name;
        }
    }

    // 현재 스테이지 데이터를 반환
    public StageData GetCurrentStageData(int stageIndex)
    {
        if (stageIndex >= 0 && stageIndex < stageDataList.Count)
            return stageDataList[stageIndex];
        return null;
    }

    // 새로운 스테이지 데이터를 초기화하고 리스트에 추가
    public void StartNewStage()
    {
        currentStageData = new StageData();
        stageDataList.Add(currentStageData);
    }

    public void OnCorrectAnswer()
    {
        if (currentStageData != null)
        {
            currentStageData.isCorrect = true;
            currentStageData.takenTime = timer.timerSlider.maxValue - timer.timerSlider.value;
        }
    }

    public void OnIncorrectAnswer()
    {
        if (currentStageData != null)
            currentStageData.errors++; // attempts 개수랑 같은가
    }    

    public void OnUserAttempt()
    {
        if (currentStageData != null)
            currentStageData.attempts++;
    }

    public void SaveInput(int curQuest, List<Vector2Int> userPath)
    {
        if (!userPaths.ContainsKey(curQuest))
        {
            // 새로운 문제 번호라면 초기화
            userPaths[curQuest] = new List<List<Vector2Int>>();
        }

        // 사용자의 경로를 추가
        userPaths[curQuest].Add(new List<Vector2Int>(userPath)); // 깊은 복사
    }
}
