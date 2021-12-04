using System.Collections.Generic;
using UnityEngine;

namespace MainGame
{
    public class ObjectPooler : MonoBehaviour
    {
        [Header("Settings")]
        public int amountToPool = 10;
        [SerializeField] List<GameObject> attackerSoldiersPool;
        [SerializeField] List<GameObject> defenderSoldiersPool;

        [Header("Prefabs")]
        public GameObject attackerSoldier;
        public GameObject defenderSoldier;

        // Start is called before the first frame update
        void Start()
        {
            InitializePool(attackerSoldier, ref attackerSoldiersPool);
            InitializePool(defenderSoldier, ref defenderSoldiersPool);
        }

        private void InitializePool(GameObject prefab, ref List<GameObject> pool)
        {
            pool = new List<GameObject>();

            for (int i = 0; i < amountToPool; i++)
            {
                GameObject obj = Instantiate(prefab);
                obj.name = $"{prefab.name}_pooled_{i}";
                obj.SetActive(false);
                pool.Add(obj);
            }
        }

        public GameObject GetAttackerSoldier()
        {
            for (int i = 0; i < attackerSoldiersPool.Count; i++)
            {
                if (!attackerSoldiersPool[i].activeInHierarchy)
                    return attackerSoldiersPool[i];
            }
            return null;
        }

        public GameObject GetDefenderSoldier()
        {
            for (int i = 0; i < defenderSoldiersPool.Count; i++)
            {
                if (!defenderSoldiersPool[i].activeInHierarchy)
                    return defenderSoldiersPool[i];
            }
            return null;
        }

        public void ResetAllSoldier()
        {
            foreach(GameObject go in attackerSoldiersPool)
            {
                go.SetActive(false);
                go.transform.SetParent(null);
            }

            foreach (GameObject go in defenderSoldiersPool)
            {
                go.SetActive(false);
                go.transform.SetParent(null);
            }
        }
    }
}
