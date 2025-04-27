using UnityEngine;

[CreateAssetMenu()]
public class GeometrySettings : ScriptableObject
{
    public float radius = 1f;
    [Range(0, 6)]
    public int resolution = 3;
}
