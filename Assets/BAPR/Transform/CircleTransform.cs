using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;



public class CircleTransform
{
    protected List<Type> collisionsTypes;
    protected bool isRender = true;
    int id;
    static int idsCounter;
    public bool isDebug = false;
    public bool isDelete { get; protected set; }
    public float speed { get; private set; }
    public float size { get; private set; }

    public Color color { get; private set; }

    public static List<CircleTransform> circles = new();

    public List<CircleTransform> touchedCircles { get; protected set; } = new();
    public List<CircleTransform> enterTouch { get; protected set; } = new();
    public List<CircleTransform> exitTouch { get; protected set; } = new();

    public float spawnTime { get; private set; }

    public Vector3 pos;

    static public string circlesCount;

    protected void MoveToDir(float direction)
    {
        float radians = direction * Mathf.Deg2Rad;

        float x = Mathf.Sin(radians) * speed;
        float y = Mathf.Cos(radians) * speed;

        pos = new Vector3(pos.x + x, pos.y + y, pos.z);
    }

    public CircleTransform()
    {
        id = idsCounter;
        idsCounter++;
        if (idsCounter == int.MaxValue - 1) idsCounter = int.MinValue;
        spawnTime = Time.time;
        circles.Add(this);
    }

    public void SetSize(float size)
    {
        this.size = size;
    }

    public void SetSpeed(float speed)
    {
        this.speed = speed;
    }

    public void SetColor(Color color)
    {
        this.color = color;
    }

    public virtual void Tick() { }

    static public float Distance2(Vector2 pos1, Vector2 pos2)
    {
        var _x = (pos1.x - pos2.x);
        var _y = (pos1.y - pos2.y);
        return (_x * _x) + (_y * _y);
    }
    public static void UpdateCollisions()
    {
        Vector2 cameraTop = (Vector2)Camera.main.ViewportToWorldPoint(new Vector3(0, 1, 0));
        Vector2 cameraBottom = cameraTop*-1;


        int i = 0;
        while (i < circles.Count)
        {
            if (circles[i].isDelete)
            {
                circles.RemoveAt(i);
            }
            else
            {
                i++;
            }
        }
        Stopwatch stopwatch = Stopwatch.StartNew();
        foreach (var circle in circles)
        {
            circle.isRender = (circle.pos.x < cameraBottom.x + circle.size / 2 &
               circle.pos.y > cameraBottom.y - circle.size / 2 &
               circle.pos.x > cameraTop.x - circle.size / 2 &
               circle.pos.y < cameraTop.y + circle.size / 2);
            //if (!circle.isRender) circle.isDelete = true;
            circle.enterTouch = new();
            circle.exitTouch = new();
            List<CircleTransform> touches;
            touches = new();
            circle.SetColor(new Color(1, 1, 1));
            foreach (var _circle in circles)
            {
                
                if (circle != _circle &
                    (_circle.collisionsTypes == null || _circle.collisionsTypes.Contains(_circle.GetType())))
                {
                    var dist = (circle.size + _circle.size) / 2;
                    dist = dist * dist;
                    if (Distance2(circle.pos, _circle.pos) < dist)
                    {
                        
                        touches.Add(_circle);
                        if (!circle.touchedCircles.Contains(_circle))
                        {
                            circle.enterTouch.Add(_circle);
                        }
                    }
                    else
                    {
                        if (circle.touchedCircles.Contains(_circle))
                        {
                            circle.exitTouch.Add(_circle);
                        }
                    }
                }
                
            }
            circle.touchedCircles = touches;
        }
        stopwatch.Stop();
        circlesCount = $"count:{circles.Count}\ncollision update:{stopwatch.ElapsedMilliseconds}\nFPS: ";
    }

    static Mesh quad;
    static MaterialPropertyBlock props;
    static RenderParams renderParams;
    public static void InitGraphic(Material circleMaterial)
    {
        renderParams = new RenderParams(circleMaterial);
        quad = Resources.GetBuiltinResource<Mesh>("Quad.fbx");
        props = new MaterialPropertyBlock();
    }
    public static void GraphicUpdate()
    {
        Matrix4x4[] matrices = new Matrix4x4[circles.Count];
        for (int i = 0; i < circles.Count; i++)
        {
            if (circles[i].isRender)
            {
                var c = circles[i];

                matrices[i].SetTRS(c.pos, Quaternion.identity, Vector3.one * c.size);
            }
        }
        Graphics.RenderMeshInstanced(renderParams, quad, 0, matrices);
    }

    public override int GetHashCode()
    {
        // Простой надёжный способ
        return HashCode.Combine(id);
    }

    public void Delete()
    {
        isDelete = true;
    }
}


public class ProjectileTransform : CircleTransform
{
    
    public float direction { get; private set; }
    const float MaxLifetime = 10;
    public float lifeTime = 10;
    public float lifeDistance = 0;
    public float distance { get; private set; }
    Vector3 lastPos;
    public bool isDeath;
    public void SetDirection(float direction)
    {
        this.direction = direction;
    }

    public ProjectileTransform() : base()
    {
        lastPos = pos;
    }


    public override void Tick()
    {
        if (isDeath) isDelete = true;
        if (isDelete) return;
        if (Time.time - spawnTime > Mathf.Min(MaxLifetime, lifeTime) || (lifeDistance != 0 & distance >= lifeDistance) || isRender == false)
        {
            isDeath = true;
        }

        MoveToDir(direction);

        distance += Vector2.Distance(pos,lastPos);
        lastPos = pos;
    }

    
}

public class PlayerTransform : CircleTransform
{
    
    public static PlayerTransform player; 
    public float direction { get; private set; }

    public void SetDirection(float direction)
    {
        this.direction = direction;
    }

    public PlayerTransform() : base()
    {
        player = this;
        collisionsTypes = new();
    }


    public override void Tick()
    {
        float direction = 0;
        bool isKeyMove = true;
        if (Input.GetKey(KeyCode.W))
        {
            if (Input.GetKey(KeyCode.A))
            {
                direction = -45;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                direction = 45;
            }
            else
            {
                direction = 0;
            }
        }
        else if (Input.GetKey(KeyCode.S))
        {
            if (Input.GetKey(KeyCode.A))
            {
                direction = 180 + 45;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                direction = 180 - 45;
            }
            else
            {
                direction = 180;
            }
        }
        else
        {
            if (Input.GetKey(KeyCode.A))
            {
                direction = -90;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                direction = 90;
            }
            else
            {
                isKeyMove = false;
            }
        }

        if (isKeyMove)
        {
            MoveToDir(direction);
        }
        else if (Input.GetMouseButton(0))
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0f;

            Vector2 dir = mouseWorld - pos;
            float angle = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg;
            if(Vector2.Distance(pos,mouseWorld) < speed)
            {
                pos = mouseWorld;
            }
            else
            {
                MoveToDir(angle);
            }
            
        }


    }
}


public class EnemyTransform : CircleTransform
{
    float health = 100f;
    public float direction { get; private set; }

    public void SetDirection(float direction)
    {
        this.direction = direction;
    }

    public EnemyTransform() : base()
    {
        collisionsTypes = new();
        collisionsTypes.Add(typeof(PlayerTransform));
    }

    public void Damage(float value)
    {
        health -= value;
        if (health <= 0f)
        {   
            isDelete = true;
        }
    }

    public override void Tick()
    {
        if (isDelete) return;
        Vector2 dir = PlayerTransform.player.pos - pos;
        float angle = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg;
        MoveToDir(angle);
    }
}