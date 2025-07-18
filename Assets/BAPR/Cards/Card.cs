using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public abstract class Card
{
    public CardDescription description = new();
    protected float cost = 1;
    public bool isDelete { get ; protected set; }
    public virtual float GetCost(float indexCost)
    {
        return 0;
    }

    public virtual Card ToDrag()
    {
        isDelete = true;
        return this;
    }
}


public abstract class ProjectileCard : ActionCard
{
    protected List<ProjectileTransform> projectiles = new();
    public ProjectileNestle nestle = new();
    int tickCounter;

    public override Card ToDrag()
    {
        base.ToDrag();  
        foreach (var projectile in projectiles)
        {
            projectile.Delete();
            
        }
        projectiles = null;
        nestle.ToHand();
        return this;
    }
    public override void Tick()
    {
        tickCounter++;  
        int i = 0;
        while (i < projectiles.Count)
        {
            if (projectiles[i].isDelete)
            {
                projectiles.RemoveAt(i);
            }
            else
            {
                i++;
            }
        }
        foreach (ProjectileTransform p in projectiles)
        {
            p.Tick();
        }
        nestle.Tick(projectiles);
    }

    public override float GetCost(float indexCost)
    {
        var _cost = nestle.GetCost();
        description.SetCost($"{Mathf.Round(indexCost*100)/100} x ({Mathf.Round(cost * 100) / 100} + {Mathf.Round(_cost * 100) / 100})");
        return (_cost + cost) * indexCost;
    }
}

public abstract class TriggerCard : Card
{
    public TriggerNestle nestle = new();

    public override Card ToDrag()
    {
        nestle.ToHand();
        return null;
    }

    public virtual void Tick(List<ProjectileTransform> projectiles)
    {
        nestle.Tick();
    }

    protected void Activate(ProjectileTransform projectile)
    {
        nestle.Activate(projectile);
    }

    public override float GetCost(float indexCost)
    {
        var _cost = nestle.GetCost();
        description.SetCost($"{Mathf.Round(cost * 100) / 100} x {Mathf.Round(_cost * 100) / 100}");
        return _cost*cost*indexCost;
    }
}

public abstract class ActionCard : Card
{
    public virtual void Tick()
    {
    }

    public abstract void Activate(ProjectileTransform projectile);

    public override float GetCost(float indexCost)
    {
        description.SetCost($"{Mathf.Round(indexCost * 100) / 100} x {Mathf.Round(cost * 100) / 100}");
        return cost * indexCost;
    }
}


public abstract class CardNestle : Card
{
    public abstract void SortNestle();
    public abstract void AddCard(Card card, int index);

    public abstract void AddCard(Card card);

    public abstract bool CheckCard(Card card);

    public abstract void RemoveCard(Card card);

    public virtual float GetCost()
    {
        return 0;
    }
}

public class ProjectileNestle : CardNestle
{
    public List<TriggerCard> cards { get; protected set; } = new();
    public float costStep = 0.25f;
    public float startCost = 1;


    public void ToHand()
    {
        foreach (var card in cards)
        {
            card.nestle.ToHand();
        }

    }
    public override void AddCard(Card card)
    {
        if (CheckCard(card))
        {
            cards.Add(card as TriggerCard);
        }
    }

    public override float GetCost()
    {
        return cards.Select((value, index) => value.GetCost(index * costStep + startCost)).Sum();
    }

    public override void SortNestle()
    {
        cards = cards.OrderBy(i => i.GetCost(-1)).ToList();
    }

    public override void AddCard(Card card, int index)
    {
        if (index > cards.Count)
        {
            AddCard(card);
            return;
        }
        if (CheckCard(card))
        {
            cards.Insert(index,card as TriggerCard);
        }
    }

    public void Tick(List<ProjectileTransform> projectiles)
    {
        foreach (TriggerCard card in cards)
        {
            card.Tick(projectiles);
        }
    }

    public override bool CheckCard(Card card)
    {
        var _card = card as TriggerCard;
        if (_card != null)
        {
            return true;
        }
        return false;
    }

    public override void RemoveCard(Card card)
    {
        if (cards.Contains(card as TriggerCard))
        {
            cards.Remove(card as TriggerCard);
        }
    }
}

public class TriggerNestle : CardNestle
{
    public List<ActionCard> cards { get; protected set; } = new();
    public float costStep = 0.25f;
    public float startCost = 1;

    public void Tick()
    {
        cards.RemoveAll(i => i.isDelete);
        foreach (ActionCard card in cards)
        {
            card.Tick();
        }
    }

    public void ToHand()
    {
        foreach (var card in cards)
        {
            CardDraggerController.instance.NewCard(card.ToDrag());
        }

    }

    public void Activate(ProjectileTransform projectile)
    {
        foreach (ActionCard card in cards)
        {
            card.Activate(projectile);
        }
    }

    public override void SortNestle()
    {
        cards = cards.OrderBy(i => i.GetCost(-1)).ToList();

    }
    public override float GetCost()
    {
        return cards.Select((value, index) => value.GetCost(index * costStep + startCost)).Sum();
    }

    public override void AddCard(Card card)
    {


        if (CheckCard(card))
        {
            cards.Add(card as ActionCard);
        }
    }

    public override void AddCard(Card card, int index)
    {
        if (index > cards.Count)
        {
            AddCard(card);
            return;
        }
        if (CheckCard(card))
        {
            cards.Insert(index,card as ActionCard);
        }
    }


    public override bool CheckCard(Card card)
    {
        var _card = card as ActionCard;
        if (_card != null)
        {
            return true;
        }
        return false;
    }

    public override void RemoveCard(Card card)
    {
        if (cards.Contains(card as ActionCard))
        {
            cards.Remove(card as ActionCard);
        }
    }
}

public class CardDescription
{
    TextMeshProUGUI headerText;
    TextMeshProUGUI descriptionText;
    TextMeshProUGUI costText;

    string header;
    string description;
    string cost;

    public void SetCost(string content)
    {
        cost = content;
        if (costText != null) costText.text = cost;
    }

    public void SetDescription(string content)
    {
        description = content;
        if(descriptionText != null)descriptionText.text = description;
    }

    public void SetHeader(string content)
    {
        header = content;
        if (headerText != null) headerText.text = header;
    }

    public void SetCostText(TextMeshProUGUI content)
    {
        costText = content;
        if (costText != null) costText.text = cost;
    }

    public void SetDescriptionText(TextMeshProUGUI content)
    {
        descriptionText = content;
        if (descriptionText != null) descriptionText.text = description;
    }

    public void SetHeaderText(TextMeshProUGUI content)
    {
        headerText = content;
        if (headerText != null) headerText.text = header;
    }
}


public class CardCollections
{
    protected List<(Card, CardRarity)> cards = new();
    public virtual List<Card> GetCard(int count,CardRarity rarity)
    {
        return cards.Where(i => i.Item2 == rarity).OrderBy(i => UnityEngine.Random.Range(-100, 100)).Select(i => i.Item1).Take(count).ToList();
    }
}

public enum CardRarity
{
    Common,
    Rarity,
    Special,
    SpecialRarity
}