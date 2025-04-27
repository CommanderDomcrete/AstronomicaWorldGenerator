using UnityEngine;

[CreateAssetMenu()]
public class BiomeSettings : ScriptableObject
{
    public Material planetMaterial;
    public BiomeColourSettings biomeColourSettings;


    [System.Serializable]
    public class BiomeColourSettings {

        public Biome[] biomes;
        //public NoiseSettings noise;
        public float noiseOffset;
        public float noiseStrength; 

        [System.Serializable]
        public class Biome {
            public Gradient gradient;
            public Color tint;
            [Range(0, 1)]
            public float startHeight;
            [Range(0, 1)]
            public float tintPercent;
        }
    }
}
