using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Player : NetworkBehaviour
{
    public float speed = 10F;
    public Rigidbody2D rb;
    public SpriteRenderer sr;

    // Puede enviarse cualquier variable sincronizada y hasta 60 por script
    // El hook es el nombre de la función que se llama cuando la variable
    // cambia, si no lo necesitamos, podemos llamar simplemente
    // [SyncVar]
    [SyncVar(hook = nameof(SetColor))]
    Color playerColor = Color.white;

    Dictionary<KeyCode, Color> colors = new Dictionary<KeyCode, Color>(){
        {KeyCode.Alpha1, Color.white},
        {KeyCode.Alpha2, Color.red},
        {KeyCode.Alpha3, Color.green},
        {KeyCode.Alpha4, Color.gray},
        {KeyCode.Alpha5, Color.blue},
        {KeyCode.Alpha6, Color.yellow}
    };


    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        foreach (var keycode in colors.Keys)
        {
            if (Input.GetKeyDown(keycode))
            {
                CmdChangeColor(colors[keycode]);
            }
        }
    }

    private void FixedUpdate()
    {
        if (!isLocalPlayer) return;

        rb.velocity = new Vector2(0,Input.GetAxisRaw("Vertical")) * speed * Time.deltaTime;
    }

    void SetColor(Color oldColor, Color newColor)
    {
        sr.color = newColor;
    }

    // Los comandos son funciones que se llaman en el
    // Servidor desde los clientes
    // Siempre empiezan por Cmd como los RPCs
    [Command]
    public void CmdChangeColor(Color newColor)
    {
        playerColor = newColor;
    }
}
