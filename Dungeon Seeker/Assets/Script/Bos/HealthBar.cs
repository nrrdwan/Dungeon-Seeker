using UnityEngine;

public class HealthBar : MonoBehaviour
{
    public Transform target;             // Target bos yang diikuti
    public SpriteRenderer fillSprite;    // Sprite merah (isi darah)
    public Vector3 offset = new Vector3(0f, 2f, 0f);

    private float fullScaleX;            // Ukuran X penuh dari bar darah

    void Start()
    {
        if (fillSprite != null)
            fullScaleX = fillSprite.transform.localScale.x;
    }

    public void SetHealth(float current, float max)
    {
        float persen = Mathf.Clamp01(current / max);

        if (fillSprite != null)
        {
            Vector3 scale = fillSprite.transform.localScale;
            scale.x = fullScaleX * persen;
            fillSprite.transform.localScale = scale;
        }
    }

    void LateUpdate()
    {
        if (target != null)
        {
            transform.position = target.position + offset;
        }
    }
}
