using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PowerlineColor : MonoBehaviour {
    public Gradient gradient;
    Vector3 startPosition;
    Quaternion startRotation;
    public bool invertDirection = false;
    ParticleSystem ps;
   // public float powerRating = 500;

    // Use this for initialization
    void Start () {
        ps = GetComponent<ParticleSystem>();
        startPosition = transform.position;
        startRotation = transform.rotation;
    }
	

    void SetPower(float value)
    {
        float fraction = (0.5f / ps.main.startLifetimeMultiplier);
        Gradient grad = new Gradient();
        grad.SetKeys(new GradientColorKey[] { new GradientColorKey(gradient.Evaluate(value), 0.0f) }, new GradientAlphaKey[] { new GradientAlphaKey(0,0), new GradientAlphaKey(1, fraction), new GradientAlphaKey(1, 1-fraction), new GradientAlphaKey(0, 1) });
        var col = ps.colorOverLifetime;
        col.enabled = true;
        col.color = grad;
        Vector3 scale = Vector3.one;
        scale *= Mathf.Lerp(0.5f, 1, value);
        scale.z = 1;
        transform.localScale = scale;
    }

    public void SetViz(bool forward, float power)
    {
       // power /= powerRating;
        if (invertDirection)
        {
            forward = !forward;
        }
        if (forward)
        {
            transform.position = startPosition;
            transform.rotation = startRotation;
        }
        else
        {
            transform.position = startPosition + (startRotation * (Vector3.forward * ps.main.startSpeedMultiplier * ps.main.startLifetimeMultiplier));
            transform.rotation = startRotation * Quaternion.Euler(0, 180, 0);
        }
        SetPower(power);

    }
}
