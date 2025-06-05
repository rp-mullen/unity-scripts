using Assets.Scripts.Systems.Player;
using System;
using System.Collections.Generic;
using UnityEngine;



public class OBJ : MonoBehaviour
{
    public HealthComponent healthComponent;

    public Action<DamageResult> OnDamageTaken;
    public Action<string> OnInteractionNameChanged;
    public Action ResetSignal;

    public ObjMaterial objMaterial;
    public ObjMaterialSubtype objMaterialSubtype = ObjMaterialSubtype.None;

    public string objectName;

    public string interactionPrompt = "Interact";
    public OBJInteractType interactType;

    public bool isPlayerInRange = true;

    public bool isInteractable = false;

    private bool interacting;
    public List<DamageType> resistances;

    public Item associatedItem;
    public bool waitForSignal = false;

    public void Start()
    {
        ResetSignal += Reset;
    }

    public virtual void Interact(CharacterStats characterStats)
    {
        if (!interacting)
        {
            interacting = true;
        }
        else
        {
            if (!waitForSignal)

            {
                Reset();
            }
            else
            {
                return;
            }

        }

        switch (interactType)
        {
            case OBJInteractType.Read:
                // used for things like readables and books
                if (associatedItem != null)
                {
                    associatedItem.Use(characterStats);
                }
                break;

            case OBJInteractType.PickUp:
                Global.AddItem(associatedItem.itemName);
                Destroy(gameObject);
                break;
            case OBJInteractType.Mount:
                var mountController = GetComponent<MountController>();
                if (!mountController.isMounted)
                {
                    mountController?.Mount(characterStats);
                }
                else
                {
                    mountController?.Dismount(characterStats);
                }

                break;
        }

    }

    public virtual void Reset()
    {
        interacting = false;
        Debug.Log($"[OBJ] Reset interaction with {name}");
        if (associatedItem != null)
        {
            associatedItem.Reset();
        }
    }

    public bool IsInteracting()
    {
        return interacting;
    }

    public void TakeAttack(AttackData attack)
    {

        foreach (DamageType damageType in attack.damageTypes)
        {
            if (resistances != null)
            {
                if (resistances.Contains(damageType))
                {
                    attack.source.owner.GetComponent<Animator>().SetTrigger("StrikeRecoil");
                    if (objMaterial == ObjMaterial.Stone)
                    {
                        EmitVisualEffect(ObjVisualEffect.Sparks);
                    }
                }
            }
        }

        if (!healthComponent) return;

        float pre = healthComponent.CurrentValue;
        healthComponent.TakeDamage(attack.damage);
        float post = healthComponent.CurrentValue;

        OnDamageTaken?.Invoke(new DamageResult
        {
            damageTaken = pre - post,
            wasLethal = post <= 0f,
            sourceAttack = attack
        });
    }

    public void EmitVisualEffect(ObjVisualEffect visual)
    {
        switch (visual)
        {
            case ObjVisualEffect.Sparks:
                GameObject sparks = Instantiate((Resources.Load("FX/Sparks") as GameObject));
                sparks.transform.position = transform.position;
                var player = GameObject.FindFirstObjectByType<Player>();
                if (player != null)
                {
                    sparks.transform.LookAt(player.transform.position);
                    sparks.transform.position = new Vector3(sparks.transform.position.x, player.transform.position.y + 2.0f, sparks.transform.position.z);
                }
                break;
        }
    }


}
