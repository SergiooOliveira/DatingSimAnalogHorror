using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Renderer))]
public class StalkerAI : MonoBehaviour
{
    [Header("Configura��es do Jogador")]
    [Tooltip("Arrasta a c�mara principal do jogador para aqui")]
    public Camera playerCamera;
    [Tooltip("Arrasta o objeto do jogador para aqui (o alvo do movimento)")]
    public Transform playerTransform;

    [Header("Defini��es de Comportamento")]
    [Tooltip("Velocidade do inimigo quando n�o est� a ser observado")]
    public float moveSpeed = 3.5f;
    [Tooltip("Acelera��o para dar um efeito mais err�tico")]
    public float acceleration = 8.0f;
    [Tooltip("Dist�ncia m�nima para ativar o modo 'Date'")]
    public float catchDistance = 1.5f;

    [Header("Dete��o e Ativa��o")]
    [Tooltip("Dist�ncia m�xima para o inimigo detetar o jogador.")]
    public float detectionRange = 15.0f;
    [Tooltip("Se ativado, o inimigo para se o jogador estiver escondido atr�s de uma parede (mesmo que o jogador n�o esteja a olhar).")]
    public bool stopIfPlayerHidden = true;

    [Header("Dete��o de Obst�culos")]
    [Tooltip("Camadas que bloqueiam a vis�o (ex: Paredes). N�o inclua o Player ou o Inimigo aqui.")]
    public LayerMask obstacleMask;

    [Header("Eventos")]
    [Tooltip("O que acontece quando o inimigo apanha o jogador (Ativar HUD de Date, mudar c�mara, etc.)")]
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
            Debug.LogWarning("Nenhuma c�mara atribu�da ao StalkerAI. A usar Camera.main.");
        }

        if (playerTransform == null && playerCamera != null)
        {
            playerTransform = playerCamera.transform;
        }
    }

    void Update()
    {
        // Se j� apanhou o jogador, para tudo
        if (_isCaught) return;

        if (Player.Instance.IsDisguised)
        {
            _isActive = false;
            StopMoving();
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (!_isActive)
        {
            if (distanceToPlayer <= detectionRange && IsVisibleToPlayer())
            {
                _isActive = true;
                Debug.Log("Inimigo ativado! A persegui��o come�ou.");
            }
            else
            {
                // Enquanto n�o ativa, fica parado
                StopMoving();
                return;
            }
        }

        // --- L�GICA DE CAPTURA ---
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
    /// Verifica se o jogador est� a olhar para o inimigo.
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
    /// Verifica se o INIMIGO consegue ver o JOGADOR (para n�o perseguir atrav�s de paredes).
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