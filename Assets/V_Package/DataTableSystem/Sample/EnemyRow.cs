using System.Collections.Generic;
using UnityEngine;

namespace DataTableSystem.Sample
{
    [System.Serializable]
    public class EnemyRow
    {
        public string id;
        public EnemyType type;
        public int powerLevel;
        public Range range;
        public List<string> listReward;
    }

    [System.Serializable]
    public class Range
    {
        public float min;
        public float max;
    }

}