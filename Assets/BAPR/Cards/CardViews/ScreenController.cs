using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ScreenController : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] float speed;
    float timer;
    bool isDrag;
    Vector3 dragStartPos;
    Vector3 offset;
    Vector3 startOffset;
    [SerializeField] new Transform transform;
    [SerializeField] GameObject UIObject;
    // Start is called before the first frame update
    void Start()
    {
        startOffset = transform.position;
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetMouseButtonUp(0))
        {
            isDrag = false;
        }

        if (isDrag)
        {
            transform.position = Input.mousePosition - dragStartPos + offset;
        }

        else
        {
            float _speed = (speed + (timer * speed)) * Time.deltaTime;

            bool isMove = false;
            if (Input.GetKey(KeyCode.D))
            {
                transform.position += new Vector3(-_speed, 0, 0);
                isMove = true;
            }
            if (Input.GetKey(KeyCode.A))
            {
                transform.position += new Vector3(_speed, 0, 0);
                isMove = true;
            }
            if (Input.GetKey(KeyCode.W))
            {
                transform.position += new Vector3(0, -_speed, 0);
                isMove = true;
            }
            if (Input.GetKey(KeyCode.S))
            {
                transform.position += new Vector3(0, _speed, 0);
                isMove = true;
            }

            if (isMove)
            {
                timer += Time.deltaTime;
            }
            else
            {
                timer = 0;
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            UIObject.SetActive(!UIObject.activeInHierarchy);
        }
    }

    public void ResetPosition()
    {
        transform.position = startOffset;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.pointerPressRaycast.gameObject != UIObject)
        {
            return;
        }
        isDrag = true;
        dragStartPos = Input.mousePosition;
        offset = transform.position;
    }
}
