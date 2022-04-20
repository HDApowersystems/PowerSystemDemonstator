using DG.Tweening;
using Network;
using System;
using UnityEngine;

namespace PowerNetwork.View
{
    public class DcLineView : MonoBehaviour
    {
        public DcLine dcLine;
        public BusView fromNode;
        public BusView toNode;
        public DcLineResult dcLineResult;

        public Action<DcLine> OnDClineChanged;
        public Action<DcLine> OnDcLineActive;

        private float pf;
        private float qf;
        private float pt;
        private float qt;

        public Gradient gradient;
        Vector3 startPosition;
        Quaternion startRotation;

        public bool invertDirection = false;
        ParticleSystem ps;
        public float powerRating = 0.96f;

        //public Arrow ArrowPrefab;
        // public float SpawnArrowFrequency = 1;
        private Vector3 _initialScale;


        private void Awake()
        {
            dcLine.from_bus = fromNode.Bus.index;
            dcLine.to_bus = toNode.Bus.index;


            ps = GetComponent<ParticleSystem>();
            ps.gameObject.SetActive(true);
            startPosition = transform.position;
            startRotation = transform.rotation;

            dcLineResult.OnDcLinePlChanged = OnPlChanged;
            dcLineResult.OnDcLinePfromChanged = OnDcLinePfromChanged;
            dcLineResult.OnDcLineQfromChanged = OnDcLineQfromChanged;
            dcLineResult.OnDcLinePtoChanged = OnDcLinePtoChanged;
            dcLineResult.OnDcLineQtoChanged = OnDcLineQtoChanged;
            CalculatePower(pf, qf, pt, qt);
        }

        #region Subscribers
        private float OnDcLineQtoChanged(float arg)
        {
            qt = arg;
            return qt;
        }

        private float OnDcLinePtoChanged(float arg)
        {
            pt = arg;
            return pt;
        }

        private float OnDcLineQfromChanged(float arg)
        {
            qf = arg;
            return qf;
        }

        private float OnDcLinePfromChanged(float arg)
        {
            pf = arg;
            return pf;
        }

        private void OnPlChanged(float pl)
        {
            if (pl == 0)
            {
                ps.enableEmission = false;
            }
            else
            { ps.enableEmission = true; }

            CalculatePower(pf, qf, pt, qt);
        }
        #endregion
        private void CalculatePower(float pFrom, float qFrom, float pTo, float qTo)
        {
            bool forward = Mathf.Sign((float)pFrom) == 1;
            float power = Mathf.Max(Mathf.Sqrt(Mathf.Pow((float)pFrom, 2) + Mathf.Pow((float)qFrom, 2)), Mathf.Sqrt(Mathf.Pow((float)pTo, 2) + Mathf.Pow((float)qTo, 2)));
            SetViz(forward, power);
        }
        void SetPower(float value)
        {
            float fraction = (0.5f / ps.main.startLifetimeMultiplier);

            Gradient grad = new Gradient();

            grad.SetKeys(new GradientColorKey[] { new GradientColorKey(gradient.Evaluate(value), 0.0f) }, new GradientAlphaKey[] { new GradientAlphaKey(0, 0), new GradientAlphaKey(1, fraction), new GradientAlphaKey(1, 1 - fraction), new GradientAlphaKey(0, 1) });
            
            var col = ps.colorOverLifetime;

            col.enabled = true;
            col.color = grad;

            Vector3 scale = _initialScale;
            value *= Mathf.Lerp(0.5f, 1, value);
            scale.x = value / 30;
            scale.y = value / 30;
            scale.z = 5;
            transform.localScale = scale;
        }

        private void SetViz(bool forward, float power)
        {
            power /= 1;

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
                transform.position = startPosition + (startRotation * (Vector3.forward * 5 * ps.main.startSpeedMultiplier * ps.main.startLifetimeMultiplier));
                transform.rotation = startRotation * Quaternion.Euler(0, 180, 0);
            }
            SetPower(power);
        }

        public void NotifyChange()
        {
            OnDClineChanged?.Invoke(dcLine);
        }
    }

}