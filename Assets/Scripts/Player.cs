using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public float vida = 100f;
    public float estamina = 10f;

    public float fuerzaSalto = 7f;

    [Header("Ataque")]
    public float rango = 20f;
    public float daño = 25f;
    public float cadencia = 1.5f;
    public int balas = 10;

    public Camera camara;

    Rigidbody rb;
    CapsuleCollider capsule;

    InputAction movimiento;

    float velocidad = 5f;
    float sensibilidad = 0.1f;

    float rotacionX;
    float tiempoDisparo;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        movimiento = new InputAction("Movimiento");

        movimiento.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");

        movimiento.Enable();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    void Update()
    {
        if (vida <= 0)
            return;

        Camara();
        FOV();
        Salto();
        Estamina();
        Ataque();

        tiempoDisparo -= Time.deltaTime;
    }


    void FixedUpdate()
    {
        if (vida <= 0)
            return;

        Movimiento();
        Rotacion();
    }


    void Movimiento()
    {
        Vector2 input = movimiento.ReadValue<Vector2>();

        Vector3 direccion = new Vector3(
            input.x,
            0,
            input.y
        );

        direccion = transform.TransformDirection(direccion);

        Vector3 velocidadActual = rb.linearVelocity;

        rb.linearVelocity = new Vector3(
            direccion.x * velocidad,
            velocidadActual.y,
            direccion.z * velocidad
        );
    }


    void Rotacion()
    {
        Vector2 mouse = Mouse.current.delta.ReadValue();

        float rotacion = mouse.x * sensibilidad;

        rb.MoveRotation(
            rb.rotation *
            Quaternion.Euler(0, rotacion, 0)
        );
    }


    void Camara()
    {
        Vector2 mouse = Mouse.current.delta.ReadValue();

        rotacionX -= mouse.y * sensibilidad;

        rotacionX = Mathf.Clamp(
            rotacionX,
            -90,
            90
        );

        camara.transform.localRotation =
            Quaternion.Euler(
                rotacionX,
                0,
                0
            );
    }


    void FOV()
    {
        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            camara.fieldOfView -= 5f;
        }

        if (Keyboard.current.yKey.wasPressedThisFrame)
        {
            camara.fieldOfView += 5f;
        }

        camara.fieldOfView = Mathf.Clamp(
            camara.fieldOfView,
            50f,
            120f
        );
    }



    void Salto()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (EstaEnElPiso())
            {
                if (estamina >= 5)
                {
                    rb.AddForce(
                        Vector3.up * fuerzaSalto,
                        ForceMode.Impulse
                    );

                    estamina -= 5;
                }
            }
        }
    }


    bool EstaEnElPiso()
    {
        Vector3 origen = capsule.bounds.center;

        origen.y = capsule.bounds.min.y + 0.05f;

        return Physics.Raycast(
            origen,
            Vector3.down,
            0.15f
        );
    }


    void Estamina()
    {
        estamina += 2f * Time.deltaTime;

        if (estamina > 10)
            estamina = 10;

        if (estamina < 0)
            estamina = 0;
    }


    void Ataque()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (tiempoDisparo <= 0)
            {
                if (balas > 0)
                {
                    Disparar();
                }
            }
        }
    }


    void Disparar()
    {
        tiempoDisparo = cadencia;

        balas--;

        Ray ray = camara.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0)
        );

        RaycastHit impacto;

        if (Physics.Raycast(ray, out impacto, rango))
        {
            Debug.Log(
                "Impacto: " +
                impacto.collider.name
            );
        }
    }


    public void RecibirDaño(float cantidad)
    {
        vida -= cantidad;

        if (vida < 0)
            vida = 0;
    }
}
