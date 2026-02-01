using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Renderer))]
public class StalkerAI : MonoBehaviour
{
    [Header("Configurações do Jogador")]
    public Camera playerCamera;
    public Transform playerTransform;

    [Header("Definições de Comportamento")]
    public float moveSpeed = 3.5f;
    public float acceleration = 8.0f;
    public float catchDistance = 1.5f;

    [Header("Definições do Susto (Look At)")]
    public Transform enemyHead;
    public float lookAtSpeed = 8.0f;

    [Header("Deteção")]
    public float detectionRange = 15.0f;
    public bool stopIfPlayerHidden = true;
    public LayerMask obstacleMask;

    [Header("Eventos")]
    public UnityEvent onScareStart;
    public UnityEvent onCaughtPlayer;

    [Header("Behaviour Modifiers")]
    [SerializeField] private MaskData hatedMask;

    private NavMeshAgent _agent;
    private Renderer _renderer;
    private bool _isCaught = false;
    private bool _isActive = false;
    private Monster _monsterIdentity;

    // Referência direta ao controlador do jogador
    private PlayerController _playerController;

    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _renderer = GetComponent<Renderer>();
        _monsterIdentity = GetComponent<Monster>();

        if (_monsterIdentity == null) Debug.LogError($"ERRO: Falta script Monster em {gameObject.name}");

        _agent.speed = moveSpeed;
        _agent.acceleration = acceleration;

        if (playerCamera == null) playerCamera = Camera.main;
        if (playerTransform == null && playerCamera != null) playerTransform = playerCamera.transform;

        // Encontra o PlayerController automaticamente
        if (playerTransform != null)
            _playerController = playerTransform.GetComponent<PlayerController>();
    }

    void Update()
    {
        if (_isCaught) return;

        bool playerIsDisguised = Player.Instance.IsDisguised;
        bool isWearingHatedMask = (hatedMask != null && Player.Instance.CurrentMask == hatedMask);

        if (playerIsDisguised && !isWearingHatedMask)
        {
            _isActive = false;
            StopMoving();
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (!_isActive)
        {
            if (distanceToPlayer <= detectionRange && IsVisibleToPlayer())
                _isActive = true;
            else
            {
                StopMoving();
                return;
            }
        }

        if (distanceToPlayer <= catchDistance)
        {
            TriggerDateMode();
            return;
        }

        bool playerLookingAtMe = IsVisibleToPlayer();
        bool enemyCanSeePlayer = HasLineOfSightToPlayer();

        if (distanceToPlayer > detectionRange || playerLookingAtMe || (stopIfPlayerHidden && !enemyCanSeePlayer))
            StopMoving();
        else
            MoveTowardsPlayer();
    }

    bool IsVisibleToPlayer()
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(playerCamera);
        if (!GeometryUtility.TestPlanesAABB(planes, _renderer.bounds)) return false;
        Vector3 direction = _renderer.bounds.center - playerCamera.transform.position;
        return !Physics.Raycast(playerCamera.transform.position, direction, direction.magnitude, obstacleMask);
    }

    bool HasLineOfSightToPlayer()
    {
        Vector3 origin = transform.position + Vector3.up * 1.5f;
        Vector3 target = playerTransform.position + Vector3.up * 1.5f;
        return !Physics.Raycast(origin, target - origin, (target - origin).magnitude, obstacleMask);
    }

    void MoveTowardsPlayer() { _agent.isStopped = false; _agent.SetDestination(playerTransform.position); }
    void StopMoving() { if (!_agent.isStopped) { _agent.isStopped = true; _agent.velocity = Vector3.zero; } }

    void TriggerDateMode()
    {
        _isCaught = true;
        _agent.isStopped = true;
        _agent.velocity = Vector3.zero;

        // 1. DESLIGA O JOGADOR IMEDIATAMENTE (Código Automático)
        if (_playerController != null)
        {
            _playerController.enabled = false;
        }

        onScareStart.Invoke();
        StartCoroutine(ForceLookAtEnemy());
    }

    IEnumerator ForceLookAtEnemy()
    {
        Transform target = enemyHead != null ? enemyHead : transform;
        Quaternion startRot = playerCamera.transform.rotation;
        Quaternion endRot = Quaternion.LookRotation(target.position - playerCamera.transform.position);

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * lookAtSpeed;
            playerCamera.transform.rotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
        }
        playerCamera.transform.LookAt(target);

        // 2. PAUSA DE SEGURANÇA (1 segundo)
        yield return new WaitForSeconds(1.0f);

        // 3. INICIA DIÁLOGO
        if (_monsterIdentity != null && Player.Instance != null)
        {
            Player.Instance.InteractWithMonster(_monsterIdentity);
        }

        onCaughtPlayer.Invoke();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, catchDistance);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}