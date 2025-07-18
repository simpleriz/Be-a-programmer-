using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class CardDraggerController : MonoBehaviour
{
    public static CardDraggerController instance;

    public GraphicRaycaster raycaster;
    public EventSystem eventSystem;

    [SerializeField] GameObject draggerPrefab;

    public List<CardDragger> cards;

    [SerializeField] Transform handRight;
    [SerializeField] Transform handLeft;
    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        NewCard(new RotationAction());
        NewCard(new RotationAction());
        NewCard(new RotationAction());
        NewCard(new RotationAction());
        NewCard(new RotationAction());
        NewCard(new RotationAction());
        NewCard(new RotationAction());
        NewCard(new RotationAction());
        NewCard(new RotationAction());
        NewCard(new RotationAction());

        NewCard(new EthernalProjectile());
        NewCard(new EthernalProjectile());
        NewCard(new EthernalProjectile());
        NewCard(new EthernalProjectile());
        NewCard(new EthernalProjectile());

        NewCard(new DamageAction());
        NewCard(new DamageAction());
        NewCard(new DamageAction());
        NewCard(new DamageAction());
        NewCard(new DamageAction());
    }

    public void UpdateView()
    {
        if (cards.Count == 0) return;
        var width = handRight.position.x - handLeft.position.x;

        if (cards.Count == 1)
        {
            var center = handLeft.position.x + width / 2;
            cards[0].transform.position = new Vector3(center, handRight.position.y, handRight.position.z);
            return;
        }
        var step = width / (cards.Count - 1);
        int stepCount = 0;
        foreach (var card in cards.Select(i => i.transform))
        {
            card.position = new Vector3(handRight.position.x - (step*stepCount), handRight.position.y, handRight.position.z);
            stepCount++;
        }
    }

    public void NewCard(Card card)
    {
        var inst = Instantiate(draggerPrefab, transform);
        cards.Add(inst.GetComponent<CardDragger>());
        cards.Last().SetCard(card);
        UpdateView();
    }

    public void NewCard(Card card, Vector3 pos)
    {
        var inst = Instantiate(draggerPrefab, transform);
        inst.transform.position = pos;
        var _card = inst.GetComponent<CardDragger>();
        cards.Add(_card);
        _card.SetCard(card);
        _card.OnPointerDown(null);
    }
}
