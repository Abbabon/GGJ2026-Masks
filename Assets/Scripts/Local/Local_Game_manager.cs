using Cysharp.Threading.Tasks;
using UnityEngine;

public class Local_Game_manager : MonoBehaviour
{
    public enum GameState
    {
        Starting,
        Playing,
        GameOver
    }

    public GameState CurrentState { get; private set; } = GameState.Starting;

    public static event System.Action OnGameStart;

    [SerializeField] int sacrificeCount = 1;
    [SerializeField] float timerDuration = 10f;
    [SerializeField] float endScreenDelay = 2f;
    [SerializeField] Local_EndScreen endScreen;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CurrentState = GameState.Starting;
        AudioManager.Instance.PlayMusic("main_theme");
    }

    // Update is called once per frame
    void Update()
    {
        if (CurrentState == GameState.Starting)
        {
            if (Input.anyKeyDown)
            {
                CurrentState = GameState.Playing;
                
                var timer = FindFirstObjectByType<Local_Timer>();
                if (timer != null)
                {
                    timer.OnStateChanged += HandleTimerStateChanged;
                    timer.StartTimer(timerDuration);
                }

                OnGameStart?.Invoke();
                Debug.Log("Game Started!");
            }
        }
    }

    void HandleTimerStateChanged(Local_Timer.TimerState state)
    {
        if (state == Local_Timer.TimerState.Waiting)
        {
            KillRandomNpcs();
        }
    }

    void KillRandomNpcs()
    {
        for (int i = 0; i < sacrificeCount; i++)
        {
            var npcs = FindObjectsByType<Local_Npc>(FindObjectsSortMode.None);
            var livingNpcs = new System.Collections.Generic.List<Local_Npc>();

            foreach (var npc in npcs)
            {
                if (npc.IsAlive)
                {
                    livingNpcs.Add(npc);
                }
            }

            if (livingNpcs.Count > 0)
            {
                var victim = livingNpcs[Random.Range(0, livingNpcs.Count)];
                victim.Kill();
                // Debug.Log($"Sacrificed NPC: {victim.name}");
            }
        }
    }

    public void TotemDestoyed(Local_POI poi)
    {
        Debug.Log("A" + poi.name);
    }

    public void HereticKilled()
    {
        Debug.Log("Heretic Killed");
        //EndGame(godWins: true);
    }

    public void NoneHereticKilled()
    {
        Debug.Log("NoneHeretic Killed");
        //EndGame(godWins: false);
    }

    void EndGame(bool godWins)
    {
        if (CurrentState == GameState.GameOver) return;
        CurrentState = GameState.GameOver;
        ShowEndScreenAfterDelay(godWins).Forget();
    }

    async UniTaskVoid ShowEndScreenAfterDelay(bool godWins)
    {
        await UniTask.WaitForSeconds(endScreenDelay, cancellationToken: destroyCancellationToken);
        if (endScreen != null)
        {
            if (godWins)
                endScreen.ShowGodWin();
            else
                endScreen.ShowHereticWin();
        }
    }

    public void SetLeftPOI(int length)
    {
        Debug.Log("Left POI" + length);
    }
}
