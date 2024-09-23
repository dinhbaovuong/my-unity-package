using UnityEngine;

namespace DataTableSystem.Sample
{
    public class DataTableSystemTest : MonoBehaviour
    {
        private void Start()
        {
            var row = EnemyTable.GetRowById("enemy_1");
            Debug.Log(row.type);
            EnemyRow r = new EnemyRow();
            r.id = "enemy_3";
            //r.type = "Type 3";
            EnemyTable.PatchTable_AddRow(r);
            Debug.Log(EnemyTable.GetRowById("enemy_3").type);
        }
    }
}