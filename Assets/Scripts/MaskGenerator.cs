using UnityEngine;

public class MaskGenerator
{
    BiomeSettings settings;
    Texture2D texture;
    const int textureResolution = 50;
    NoiseFilter noiseFilter;

    public void UpdateSettings(BiomeSettings settings) {
        this.settings = settings;
        if(texture == null || texture.height != settings.biomeColourSettings.biomes.Length) {
            texture = new Texture2D(textureResolution, settings.biomeColourSettings.biomes.Length);
        }
        
    }

    public float BiomePercentFromPoint(Vector3 pointOnUnitSphere, float radius) {
        noiseFilter = new NoiseFilter();
        float heightPercent = (pointOnUnitSphere.y + radius) / (2 * radius); // Convert the y coordinate to a percentage between 0 and 1
        heightPercent += (noiseFilter.Evaluate(pointOnUnitSphere)-settings.biomeColourSettings.noiseOffset) * settings.biomeColourSettings.noiseStrength;
        //Debug.Log(noiseFilter.Evaluate(pointOnUnitSphere));// - settings.biomeColourSettings.noiseOffset) * settings.biomeColourSettings.noiseStrength);
        float biomeIndex = 0;
        int numBiomes = settings.biomeColourSettings.biomes.Length;

        for (int i = 0; i < numBiomes; i++) {
            if (settings.biomeColourSettings.biomes[i].startHeight < heightPercent) {
                biomeIndex = i;
            }
            else {
                break;
            }
        }

        return biomeIndex / Mathf.Max(1, numBiomes - 1); // Return the biome index as a percentage
    }

    public void UpdateMask() {
        // Generate the mask texture based on the biome settings
        Color[] colours = new Color[texture.width * texture.height];
        int colourIndex = 0;
        foreach (var biome in settings.biomeColourSettings.biomes) {

            for (int i = 0; i < textureResolution; i++) {
                //Do something

                Color gradientCol = biome.gradient.Evaluate(i / (textureResolution - 1f));
                Color tintCol = biome.tint;
                colours[colourIndex] = gradientCol * (1 - biome.tintPercent) + tintCol * biome.tintPercent;

                colourIndex++;
            }
        }
        
        texture.SetPixels(colours);
        texture.Apply();
        settings.planetMaterial.SetTexture("_texture", texture);
    }
}
