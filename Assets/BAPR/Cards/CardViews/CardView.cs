using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardView : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] TextMeshProUGUI header;
    [SerializeField] TextMeshProUGUI cost;
    [SerializeField] TextMeshProUGUI content;
    [SerializeField] GameObject nestleButton;
    [SerializeField] GameObject headerButtons;
    [SerializeField] GameObject rightCardPos;
    [SerializeField] GameObject leftCardPos;
    CardViewController viewController;
    public Card card { get; private set; }
    public bool isHeader = false;

    bool isEditable = true;

    public void SetCard(Card card, CardViewController controller, bool isHeader = false)
    {
        if (card as TriggerCard == null)
        {
            isEditable = true;
        }
        else if(isHeader == true & card as TriggerCard != null)
        {
            isEditable = true;
        }
        else
        {
            isEditable = false;
        }

        this.isHeader = isHeader;
        viewController = controller;
        this.card = card;
        UpdateView();
    }

    public void UpdateView()
    {
        card.description.SetCostText(cost);
        card.description.SetHeaderText(header);
        card.description.SetDescriptionText(content);
        if ((viewController.locked == false & (card as TriggerCard != null || card as ProjectileCard != null)) & isHeader == false)
        {
            nestleButton.SetActive(true);
        }
        else
        {
            nestleButton.SetActive(false);
        }
        if (isHeader & viewController.isMain == false)
        {
            headerButtons.SetActive(true);
        }
        else
        {
            headerButtons.SetActive(false);
        }
    }

    public void OpenNestle()
    {
        var inst = Instantiate(CardViewController.instance.CardViewControllerPrefab, CardViewController.instance.transform.parent);
        
        inst.transform.position = transform.position;
        var controller = inst.GetComponent<CardViewController>();
        controller.SetParent(viewController);
        if (card as TriggerCard != null)
        {
            controller.UpdateView(card as TriggerCard);
        }
        else
        {
            controller.UpdateView(card as ProjectileCard);
        }
    }

    public void CloseNestle()
    {
        viewController.Close();
    }

    public void DeleteCard()
    {

    }

    public void SortNestle()
    {
        viewController.SortNestle();
    }

    public bool DragCardTouch(CardDragger _card, bool isSet = false)
    {
        if (!isEditable) return false;
        if(Input.mousePosition.x > transform.position.x || isHeader)
        {
            if (isSet)
            {
                viewController.AddCardToNestle(this, _card, true);
            }
            else
            {
                rightCardPos.SetActive(true);
                leftCardPos.SetActive(false);
            }
        }
        else
        {
            
            if(isSet)
            {
                viewController.AddCardToNestle(this, _card, false);
            }
            else
            {
                leftCardPos.SetActive(true);
                rightCardPos.SetActive(false);
            }
        }
        return true;
    }


    public void DiscardCardTouch()
    {
        leftCardPos.SetActive(false);
        rightCardPos.SetActive(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(!isEditable || isHeader) return;
        CardDraggerController.instance.NewCard(card.ToDrag(),transform.position);
        viewController.UpdateView();

    }
}
