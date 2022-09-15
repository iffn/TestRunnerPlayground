using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace iffnsStuff.iffnsPhysics.PredictUnityPhysics
{
    public class VelocityPredictor
    {
        public static float PredictVelocityDueToDragOnly(float initialVelocity, float drag, float time)
        {
            return initialVelocity / Mathf.Exp(drag * time);
        }
    }
}