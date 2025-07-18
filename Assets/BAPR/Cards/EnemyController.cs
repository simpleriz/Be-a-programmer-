using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public static EnemyController instance;

    [SerializeField]
    GameObject enemyPrefab;

    float timer = 0f;
    const float spawnrate = 1f;

    List<EnemyTransform> enemies = new();
    private void Awake()
    {
        instance = this;
    }
    private void FixedUpdate()
    {
        foreach (var enemy in enemies)
        {
            enemy.Tick();
        }
        timer += Time.fixedDeltaTime;
        if (timer > spawnrate)
        {
            timer = 0;
            CreateEnemy();
        }
    }

    void CreateEnemy()
    {
        enemies.Add(new EnemyTransform());

        float r = 15 * Mathf.Sqrt(Random.value);
        float theta = Random.value * 2f * Mathf.PI;

        Vector2 point = new Vector2(
            r * Mathf.Cos(theta),
            r * Mathf.Sin(theta)
        );

        enemies.Last().SetSize(0.3f);
        enemies.Last().SetSpeed(0.01f);
        enemies.Last().pos = point;
    }
}
