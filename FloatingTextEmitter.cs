using UnityEngine;
using TMPro;

public class FloatingTextEmitter : MonoBehaviour
{
    public GameObject damageTextPrefab; // Assign in inspector, should have a TMP Text component
    public float floatSpeed = 2f;
    public float lifeTime = 1f;

    public void Update() {
        if (Input.GetKeyDown(KeyCode.K)) {
            EmitDamageText(25);
        }
    }

    public void EmitDamageText(int damageAmount)
    {
        if (damageTextPrefab == null)
        {
            Debug.LogWarning("DamageTextPrefab is not assigned!");
            return;
        }

        // Instantiate damage text at the object's position
        GameObject damageTextInstance = Instantiate(damageTextPrefab, transform.position, Quaternion.identity);
        TMP_Text textMesh = damageTextInstance.GetComponentInChildren<TMP_Text>();

        if (textMesh != null)
        {
            textMesh.text = damageAmount.ToString();
        }
        else
        {
            Debug.LogWarning("No TMP_Text component found on damageTextPrefab!");
        }

        // Start floating and destroy sequence
        damageTextInstance.AddComponent<DamageTextFloat>().Initialize(floatSpeed, lifeTime);
    }
}

public class DamageTextFloat : MonoBehaviour
{
    private float floatSpeed;
    private TMP_Text textMesh;
    private float lifeTime;
    private float elapsedTime = 0f;
    Camera mainCamera;
    public void Initialize(float floatSpeed, float lifeTime)
    {
        this.floatSpeed = floatSpeed;
        this.lifeTime = lifeTime;
        textMesh = GetComponentInChildren<TMP_Text>();
        Destroy(gameObject, lifeTime);
        mainCamera = Camera.main;

        transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                             mainCamera.transform.rotation * Vector3.up);
    }

    void Update()
    {
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;
        elapsedTime += Time.deltaTime;

        if (mainCamera != null)
        {
            transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                             mainCamera.transform.rotation * Vector3.up);
        }

        float halfLife = lifeTime / 2f;
            if (elapsedTime < halfLife)
            {
                // Fade in
                textMesh.color = new Color(textMesh.color.r, textMesh.color.g, textMesh.color.b, Mathf.Lerp(0f, 1f, elapsedTime / halfLife));
            }
            else
            {
                // Fade out
                textMesh.color = new Color(textMesh.color.r, textMesh.color.g, textMesh.color.b, Mathf.Lerp(1f, 0f, (elapsedTime - halfLife) / halfLife));
            }
    }
}
