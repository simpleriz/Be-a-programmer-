using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class CardController : MonoBehaviour
{
    public static CardController instance;

    [SerializeField]
    TextMeshProUGUI pCounter;

    [SerializeField]
    Material circleMaterial;


    public TriggerCard mainCard;
    float deltaTime;
    private void Awake()
    {
        instance = this;

        mainCard = new BaseGameTrigger();
        var mainProjectile = new EthernalProjectile();
        mainCard.nestle.AddCard(mainProjectile);

        new PlayerTransform();

        PlayerTransform.player.SetSize(0.5f);
        PlayerTransform.player.SetColor(new Color(0.5f,1,0.5f,1));
        PlayerTransform.player.SetSpeed(0.025f);
    

        CircleTransform.InitGraphic(circleMaterial);
    }

    private void FixedUpdate()
    {
        mainCard.Tick(null);
        
        PlayerTransform.player.Tick();

        CircleTransform.UpdateCollisions();
        float fps = 1.0f / deltaTime;
        pCounter.text = CircleTransform.circlesCount + Mathf.RoundToInt(fps);

        
    }

    private void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }
    private void LateUpdate()
    {
        CircleTransform.GraphicUpdate();
    }

    public ProjectileTransform CreateProjectile()
    {
        ProjectileTransform projectile = new();

        return projectile;
    }
}


