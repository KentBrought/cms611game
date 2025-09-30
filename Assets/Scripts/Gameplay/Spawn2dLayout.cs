using UnityEngine;

namespace Scripts.Gameplay
{
    public class Spawn2dLayout : MonoBehaviour
    {
        public GameObject spawnPrefab;
        public Camera pointOfView;
        public Vector2 offset;
        public int numRows = 10;
        public int numColumns = 10;
        
        void Start()
        {
            SpawnAll();
        }

        public void DespawnAll()
        {
            int numChildren = transform.childCount;
            for (int i = 0; i < numChildren; i++)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }
        
        public void SpawnAll()
        {
            if (!this.isActiveAndEnabled)
            {
                return;  // if this script is disabled, don't spawn anything
                
                // How can this happen???
                // Well, it can happen if the script is disabled but the GameManager is enabled and sending onRestart events.
            }
            
            if (!this.spawnPrefab)
            {
                Debug.LogWarning("Spawn2dLayout: spawnPrefab is null");
                return;
            }

            if (!this.pointOfView)
            {
                Debug.LogWarning("Spawn2dLayout: pointOfView is null");
                return;
            }

            if (this.numRows < 1)
            {
                Debug.LogWarning("Spawn2dLayout: numRows is less than 1");
                return;
            }

            if (this.numColumns < 1)
            {
                Debug.LogWarning("Spawn2dLayout: numColumns is less than 1");
                return;
            }

            var roughWidth = this.offset.x * (this.numColumns - 1);
            var roughHeight = this.offset.y * (this.numRows - 1);

            var center = this.transform.position;
            var xMin = center.x - (roughWidth / 2);
            var yMin = center.y - (roughHeight / 2);

            for (var row = 0; row < this.numRows; row++)
            {
                for (var column = 0; column < this.numColumns; column++)
                {
                    var x = xMin + (this.offset.x * column);
                    var y = yMin + (this.offset.y * row);
                    var position = new Vector3(x, y, 0);
                    var rotation = Quaternion.identity;
                    var newSpawn = Instantiate(this.spawnPrefab, position, rotation);
                    newSpawn.transform.SetParent(this.transform);
                }
            }

        }

        public void RespawnAll()
        {
            this.DespawnAll();
            this.SpawnAll();
        }
    }
}