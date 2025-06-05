using UnityEngine;
using System.Collections.Generic;

public class SpellProjectile : MonoBehaviour
{
    [Header("Auto-Filled by Spell")]
    public Spell sourceSpell;
    public CharacterStats caster;

    [Header("Projectile Settings")]
    public float speed = 40000f;
    public float lifetime = 5f;
    public GameObject impactEffect;

    [Header("Runtime Data")]
    public float damage;
    public DamageType damageType;
    public LayerMask targetLayers;
    public List<StatusEffect> statusEffects = new List<StatusEffect>();
    public float aoeRadius;

    public Vector3 direction;

    private Rigidbody rb;

    public GameObject trail;

    void Start()
        {
            targetLayers = LayerMask.GetMask("NPC");

            rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = false;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            rb.mass = 1f;
            rb.linearDamping = 0f;
            rb.angularDamping = 0f;


            rb.linearVelocity = direction.normalized * speed;

            Destroy(gameObject, lifetime);

        }

    void Update()
{
    if (rb != null)
        Debug.Log("Current velocity: " + rb.linearVelocity);
}

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Spell")) return;

        Transform root = other.transform.root;

        // Check targeting layer
        if ((targetLayers.value & (1 << other.gameObject.layer)) == 0)
            return;

        // Find the NPC (or other damageable) on root or in children
        NPC npc = root.GetComponentInChildren<NPC>();
        if (npc != null)
        {
            Debug.Log($"Spell '{sourceSpell.spellName}' hit NPC: {npc.name}");

            var attackData = new AttackData
            {
                damage = damage,
                damageTypes = new List<DamageType> { damageType },
                statusEffects = statusEffects,
                knockbackForce = Vector3.zero,
                source = caster,
            };

            npc.TakeAttack(attackData);
        }

        if (impactEffect != null)
        {
            Instantiate(impactEffect, transform.position, Quaternion.identity);
        }

        if (sourceSpell.spellCommands.Count > 0)
        {
            sourceSpell.ExecuteCommandsOn(npc.gameObject);
        }

        if (trail != null) {
            trail.transform.parent = null;
            trail.GetComponent<ParticleSystem>().Stop();
            Destroy(trail, 2f);
        }
        Destroy(gameObject);
    }

    public void ApplyHandParticleColor(Transform handParticleRoot)
{
    ParticleSystem sourcePS = handParticleRoot.GetComponentInChildren<ParticleSystem>();
    if (sourcePS == null) return;

    // Get the main module to extract the start color
    Color sourceColor = sourcePS.main.startColor.color;

    ParticleSystem[] projectileParticles = GetComponentsInChildren<ParticleSystem>(true);
    foreach (var ps in projectileParticles)
    {
        var main = ps.main;
        main.startColor = sourceColor;
    }
}

}
