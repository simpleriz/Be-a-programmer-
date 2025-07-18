using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst.Intrinsics;
using UnityEngine;
using static UnityEditor.PlayerSettings;


public class EnemyTouchTrigger : TriggerCard
{
    Type enemyType = typeof(EnemyTransform);

    public EnemyTouchTrigger()
    {
        description.SetDescription("Вызывается при касании к врагу");
        description.SetHeader("Столкновитель");
    }
    public override void Tick(List<ProjectileTransform> projectiles)
    {
        base.Tick(projectiles);
        foreach (ProjectileTransform projectile in projectiles)
        {
            if (projectile.enterTouch.Any(i => i.GetType() == enemyType))
            {
                Activate(projectile);
            }
        }
    }

    public override Card ToDrag()
    {
        base.ToDrag();
        return new EnemyTouchTrigger();
    }
}

public class BaseGameTrigger : TriggerCard
{
    float timer = 10f;
    float duration = 10f;
    static BaseGameTrigger inst;
    public BaseGameTrigger()
    {
        inst = this;
        nestle.startCost = 1;
        nestle.costStep = 0.25f;
        cost = 1;

        description.SetDescription("Код написанный справа будет вызываться каждые 10 секунд.");
        description.SetHeader("4S script");
    }

    public override void Tick(List<ProjectileTransform> projectiles)
    {
        base.Tick(projectiles);
        timer += Time.fixedDeltaTime;
        if (timer < 0.25f) return;
        duration = GetCost(1);  
        timer = 0;
        Activate(null);
    }

    public static void UpdateCosts()
    {
        inst.GetCost(1);
    }
}


public class CommonTicker : TriggerCard
{
    float duration;
    Dictionary<ProjectileTransform, float> timers = new();

    public CommonTicker(float frequency,float cost)
    {
        nestle.startCost = 1;
        nestle.costStep = 0.5f;
        duration = frequency;
        this.cost = cost;

        description.SetDescription($"Вызывается раз в <b>{frequency}</b> сек");
        description.SetHeader("Цикл");
    }
    public override void Tick(List<ProjectileTransform> projectiles)
    {
        base.Tick(projectiles);
        foreach (ProjectileTransform projectile in projectiles)
        {
            if (!timers.ContainsKey(projectile))
            {
                timers.Add(projectile, 0);
            }
            timers[projectile] += Time.fixedDeltaTime;
            if (timers[projectile] >= duration)
            {
                timers[projectile] = 0;
                Activate(projectile);
            }
        }
    }
}

public class DistanceTicker : TriggerCard
{
    float distance;
    Dictionary<ProjectileTransform, float> distances = new();

    public DistanceTicker(float distance, float cost)
    {
        nestle.startCost = 1;
        nestle.costStep = 0.5f;
        this.distance = distance;
        this.cost = cost;

        description.SetDescription($"Вызывается каждые <b>{distance}</b> метров");
        description.SetHeader("Цикл");
    }
    public override void Tick(List<ProjectileTransform> projectiles)
    {
        base.Tick(projectiles);
        foreach (ProjectileTransform projectile in projectiles)
        {
            if (!distances.ContainsKey(projectile))
            {
                distances.Add(projectile, 0);
            }
;
            if (distances[projectile]-projectile.distance >= distance)
            {
                distances[projectile] = projectile.distance;
                Activate(projectile);
            }
        }
    }
}

public class StartTrigger : TriggerCard
{
    List<ProjectileTransform> projectiles = new();
    public StartTrigger(float cost)
    {
        nestle.startCost = 1;
        nestle.costStep = 0.5f;
        this.cost = cost;

        description.SetDescription("Вызывается при создании снаряда");
        description.SetHeader("Инициализатор");
    }
    public override void Tick(List<ProjectileTransform> projectiles)
    {
        base.Tick(projectiles);
        foreach (ProjectileTransform projectile in projectiles)
        {
            if (!this.projectiles.Contains(projectile))
            {
                this.projectiles.Add(projectile);
                Activate(projectile);
            }
        }
    }
}

