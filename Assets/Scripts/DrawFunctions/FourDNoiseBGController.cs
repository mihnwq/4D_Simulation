using UnityEngine;
using UnityEngine.UI;

public class FourDNoiseBGController : MonoBehaviour
{
    public RawImage targetImage;
    public float scrollSpeed = 0.3f;

    Material mat;

    void Start()
    {
        mat = Instantiate(targetImage.material);
        targetImage.material = mat;
    }

    void Update()
    {
        float w = Time.time * scrollSpeed;
        mat.SetFloat("_ScrollW", w);
    }
}
