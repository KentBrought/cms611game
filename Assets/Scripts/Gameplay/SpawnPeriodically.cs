using Scripts.Helpers;
using UnityEngine;

namespace Scripts.Gameplay
{
    public class SpawnPeriodically : MonoBehaviour
    {
        [Tooltip("rSpikeValue is the number of objects spawned during a spike.")]
        public FrameRateManager.SpikeSpec spikeSpawnPeriod = new FrameRateManager.SpikeSpec()
        {
            rSpikeValue = RangeF.Range00,
            rSpikeDurationInSeconds = RangeF.Range01,
            rSecondsBetweenSpikes = RangeF.Range01,
        };

        public GameObject spawnPrefab;
        [Tooltip(
            "If set, spawn objects inside the collider instead of where the spawnInfo suggests.  Only supports circle and box 2d colliders.  Might be cool to add support for other shapes, or replace the whole spawn idea with a particle system that spawns gameobjects instead of particles.")]
        public Collider2D spawnArea;
        public SpawnInfo spawnInfo;
        //public float minDistanceFromPlayer = 3f;

        protected int numSpawnedDuringThisSpike = 0;

        private void Update()
        {
            
            // Do we need a new spike instance?
            if (null == this.spikeSpawnPeriod.currentSpike)
            {
                // Yes!  Make one.
                this.spikeSpawnPeriod.currentSpike = this.RollNextSpike();
            }

            if (this.spikeSpawnPeriod.currentSpike.sSpikeInterval.max <= Time.time)
            {
                // Spike is in the past.  Spawn all remaining objects.
                this.Spawn(this.spikeSpawnPeriod.currentSpike.spikeValue - this.numSpawnedDuringThisSpike);
                this.spikeSpawnPeriod.currentSpike = this.RollNextSpike();
                return;
            }
            
            // Spike has a duration.  Spawn evenly throughout it.
            if (Time.time > this.spikeSpawnPeriod.currentSpike.sSpikeInterval.min)
            {
                var spikeDuration = this.spikeSpawnPeriod.currentSpike.sSpikeInterval.Width;
                if (spikeDuration <= 0)
                {
                    Debug.LogError("Spike duration is zero, but time is past min time AND less than max time.  This is a bug.");
                    this.spikeSpawnPeriod.currentSpike = this.RollNextSpike();
                    return;
                }
                
                var fractionOfSpikeElapsed = (Time.time - this.spikeSpawnPeriod.currentSpike.sSpikeInterval.min) / spikeDuration;
                var expectedSpawns = this.spikeSpawnPeriod.currentSpike.spikeValue * fractionOfSpikeElapsed;
                this.Spawn(expectedSpawns - this.numSpawnedDuringThisSpike);
            }
        }

        private void Spawn(float numToSpawn)
        {
            if (numToSpawn > 100)
            {
                Debug.LogError($"Spawning too many objects at once ({numToSpawn}).  This is probably a bug.");
                return;
            }
            
            for (var i = 0; i < numToSpawn; i++)
            {
                var spawn = this.spawnInfo.Spawn(this.transform, this.spawnPrefab.transform);

                if (this.spawnArea)
                {
                    spawn.position = GeneralHelpers.GetRandomPointInside(this.spawnArea);
                }
                ++this.numSpawnedDuringThisSpike;
            }
        }

        private FrameRateManager.SpikeInstance RollNextSpike()
        {
            this.numSpawnedDuringThisSpike = 0;
            return this.spikeSpawnPeriod.RollAndSetSpike();
        }
    }
}