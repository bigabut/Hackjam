using UnityEngine;

public class JellyJiggle : MonoBehaviour
{
    [Header("Jiggle Settings")]
    public float speed = 5f;
    public float amount = 0.15f; 

    private BodyCell cell;

    void Start()
    {
        cell = GetComponent<BodyCell>();
    }

    void Update()
    {
        if (cell == null) return;
        
        // Cek apakah blok ini sedang menjadi anak dari GridBodyMovement
        bool isAttached = transform.parent != null && transform.parent.GetComponent<GridBodyMovement>() != null;

        // Pertahankan hierarki skala: Kepala = 0.8, Tubuh = 1.0
        float baseScale = cell.IsHead ? 0.8f : 1.0f;

        if (isAttached)
        {
            // RUMUS FIX: Gelombang Sinus murni.
            // (Sin + 1) * 0.5 membuat nilainya mengayun sangat mulus dari 0 ke 1.
            // Hasilnya: Jeli hanya membesar secara kenyal dan tidak bergetar kaku!
            float melar = (Mathf.Sin(Time.time * speed) + 1f) * 0.5f * amount;
            
            // Tembak langsung ke transform.localScale seperti kodemu
            transform.localScale = new Vector3(baseScale + melar, baseScale + melar, 1f);
        }
        else
        {
            // Jika statusnya terpotong, matikan jiggle dan diam di ukuran dasar
            transform.localScale = new Vector3(baseScale, baseScale, 1f);
        }
    }
}