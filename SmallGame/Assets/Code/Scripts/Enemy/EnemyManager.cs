using UnityEngine;
using System.Collections.Generic;

public static class EnemyManager {

    public static List<GameObject> enemyList = new List<GameObject>(); //the total list of enemies in the current level
    public static List<GameObject> enemiesAttackingPlayer = new List<GameObject>(); //a list of enemies that are currently attacking
    public static List<GameObject> activeEnemies = new List<GameObject>(); //a list of enemies that are currently active

    //移除敌人， 在敌人列表中 
    public static void RemoveEnemyFromList(GameObject g) {
        enemyList.Remove(g);
        SetEnemyTactics();
    }


    //设置当前敌人的状态
    public static void SetEnemyTactics() {
        getActiveEnemies();
        if (activeEnemies.Count > 0) {
            for (int i = 0; i < activeEnemies.Count; i++) {
                if (i < MaxEnemyAttacking()) {
                    activeEnemies[i].GetComponent<EnemyAI>().SetEnemyTactic(ENEMYTACTIC.ENGAGE);
                } else {
                    activeEnemies[i].GetComponent<EnemyAI>().SetEnemyTactic(ENEMYTACTIC.KEEPMEDIUMDISTANCE);
                }
            }
        }
    }

    //Force all enemies to use a certain tactic
    //public static void ForceEnemyTactic(ENEMYTACTIC tactic) {
    //    getActiveEnemies();
    //    if (activeEnemies.Count > 0) {
    //        for (int i = 0; i < activeEnemies.Count; i++) {
    //            activeEnemies[i].GetComponent<EnemyAI>().SetEnemyTactic(tactic);
    //        }
    //    }
    //}

    /// <summary>
    /// 移除所有敌人的AI
    /// </summary>
    public static void DisableAllEnemyAIs() {
        getActiveEnemies();
        if (activeEnemies.Count > 0) {
            for (int i = 0; i < activeEnemies.Count; i++) {
                activeEnemies[i].GetComponent<EnemyAI>().enableAI = false;
            }
        }
    }
    /// <summary>
    /// 返回当前活跃敌人的列表
    /// </summary>
    public static void getActiveEnemies() {
        activeEnemies.Clear();
        foreach (GameObject enemy in enemyList) {
            if (enemy != null && enemy.activeSelf) activeEnemies.Add(enemy);
        }
    }

    //玩家死亡
    public static void PlayerHasDied() {
        DisableAllEnemyAIs();
        enemyList.Clear();
    }

    /// <summary>
    /// 返回当前能够攻击到玩家的最大数量
    /// </summary>
    /// <returns></returns>
    static int MaxEnemyAttacking() {
        EnemyWaveSystem EWS = GameObject.FindObjectOfType<EnemyWaveSystem>();
        if (EWS != null) return EWS.MaxAttackers;
        return 3;
    }

    /// <summary>
    /// 设置一个敌人的攻击状态
    /// </summary>
    /// <param name="enemy"></param>
    public static void setAgressive(GameObject enemy) {
        enemy.GetComponent<EnemyAI>().SetEnemyTactic(ENEMYTACTIC.ENGAGE);

        //让其他敌人处于被动状态
        foreach (GameObject e in activeEnemies) {
            if (e != enemy) {
                e.GetComponent<EnemyAI>().SetEnemyTactic(ENEMYTACTIC.KEEPMEDIUMDISTANCE);
                return;
            }
        }
    }
}
