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

        // KUNCI JAWABAN: Set baseScale ke 1.0f untuk SEMUA blok.
        // BodyCell.cs sudah mengecilkan gambar Kepala ke 0.8f secara mandiri.
        float baseScale = 1.0f;

        if (isAttached)
        {
            // RUMUS FIX: Gelombang Sinus murni.
            float melar = (Mathf.Sin(Time.time * speed) + 1f) * 0.5f * amount;
            
            // Tembak langsung ke transform.localScale
            transform.localScale = new Vector3(baseScale + melar, baseScale + melar, 1f);
        }
        else
        {
            // Jika statusnya terpotong, matikan jiggle dan diam di ukuran dasar
            transform.localScale = new Vector3(baseScale, baseScale, 1f);
        }
    }
}