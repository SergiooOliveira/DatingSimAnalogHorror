using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Renderer))]
public class StalkerAI : MonoBehaviour
{
    [Header("Configurações do Jogador")]
    [Tooltip("Arrasta a câmara principal do jogador para aqui")]
    public Camera playerCamera;
    [Tooltip("Arrasta o objeto do jogador para aqui (o alvo do movimento)")]
    public Transform playerTransform;

    [Header("Definições de Comportamento")]
    [Tooltip("Velocidade do inimigo quando não está a ser observado")]
    public float moveSpeed = 3.5f;
    [Tooltip("Aceleração para dar um efeito mais errático")]
    public float acceleration = 8.0f;
    [Tooltip("Distância mínima para ativar o modo 'Date'")]
    public float catchDistance = 1.5f;

    [Header("Deteção e Ativação")]
    [Tooltip("Distância máxima para o inimigo detetar o jogador.")]
    public float detectionRange = 15.0f;
    [Tooltip("Se ativado, o inimigo para se o jogador estiver escondido atrás de uma parede (mesmo que o jogador não esteja a olhar).")]
    public bool stopIfPlayerHidden = true;

    [Header("Deteção de Obstáculos")]
    [Tooltip("Camadas que bloqueiam a visão (ex: Paredes). Não inclua o Player ou o Inimigo aqui.")]
    public LayerMask obstacleMask;

    [Header("Eventos")]
    [Tooltip("O que acontece quando o inimigo apanha o jogador (Ativar HUD de Date, mudar câmara, etc.)")]
    public UnityEvent onCaughtPlayer;

    private NavMeshAgent _agent;
    private Renderer _renderer;
    private bool _isCaught = false;
    private bool _isActive = false;

    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _renderer = GetComponent<Renderer>();

        _agent.speed = moveSpeed;
        _agent.acceleration = acceleration;

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            Debug.LogWarning("Nenhuma câmara atribuída ao StalkerAI. A usar Camera.main.");
        }

        if (playerTransform == null && playerCamera != null)
        {
            playerTransform = playerCamera.transform;
        }
    }

    void Update()
    {
        // Se já apanhou o jogador, para tudo
        if (_isCaught) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (!_isActive)
        {
            if (distanceToPlayer <= detectionRange && IsVisibleToPlayer())
            {
                _isActive = true;
                Debug.Log("Inimigo ativado! A perseguição começou.");
            }
            else
            {
                // Enquanto não ativa, fica parado
                StopMoving();
                return;
            }
        }

        // --- LÓGICA DE CAPTURA ---
        if (distanceToPlayer <= catchDistance)
        {
            TriggerDateMode();
            return;
        }

        if (distanceToPlayer > detectionRange)
        {
            StopMoving();
            return;
        }


        bool playerLookingAtMe = IsVisibleToPlayer();
        bool enemyCanSeePlayer = HasLineOfSightToPlayer();

        if (playerLookingAtMe)
        {
            StopMoving();
        }
        else if (stopIfPlayerHidden && !enemyCanSeePlayer)
        {
            StopMoving();
        }
        else
        {
            MoveTowardsPlayer();
        }
    }

    /// <summary>
    /// Verifica se o jogador está a olhar para o inimigo.
    /// </summary>
    bool IsVisibleToPlayer()
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(playerCamera);

        if (!GeometryUtility.TestPlanesAABB(planes, _renderer.bounds))
        {
            return false;
        }

        Vector3 direction = _renderer.bounds.center - playerCamera.transform.position;
        float distance = direction.magnitude;

        if (Physics.Raycast(playerCamera.transform.position, direction, distance, obstacleMask))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Verifica se o INIMIGO consegue ver o JOGADOR (para não perseguir através de paredes).
    /// </summary>
    bool HasLineOfSightToPlayer()
    {
        Vector3 origin = transform.position + Vector3.up * 1.5f;
        Vector3 target = playerTransform.position + Vector3.up * 1.5f;

        Vector3 direction = target - origin;
        float distance = direction.magnitude;

        if (Physics.Raycast(origin, direction, distance, obstacleMask))
        {
            return false;
        }

        return true;
    }

    void MoveTowardsPlayer()
    {
        if (_agent.isStopped)
        {
            _agent.isStopped = false;
        }
        _agent.SetDestination(playerTransform.position);
    }

    void StopMoving()
    {
        if (!_agent.isStopped)
        {
            _agent.isStopped = true;
            _agent.velocity = Vector3.zero; 
        }
    }

    void TriggerDateMode()
    {
        _isCaught = true;
        _agent.isStopped = true;

        Debug.Log("Dating Simulator Mode Activated! <3");
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