public class DeathTrigger : TriggerCard
{
    public DeathTrigger(float cost)
    {
        nestle.startCost = 1;
        nestle.costStep = 0.5f;
        this.cost = cost;

        description.SetDescription("Вызывается при унчтожении снаряда");
        description.SetHeader("Деконструктор");
    }
    public override void Tick(List<ProjectileTransform> projectiles)
    {
        base.Tick(projectiles);
        foreach (ProjectileTransform projectile in projectiles)
        {
            if (projectile.isDeath)
            {
                Activate(projectile);
            }
        }
    }
}

public abstract class CommonProjectile : ProjectileCard
{
    float pLifeTime;
    float pDistance;
    float pSize;
    float pSpeed;

    public CommonProjectile(float cost, float baseSpeed, float size, float maxLifeTime, float maxDistance = 0)
    {
        this.cost = cost;
        pSpeed = baseSpeed;
        pSize = size;
        pLifeTime = maxLifeTime;
        pDistance = maxDistance;
        

        description.SetDescription($"Создаёт снаряд:\nСкорость:{pSize}\nРазмер:{pSpeed}\nВ направлении:{DirectionDescription()}");
        description.SetHeader("снаряд");
    }
    public override void Activate(ProjectileTransform projectile)
    {
        var p = CardController.instance.CreateProjectile();
        projectiles.Add(p);
        if (projectile == null)
        {
            p.pos = PlayerTransform.player.pos;
        }
        else
        {
            p.pos = projectile.pos;
        }
        
        p.SetSize(pSize);
        p.SetSpeed(pSpeed);
        SetDirection(p);
    }

    protected abstract string DirectionDescription();

    protected abstract void SetDirection(ProjectileTransform projectile);
}

abstract class NearEnemyProjectile : CommonProjectile
{
    Type enemyType = typeof(EnemyTransform);
    public NearEnemyProjectile(float cost,  float baseSpeed, float size, float maxLifeTime, float maxDistance = 0) : base(cost, baseSpeed, size, maxLifeTime, maxDistance)
    {
    }

    protected override string DirectionDescription()
    {
        return "ближайшего оппонента";
    }

    protected override void SetDirection(ProjectileTransform projectile)
    {
        var _c = CircleTransform.circles.Where(i => i.GetType() == enemyType)
            .OrderBy(i => CircleTransform.Distance2(i.pos, projectile.pos)).First();
        Vector2 dir = _c.pos - projectile.pos;
        float angle = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg;
        projectile.SetDirection(angle);
    }
}

class BaseProjectile1 : NearEnemyProjectile
{
    public BaseProjectile1() : base(5, 350, 80, 4, 0)
    {
        nestle.AddCard(new CommonTicker(1, 1));
        nestle.AddCard(new DistanceTicker(400, 1));
    }

    public override Card ToDrag()
    {
        base.ToDrag();
        return new BaseProjectile1();
    }
}

class BaseProjectile2 : NearEnemyProjectile
{
    public BaseProjectile2() : base(8, 50, 90, 6, 0)
    {
        nestle.AddCard(new CommonTicker(1, 2));
        nestle.AddCard(new DeathTrigger(0.4f));
    }

    public override Card ToDrag()
    {
        base.ToDrag();
        return new BaseProjectile2();
    }
}

class BaseProjectile3 : NearEnemyProjectile
{
    public BaseProjectile3() : base(3, 400, 10, 4, 0)
    {
        nestle.AddCard(new CommonTicker(1, 1));
        nestle.AddCard(new DistanceTicker(400, 1));
    }

    public override Card ToDrag()
    {
        base.ToDrag();
        return new BaseProjectile3();
    }
}

class BaseProjectile4 : NearEnemyProjectile
{
    public BaseProjectile4() : base(10, 200, 80, 10, 800)
    {
        nestle.AddCard(new DistanceTicker(200, 2));
        nestle.AddCard(new DeathTrigger(0.4f));
    }

    public override Card ToDrag()
    {
        base.ToDrag();
        return new BaseProjectile4();
    }
}
class BaseProjectile5 : NearEnemyProjectile
{
    public BaseProjectile5() : base(10, 350, 140, 4, 0)
    {
        nestle.AddCard(new CommonTicker(1, 1.5f));
    }

    public override Card ToDrag()
    {
        base.ToDrag();
        return new BaseProjectile5();
    }
}

class BaseProjectile6 : NearEnemyProjectile
{
    public BaseProjectile6() : base(5, 300, 60, 10, 900)
    {
        nestle.AddCard(new CommonTicker(1, 0.5f));
        nestle.AddCard(new DistanceTicker(400, 0.5f));
    }

