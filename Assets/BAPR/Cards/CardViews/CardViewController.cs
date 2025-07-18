using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class CardViewController : MonoBehaviour
{

    List<CardView> cards = new();
    [SerializeField] GameObject CardPrefab;

    public bool isMain = false;
    public static CardViewController instance;
    public GameObject CardViewControllerPrefab;
    public CardViewController parent { get; private set; }

    CardView mainCard;
    public UnityEvent OnCLose = new();
    public bool locked { get; private set; } = false;

    const float lineOffset = 135;

    private void Awake()
    {
        if(isMain) instance = this;
    }
    private void Start()
    {
        if (isMain)
        {
            UpdateView(CardController.instance.mainCard.nestle.cards[0] as ProjectileCard);
        }
    }

    public void UpdateView(TriggerCard card)
    {
        OnCLose.Invoke();

        while(cards.Count != 0)
        {
            Destroy(cards[0].gameObject);
            cards.RemoveAt(0);
        }

        mainCard = NewCard(card, true);

        foreach (var _card in card.nestle.cards)
        {
            if(_card.isDelete == false) NewCard(_card);
        }
    }

    public void UpdateView()
    {
        TriggerCard _card = mainCard.card as TriggerCard;
        if (_card != null)
        {
            _card.nestle.SortNestle();
            UpdateView(_card);
        }
        else
        {
            ProjectileCard __card = mainCard.card as ProjectileCard;
            __card.nestle.SortNestle();
            UpdateView(__card);
        }
    }

    public void AddCardToNestle(CardView view, CardDragger card, bool isRight)
    {
        int index = cards.IndexOf(view);
        var _card = mainCard.card as TriggerCard;
        if(_card == null) return;

        if (isRight)
        {
            _card.nestle.AddCard(card.card, index);
        }
        else
        {
            _card.nestle.AddCard(card.card, index-1);
        }
        

        UpdateView(mainCard.card as TriggerCard);
        BaseGameTrigger.UpdateCosts();
    }

    public void SortNestle()
    {
        TriggerCard _card = mainCard.card as TriggerCard;
        if(_card != null)
        {
            _card.nestle.SortNestle();
            UpdateView(_card);
        }
        else
        {
            ProjectileCard __card = mainCard.card as ProjectileCard;
            __card.nestle.SortNestle();
            UpdateView(__card);
        }

        
    }

    public void UpdateView(ProjectileCard card)
    {
        mainCard = NewCard(card, true);

        foreach (var _card in card.nestle.cards)
        {
            NewCard(_card);
        }
    }

    CardView NewCard(Card card,bool isHeader = false)
    {
        var inst = Instantiate(instance.CardPrefab, transform);
        var view = inst.GetComponent<CardView>();

        view.SetCard(card,this, isHeader);
        cards.Add(view);

        return view;
    }

    public void Close()
    {
        if (isMain) return;
        OnCLose.Invoke();
        parent.SetLock(false);
        Destroy(gameObject);
    }

    public void SetParent(CardViewController parent)
    {
        this.parent = parent;
        transform.position = new Vector3(transform.position.x, parent.transform.position.y - lineOffset, transform.position.y);
        this.parent.SetLock(true);
        this.parent.OnCLose.AddListener(Close);
    }

    public void SetLock(bool _locked = false)
    {
        locked = _locked;
        foreach(var card in cards)
        {
            card.UpdateView();
        }
    }
}
