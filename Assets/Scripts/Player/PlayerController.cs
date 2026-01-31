using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Configura��o de Input")]
    public InputActionReference moveAction;
    public InputActionReference lookAction;
    public InputActionReference sprintAction;
    public InputActionReference jumpAction;

    [Header("Movimento")]
    public float velocidadeAndar = 4.0f;
    public float velocidadeCorrer = 7.0f;
    public float gravidade = 20.0f;
    public float alturaPulo = 1.5f;

    [Header("C�mara")]
    public Transform cameraJogador;
    public float sensibilidadeMouse = 15.0f;
    public float limiteOlharCimaBaixo = 80.0f;

    [Header("Head Bob")]
    public bool ativarHeadBob = true;
    public float velocidadeBob = 12.0f;
    public float forcaBob = 0.05f;

    private CharacterController controller;
    private Vector3 direcaoMovimento = Vector3.zero;
    private float rotacaoX = 0;
    private float timerBob = 0;
    private float alturaPadraoCamera;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (cameraJogador != null)
            alturaPadraoCamera = cameraJogador.transform.localPosition.y;
    }

    void OnEnable()
    {
        if (moveAction) moveAction.action.Enable();
        if (lookAction) lookAction.action.Enable();
        if (sprintAction) sprintAction.action.Enable();
        if (jumpAction) jumpAction.action.Enable();
    }

    void OnDisable()
    {
        if (moveAction) moveAction.action.Disable();
        if (lookAction) lookAction.action.Disable();
        if (sprintAction) sprintAction.action.Disable();
        if (jumpAction) jumpAction.action.Disable();
    }

    void Update()
    {
        if (DialogManager.Instance.dialogueIsPlaying) return;

        MoverJogador();
        RodarCamera();

        if (ativarHeadBob)
            AplicarHeadBob();
    }

    void MoverJogador()
    {
        if (controller.isGrounded)
        {
            if (direcaoMovimento.y < 0) direcaoMovimento.y = -2f;

            Vector2 inputMove = Vector2.zero;
            if (moveAction != null) inputMove = moveAction.action.ReadValue<Vector2>();

            bool isSprinting = sprintAction != null && sprintAction.action.IsPressed();
            float velocidadeAtual = isSprinting ? velocidadeCorrer : velocidadeAndar;

            Vector3 move = transform.right * inputMove.x + transform.forward * inputMove.y;

            direcaoMovimento.x = move.x * velocidadeAtual;
            direcaoMovimento.z = move.z * velocidadeAtual;

            if (jumpAction != null && jumpAction.action.WasPressedThisFrame())
            {
                direcaoMovimento.y = Mathf.Sqrt(alturaPulo * 2f * gravidade);
            }
        }

        direcaoMovimento.y -= gravidade * Time.deltaTime;
        controller.Move(direcaoMovimento * Time.deltaTime);
    }

    void RodarCamera()
    {
        if (lookAction == null) return;
        Vector2 inputLook = lookAction.action.ReadValue<Vector2>();

        float mouseX = inputLook.x * sensibilidadeMouse * Time.deltaTime;
        float mouseY = inputLook.y * sensibilidadeMouse * Time.deltaTime;

        transform.Rotate(Vector3.up * mouseX);

        rotacaoX -= mouseY;
        rotacaoX = Mathf.Clamp(rotacaoX, -limiteOlharCimaBaixo, limiteOlharCimaBaixo);

        if (cameraJogador != null)
        {
            cameraJogador.localRotation = Quaternion.Euler(rotacaoX, 0f, 0f);
        }
    }

    void AplicarHeadBob()
    {
        Vector2 inputMove = Vector2.zero;
        if (moveAction != null) inputMove = moveAction.action.ReadValue<Vector2>();

        if (inputMove.magnitude > 0.1f && controller.isGrounded)
        {
            timerBob += Time.deltaTime * velocidadeBob;

            // C�lculo da posi��o da cabe�a
            float posicaoY = Mathf.Sin(timerBob);

            // Move a c�mara visualmente
            float novaPosicaoY = alturaPadraoCamera + posicaoY * forcaBob;
            cameraJogador.transform.localPosition = new Vector3(cameraJogador.transform.localPosition.x, novaPosicaoY, cameraJogador.transform.localPosition.z);
        }
        else
        {
            timerBob = 0;
            cameraJogador.transform.localPosition = new Vector3(cameraJogador.transform.localPosition.x, Mathf.Lerp(cameraJogador.transform.localPosition.y, alturaPadraoCamera, Time.deltaTime * 10), cameraJogador.transform.localPosition.z);
        }
    }
}