    public override Card ToDrag()
    {
        base.ToDrag();
        return new BaseProjectile6();
    }
}


public class RotationAction180 : ActionCard
{
    public RotationAction180(float cost = 10)
    {
        this.cost = cost;

        description.SetDescription("Поворачивает снаряд на <b>180</b> градусов");
        description.SetHeader("Разворот");
    }
    public override void Activate(ProjectileTransform projectile)
    {
        projectile.SetDirection(projectile.direction + 180);
    }

    public override Card ToDrag()
    {
        base.ToDrag();
        return new RotationAction180();
    }
}

public class RotationActionToPlayer : ActionCard
{
    public RotationActionToPlayer(float cost = 10)
    {
        this.cost = cost;

        description.SetDescription("Поворачивает снаряд к игроку");
        description.SetHeader("к игроку");
    }
    public override void Activate(ProjectileTransform projectile)
    {
        Vector2 dir = PlayerTransform.player.pos - projectile.pos;
        float angle = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg;
        projectile.SetDirection(projectile.direction + 180);
    }

    public override Card ToDrag()
    {
        base.ToDrag();
        return new RotationActionToPlayer();
    }
}


public class TeleportActionToPlayer : ActionCard
{
    public TeleportActionToPlayer(float cost = 10)
    {
        this.cost = cost;

        description.SetDescription("Телепортирует к игроку");
        description.SetHeader("в игрока");
    }
    public override void Activate(ProjectileTransform projectile)
    {
        projectile.pos = PlayerTransform.player.pos;
    }

    public override Card ToDrag()
    {
        base.ToDrag();
        return new TeleportActionToPlayer();
    }
}


public abstract class DamageAction : ActionCard
{
    Type enemytype = typeof(EnemyTransform);

    public DamageAction(float cost)
    {
        this.cost = cost;

    }
    public override void Activate(ProjectileTransform projectile)
    {
        foreach (var enemy in projectile.touchedCircles.Select(i => i as EnemyTransform).Where(i => i != null))
        {
            enemy.Damage(GetDamage(projectile));
        }
    }

    protected abstract float GetDamage(ProjectileTransform projectile);
}

class SpeedDamageAction : DamageAction
{
    float cof;
    public SpeedDamageAction(float percent,float cost) : base(cost)
    {
        cof = 1/percent;
    }

    protected override float GetDamage(ProjectileTransform projectile)
    {
        return projectile.speed * cof;
    }
}

public abstract class BaseDamageAction : DamageAction
{
    Type enemytype = typeof(EnemyTransform);
    float damage;
    public BaseDamageAction(float damage,float cost) : base(cost)
    {
        this.damage = damage;

        description.SetDescription($"Наносит <b>{damage}</b> урона каждому противнику, которого касаются");
        description.SetHeader("урон");
    }

    protected override float GetDamage(ProjectileTransform projectile)
    {
        return damage;
    }
}

class BaseDamageAction5 : BaseDamageAction
{
    public BaseDamageAction5() : base(5, 5)
    {
    }

    public override Card ToDrag()
    {
        base.ToDrag();
        return new BaseDamageAction5();
    }
}

class BaseDamageAction15 : BaseDamageAction
{
    public BaseDamageAction15() : base(15, 25)
    {
    }

    public override Card ToDrag()
    {
        base.ToDrag();
        return new BaseDamageAction15();
    }
}

class SpeedDamageAction1 : BaseDamageAction
{
    public SpeedDamageAction1() : base(1, 7)
    {
    }

    public override Card ToDrag()
    {
        base.ToDrag();
        return new SpeedDamageAction1();
    }
}


public class BaseCardCollections : CardCollections
{
    public BaseCardCollections()
    {
        //projectles
        cards.Add((
            new BaseProjectile1(),
            CardRarity.Special
            ));
        cards.Add((
            new BaseProjectile2(),
            CardRarity.Special
            ));
        cards.Add((
            new BaseProjectile3(),
            CardRarity.Special
            ));
        cards.Add((
            new BaseProjectile4(),
            CardRarity.Special
            ));
        cards.Add((
            new BaseProjectile5(),
            CardRarity.Special
            ));
        cards.Add((
            new BaseProjectile6(),
            CardRarity.Special
            ));

        //actions
    }
}