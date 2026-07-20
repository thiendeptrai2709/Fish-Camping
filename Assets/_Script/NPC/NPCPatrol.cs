using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

[RequireComponent(typeof(NavMeshAgent))]
public class NPCPatrol : MonoBehaviour
{
    [Header("Patrol Settings")]
    [Tooltip("Danh sách các điểm Point tuần tra. Để trống nếu muốn NPC đứng im.")]
    [SerializeField] private List<Transform> patrolPoints = new List<Transform>();
    [SerializeField] private float movementSpeed = 2f;
    [SerializeField] private float stopDuration = 2f; // Thời gian đứng nghỉ tại mỗi điểm

    private NavMeshAgent _agent;
    private NPCBase _npcBase;
    private int _currentPointIndex = 0;
    private float _waitTimer = 0f;
    private bool _isWaiting = false;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _npcBase = GetComponent<NPCBase>();
    }

    private void Start()
    {
        // Cấu hình thông số NavMeshAgent bằng code cho đồng bộ
        _agent.speed = movementSpeed;

        // Nếu KHÔNG có điểm tuần tra nào, tắt Agent đi để tối ưu và cho NPC đứng im
        if (patrolPoints == null || patrolPoints.Count == 0)
        {
            _agent.enabled = false;
            if (_npcBase != null) _npcBase.SetWalkingState(false);
            return;
        }

        // Nếu có điểm, xuất phát tới điểm đầu tiên
        MoveToNextPoint();
    }

    private void Update()
    {
        // Nếu không có điểm tuần tra hoặc Agent bị tắt (đang nói chuyện), dừng Update
        if (patrolPoints.Count == 0 || !_agent.enabled || !_agent.isOnNavMesh) return;

        // Kiểm tra xem NPC đã đi đến đích chưa
        if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
        {
            if (!_isWaiting)
            {
                _isWaiting = true;
                _waitTimer = 0f;
                // Đến điểm dừng thì chuyển animation về Idle (NPCState = 0)
                if (_npcBase != null) _npcBase.SetWalkingState(false);
            }

            // Đếm thời gian đứng chờ nghỉ mệt
            _waitTimer += Time.deltaTime;
            if (_waitTimer >= stopDuration)
            {
                _isWaiting = false;
                MoveToNextPoint();
            }
        }
    }

    private void MoveToNextPoint()
    {
        if (patrolPoints.Count == 0) return;

        // Ra lệnh cho NavMesh di chuyển tới điểm tiếp theo
        _agent.destination = patrolPoints[_currentPointIndex].position;

        // Bật animation đi bộ/chạy (NPCState = 1)
        if (_npcBase != null) _npcBase.SetWalkingState(true);

        // Chuyển chỉ số sang điểm kế tiếp (vòng lặp)
        _currentPointIndex = (_currentPointIndex + 1) % patrolPoints.Count;
    }

    // GỌI TỪ NPCBASE: Khi Player bấm nói chuyện, đóng băng Agent lại
    public void PausePatrol()
    {
        if (patrolPoints.Count == 0) return;
        _agent.isStopped = true;
        _agent.enabled = false;
    }

    // GỌI TỪ NPCBASE: Khi nói chuyện xong, tiếp tục đi tuần
    public void ResumePatrol()
    {
        if (patrolPoints.Count == 0) return;
        _agent.enabled = true;

        // Đợi 1 khung hình để Agent kích hoạt lại rồi mới ra lệnh di chuyển tiếp
        Invoke(nameof(RestartAgentDestination), 0.1f);
    }

    private void RestartAgentDestination()
    {
        if (!_agent.enabled || patrolPoints.Count == 0) return;
        _agent.isStopped = false;

        // Quay trở lại điểm cũ đang đi dở
        int lastIndex = _currentPointIndex - 1;
        if (lastIndex < 0) lastIndex = patrolPoints.Count - 1;

        _agent.destination = patrolPoints[lastIndex].position;
        if (_npcBase != null) _npcBase.SetWalkingState(true);
    }
}