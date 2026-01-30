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

    [Header("Deteção de Obstáculos")]
    [Tooltip("Camadas que bloqueiam a visão (ex: Paredes). Não inclua o Player ou o Inimigo aqui.")]
    public LayerMask obstacleMask;

    [Header("Eventos")]
    [Tooltip("O que acontece quando o inimigo apanha o jogador (Ativar HUD de Date, mudar câmara, etc.)")]
    public UnityEvent onCaughtPlayer;

    private NavMeshAgent _agent;
    private Renderer _renderer;
    private bool _isCaught = false; // Para garantir que o evento só dispara uma vez

    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _renderer = GetComponent<Renderer>();

        // Configuração inicial do NavMesh
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
        // Se já apanhou o jogador, para tudo e não faz mais cálculos
        if (_isCaught) return;

        // 1. Verificar a distância para o "Date Mode"
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= catchDistance)
        {
            TriggerDateMode();
            return;
        }

        // 2. Lógica de "Weeping Angel" (Mexe se não for visto)
        if (IsVisibleToPlayer())
        {
            StopMoving();
        }
        else
        {
            MoveTowardsPlayer();
        }
    }

    /// <summary>
    /// Verifica se o inimigo está dentro do campo de visão da câmara E sem obstáculos à frente.
    /// </summary>
    bool IsVisibleToPlayer()
    {
        // Passo A: Teste de Frustum (Está dentro do ângulo da câmara?)
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(playerCamera);

        // Se a caixa de colisão do renderizador não estiver dentro dos planos da câmara, não está visível
        if (!GeometryUtility.TestPlanesAABB(planes, _renderer.bounds))
        {
            return false;
        }

        Vector3 direction = _renderer.bounds.center - playerCamera.transform.position;
        float distance = direction.magnitude;

        // Se o raio bater numa parede antes de bater no inimigo, então o jogador não consegue vê-lo
        if (Physics.Raycast(playerCamera.transform.position, direction, out RaycastHit hit, distance, obstacleMask))
        {
            // Bateu em algo que não é o inimigo (uma parede), logo está escondido
            return false;
        }

        // Passou nos dois testes: Está no ecrã e sem paredes à frente.
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
    }
}