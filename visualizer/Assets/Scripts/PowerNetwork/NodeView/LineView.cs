using DG.Tweening;
using Network;
using System;
using UnityEngine;
namespace PowerNetwork.View
{
    public class LineView : MonoBehaviour
    {
        public Line Line;
        public BusView FromNode;
        public BusView ToNode;
        public LineResult LineResult;

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

        private Vector3 _initialScale;
        private float _elapsed;

        public Action<Line> OnAClineChanged;

        public Arrow ArrowPrefab;
        public float SpawnArrowFrequency = 16f;
      

        private void Awake()
        {
            Line.from_bus = FromNode.Bus.index;
            Line.to_bus = ToNode.Bus.index;

            ps = GetComponent<ParticleSystem>();
           // ps.gameObject.SetActive(true);
            startPosition = transform.position;
            startRotation = transform.rotation;
            _initialScale = transform.localScale;

            LineResult.OnLineLoadingChanged = OnLineLoadingChanged;
            LineResult.OnLinePfromChanged = OnLinePfromChanged;
            LineResult.OnLineQfromChanged = OnLineQfromChanged;
            LineResult.OnLinePtoChanged = OnLinePtoChanged;
            LineResult.OnLineQtoChanged = OnLineQtoChanged;

            CalculatePower(pf, qf, pt, qt);

            // _initialScale = transform.localScale;
        }
        #region subscriber
        private float OnLineQtoChanged(float arg)
        {
            qt = arg;
            return qt;
        }

        private float OnLinePtoChanged(float arg)
        {
            pt = arg;
            return pt;
        }

        private float OnLineQfromChanged(float arg)
        {
            qf = arg;
            return qf;
        }

        private float OnLinePfromChanged(float arg)
        {
            pf = arg;
            return pf;
        }

        private void OnLineLoadingChanged(float loadingper)
        {
            if (loadingper == 0)
            {
                ps.enableEmission = false;
            }
            else
            { ps.enableEmission = true; }

            CalculatePower(pf, qf, pt, qt);
            SetPower(loadingper);

        }
        #endregion
        private void CalculatePower(float pFrom, float qFrom, float pTo, float qTo)
        {
           //SpawnArrow();
            bool forward = Mathf.Sign((float)pFrom) == 1;
            float power = Mathf.Max(Mathf.Sqrt(Mathf.Pow((float)pFrom, 2) + Mathf.Pow((float)qFrom, 2)), Mathf.Sqrt(Mathf.Pow((float)pTo, 2) + Mathf.Pow((float)qTo, 2)));
            //  Debug.Log(gameObject.name + " - " + (forward ? "Forward" : "Backward"));
            SetViz(forward);          
        }


        private void SetViz(bool forward)
        {

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
                transform.position = startPosition + (startRotation * (Vector3.forward *5 * ps.main.startSpeedMultiplier * ps.main.startLifetimeMultiplier));
                transform.rotation = startRotation * Quaternion.Euler(0, 180, 0);
            }
         
        }

        void SetPower(float value)
        {
            float fraction = (0.5f / ps.main.startLifetimeMultiplier);
    
            var val = value / 100;
            Gradient grad = new Gradient();
            grad.SetKeys(new GradientColorKey[] { new GradientColorKey(gradient.Evaluate(val), 0.0f) }, new GradientAlphaKey[] { new GradientAlphaKey(0, 0), new GradientAlphaKey(1, 2*fraction), new GradientAlphaKey(1, 1 - 5*fraction), new GradientAlphaKey(0, 1) });
            var col = ps.colorOverLifetime;
            Debug.Log(gradient.Evaluate(val));

            col.enabled = true;
            col.color = grad;

            Vector3 scale = _initialScale;
             value *= Mathf.Lerp(0.5f, 1, value);        
             scale.x = value / 10;           
             scale.y = value / 10;
             scale.z =5 ;
            transform.localScale = scale;
        }

        public void NotifyChange()
        {
            OnAClineChanged?.Invoke(Line);
        }


        private void Update()
        {
            _elapsed += Time.deltaTime;
          //  SpawnArrow();
        }

        // Another way for creating Arrows
        private void SpawnArrow()
        {
            if (_elapsed >= SpawnArrowFrequency)
            {
                Arrow arrow = Instantiate(ArrowPrefab);

                if(FromNode.Bus.BusResult.vm_pu > ToNode.Bus.BusResult.vm_pu)
                {
                    arrow.Origin = FromNode.transform;

                    Vector3 direction = ToNode.transform.position - FromNode.transform.position;

                    arrow.transform.rotation = Quaternion.FromToRotation(arrow.transform.forward, direction);

                    arrow.Duration = SpawnArrowFrequency * 3;

                    arrow.MoveTo(ToNode.transform.position, () => Destroy(arrow));
                }
                else if ( ToNode.Bus.BusResult.vm_pu > FromNode.Bus.BusResult.vm_pu)
                {
                    arrow.Origin = ToNode.transform;
                    Vector3 direction =  FromNode.transform.position - ToNode.transform.position ;
                    arrow.transform.rotation = Quaternion.FromToRotation(arrow.transform.forward, direction);

                    arrow.Duration = SpawnArrowFrequency * 3;

                    arrow.MoveTo(ToNode.transform.position, () => Destroy(arrow));

                }

                    _elapsed = 0;



            }
        }

    }
}
