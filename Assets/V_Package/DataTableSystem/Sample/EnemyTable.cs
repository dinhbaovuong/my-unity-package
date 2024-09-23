using UnityEngine;
using VPackage.DataTableSystem.SoTableSystem;

namespace DataTableSystem.Sample
{
    [CreateAssetMenu(menuName = "Data Table/Enemy")]
    public class EnemyTable : SoTableOneId<EnemyTable, EnemyRow, string>
    {
        protected override string InstancePath => "Database/Enemy Table";
        protected override string GetRowId(EnemyRow row)
        {
            return row.id;
        }
    }
    
    
    public enum EnemyType
    {
        Monster, Boss
    }
}