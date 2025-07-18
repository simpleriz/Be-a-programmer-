using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardDragger : MonoBehaviour, IPointerDownHandler
{
    public Card card { get; private set; }
    [SerializeField] TextMeshProUGUI cost;
    [SerializeField] TextMeshProUGUI header;
    [SerializeField] TextMeshProUGUI content;
    bool isDrag = false;
    Vector3 offset;
    CardView touchCard;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }
    // Update is called once per frame
    void Update()
    {
        if (isDrag)
        {
            if (Input.GetMouseButtonUp(0))
            {
                isDrag = false;
                if(touchCard != null)
                {
                    touchCard.DragCardTouch(this, true);
                    CardDraggerController.instance.cards.Remove(this);
                    Destroy(gameObject);
                }
                CardDraggerController.instance.UpdateView();
                return;
            }

            transform.position = Input.mousePosition + offset;

            PointerEventData pointerData = new PointerEventData(CardDraggerController.instance.eventSystem);
            pointerData.position = Input.mousePosition;

            List<RaycastResult> results = new List<RaycastResult>();
            CardDraggerController.instance.raycaster.Raycast(pointerData, results);

            foreach (RaycastResult result in results)   
            {
                if (result.gameObject.CompareTag("GameCard"))
                {

                    var _touchCard = result.gameObject.GetComponent<CardView>();
                    if(touchCard != _touchCard)
                    {
                        if(touchCard != null)
                        {
                            touchCard.DiscardCardTouch();
                        }
                        if (_touchCard.DragCardTouch(this))
                        {
                            touchCard = _touchCard;
                        }
                    }
                    else
                    {
                        _touchCard.DragCardTouch(this);
                    }
                    
                    return;
                }
            }
            if (touchCard != null) {

                touchCard.DiscardCardTouch();
                touchCard = null;
            }
        }
    }

    public void SetCard(Card card)
    {
        card.description.SetCostText(cost);
        card.description.SetHeaderText(header);
        card.description.SetDescriptionText(content);
        this.card = card;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isDrag = true;
        offset = transform.position - Input.mousePosition;
    }
}
