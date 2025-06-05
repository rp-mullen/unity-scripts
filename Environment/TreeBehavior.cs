using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Systems.Environment
{
    class TreeBehavior : MonoBehaviour
    {

        public OBJ obj;
        public GameObject Trunk;

        public Transform forcePoint;

        public void Start()
        {
            obj.OnDamageTaken += OnHealthDepleted;

            var rb = GetComponent<Rigidbody>();
            if (rb.isKinematic) 
            { 
                rb.isKinematic = true;    
            }

            forcePoint.gameObject.GetComponent<ForcePoint>().OnHit += OnForcePointHit;
        }

        public void OnHealthDepleted(DamageResult damageResult)
        {
            if (damageResult.wasLethal)
            {
                forcePoint.gameObject.GetComponent<ForcePoint>().activated = true;
                TriggerRigidbody();
            }
        }

        public void TriggerRigidbody()
        {
            var rb = Trunk.GetComponent<Rigidbody>();
            rb.isKinematic = false;

            Transform player = GameObject.FindWithTag("Player").transform;

            Vector3 fallForce = 10000f * (Trunk.transform.position - player.position).normalized;
            Vector3 torqueArm = forcePoint.position - Trunk.transform.position;

            Vector3 torque = Vector3.Cross(torqueArm, fallForce) * rb.mass;

            rb.AddTorque(torque, ForceMode.Impulse);
        }

        public void OnForcePointHit()
        {
            obj.OnDamageTaken -= OnHealthDepleted;
            Destroy(Trunk.GetComponent<HingeJoint>());
        }

    }
}
