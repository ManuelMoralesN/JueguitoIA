// BossEnemy.cs - Mejoras y optimización
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossEnemy : BaseEnemy
{
    [Header("Rangos")]
    public float meleeRange = 5f;
    public float rangedRange = 10f;

    [Header("Cooldowns de Ataque")]
    public float basicAttackCooldown = 1f;
    public float areaAttackCooldown = 3f;
    public float dashAttackCooldown = 2f;
    public float ultimateAttackCooldown = 10f;

    private float nextAttackTime;
    private List<string> recentSpecialAttacks = new List<string>();

    private EnemyFSM fsm;
    private Transform player;
    private bool isGameStarted = false;
    private bool ultimateUsed = false;

    public override void Awake()
    {
        base.Awake();
        fsm = GetComponent<EnemyFSM>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        StartCoroutine(StartDelay());
    }

    private IEnumerator StartDelay()
    {
        yield return new WaitForSeconds(3f); // Retraso para evitar ejecuciones prematuras
        isGameStarted = true;
    }

    void Update()
    {
        if (!isGameStarted || player == null) return;

        HandleStateTransitions();
    }

    private void HandleStateTransitions()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool isInMeleeRange = distanceToPlayer <= meleeRange;
        bool isInRangedRange = distanceToPlayer > meleeRange && distanceToPlayer <= rangedRange;

        if (isInMeleeRange && !(fsm.CurrentState is MeleeState))
        {
            fsm.ChangeState(fsm.MeleeState);
        }
        else if (isInRangedRange && !(fsm.CurrentState is RangeState))
        {
            fsm.ChangeState(fsm.RangeState);
        }
    }

    public void ExecuteNextAttack()
    {
        if (Time.time < nextAttackTime) return;

        if (!ultimateUsed && CanExecuteUltimateMeleeAttack())
        {
            ExecuteUltimateMeleeAttack();
        }
        else if (!ultimateUsed && CanExecuteUltimateRangedAttack())
        {
            ExecuteUltimateRangedAttack();
        }
        else
        {
            ExecuteAttack(DetermineNextAttack());
        }
    }

    private string DetermineNextAttack()
    {
        if (recentSpecialAttacks.Count == 0 || recentSpecialAttacks[^1] == "Dash")
        {
            return "Basic";
        }
        else if (recentSpecialAttacks[^1] == "Basic")
        {
            return "Area";
        }
        return "Dash";
    }

    public void ExecuteAttack(string attackType)
    {
        switch (attackType)
        {
            case "Basic":
                StartCoroutine(ExecuteBasicAttack());
                break;
            case "Area":
                StartCoroutine(ExecuteAreaAttack());
                break;
            case "Dash":
                StartCoroutine(ExecuteDashAttack());
                break;
        }
    }

    public IEnumerator ExecuteBasicAttack()
    {
        Debug.Log("Ejecutando ataque básico.");
        yield return new WaitForSeconds(basicAttackCooldown);
        nextAttackTime = Time.time + basicAttackCooldown;
    }


    public IEnumerator ExecuteAreaAttack()
    {
        Debug.Log("Ejecutando ataque de área.");
        yield return new WaitForSeconds(areaAttackCooldown);
        recentSpecialAttacks.Add("Area");
        nextAttackTime = Time.time + areaAttackCooldown;
    }

    public IEnumerator ExecuteDashAttack()
    {
        Debug.Log("Ejecutando ataque de dash.");
        yield return new WaitForSeconds(dashAttackCooldown);
        recentSpecialAttacks.Add("Dash");
        nextAttackTime = Time.time + dashAttackCooldown;
    }

    public override void ExecuteUltimateAttack(bool isRanged)
    {
        Debug.Log(isRanged ? "Ejecutando ataque ultimate a distancia." : "Ejecutando ataque ultimate melee.");
        nextAttackTime = Time.time + ultimateAttackCooldown;
        recentSpecialAttacks.Clear();
        ultimateUsed = true; // Evitar uso repetido
    }

    public void ExecuteUltimateMeleeAttack()
    {
        Debug.Log("Ejecutando ataque ultimate melee.");
        ExecuteUltimateAttack(false);
    }

    public void ExecuteUltimateRangedAttack()
    {
        Debug.Log("Ejecutando ataque ultimate a distancia.");
        ExecuteUltimateAttack(true);
    }

    public bool CanExecuteUltimateMeleeAttack()
    {
        return isGameStarted && !ultimateUsed && health < 50 && recentSpecialAttacks.Count >= 2 &&
               (recentSpecialAttacks[^1] == "Dash" || recentSpecialAttacks[^1] == "Area");
    }

    public bool CanExecuteUltimateRangedAttack()
    {
        return isGameStarted && !ultimateUsed && health < 50 && recentSpecialAttacks.Count >= 3;
    }

    public bool IsPlayerInMeleeRange()
    {
        return Vector3.Distance(transform.position, player.position) <= meleeRange;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, rangedRange);
    }
}