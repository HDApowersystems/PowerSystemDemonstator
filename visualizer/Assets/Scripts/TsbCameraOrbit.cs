using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;

public class TsbCameraOrbit : MonoBehaviour
{
    public Transform subject;
    public Transform targetLookAtLocation;
    float distance = 1.5f;
    public int cameraSpeed = 50;
    public float xSpeed = 10f;
    public float ySpeed = 10.0f;
    public float pinchSpeed;
    private float lastDist = 0;
    private float curDist = 0;
    public int yMinLimit = 10; //Lowest vertical angle in respect with the target.
    public int yMaxLimit = 80;
    public float minDistance = 5f; //Min distance of the camera from the target
    public float maxDistance = 20f;
    private float x = 0.0f;
    private float y = 0.0f;
    private Touch touch;
    bool isSetUp = false;
    public bool rightPanel = false;
    Vector3 centerPoint;
    Vector3 zoomHitPoint = Vector3.zero;
  //  float zoomHitRayTime = 0;
    public float rightPanelSlide = 0f;
    Vector3 ?startPosition = null;
    bool animating = false;
    float animationStartTime;
    float animationStartDistance;
    float animationEndDistance;
    Vector3 animationZoomHitStartPoint;
    Vector3 animationZoomHitEndPoint;
    Quaternion animationStartRotation;
    Quaternion animationEndRotation;
    public float animationSpeed = 10;
    float animationDuration;
    Vector2 ?lastDragPointerPosition = null;
    public event Action AnimationComplete;
    public EventSystem eventSystem;

    /*
    public override void OnAwake()
    {
        base.OnAwake();
        GetTransitionState().OnPresentEnd += Setup;
    }*/
    void Start()
    {
        Setup();
    }

    public void Setup()
    {
        Vector3 angles = subject.transform.eulerAngles;
        x = angles.y;
        y = angles.x;
        centerPoint = targetLookAtLocation.position;
        distance = Vector3.Distance(subject.transform.position, centerPoint);

        maxDistance = distance;
        isSetUp = true;
    }
    static float ClampAngle(float angle, float min, float max)
    {

        if (angle < -360)

            angle += 360;

        if (angle > 360)

            angle -= 360;

        return Mathf.Clamp(angle, min, max);

    }
    public bool IsPositionInView(Vector3 position)
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = position;
        List<RaycastResult> results = new List<RaycastResult>();
        eventSystem.RaycastAll(eventDataCurrentPosition, results);
        return results.Count == 0;
    }
    void Zoom(float input, Vector2 position)
    {
        
        distance += input;
        if (Distance01() > 0.95f)
        {
            RaycastHit hit;
            int layer = 1 << 9;
            Ray ray = Camera.main.ScreenPointToRay(position);
            if (Physics.Raycast(ray, out hit, 100.0f, layer))
            {
                zoomHitPoint = hit.point;
            }
        }
        /*
        if (input < 0)
        {
            if (Time.time - zoomHitRayTime > 1)
            {
                RaycastHit hit;
                int layer = 1 << 9;
                Ray ray = Camera.main.ScreenPointToRay(position);
                if (Physics.Raycast(ray, out hit, 100.0f, layer))
                {
                    zoomHitPoint = hit.point;
                }
            }
            zoomHitRayTime = Time.time;
        }
        else if (input > 0)
        {
            //centerPoint = Vector3.MoveTowards(centerPoint, targetLookAtLocation.position, -Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * 300);
            //zoomHitRayTime = 0;
        }
        */
    }

    Vector2 ?MouseInput()
    {
        if (Input.GetMouseButton(0))
        {
            if (IsPositionInView(Input.mousePosition))
            {
                x += Input.GetAxis("Mouse X") * xSpeed * 0.2f;
                y -= Input.GetAxis("Mouse Y") * ySpeed * 0.2f;
            }
        }
        Zoom(-Input.GetAxis("Mouse ScrollWheel"), Input.mousePosition);
        if (Input.GetMouseButton(1))
        {
            return PixelsTo01(Input.mousePosition);
        }
        return null;

    }
    void FinishAnimation()
    {
        AnimationComplete?.Invoke();
        animating = false;
    }
    void Animate()
    {
        float animationTime = Mathf.Clamp01((Time.time - animationStartTime) / ((1f / animationSpeed) * animationDuration));
        if (float.IsNaN(animationTime))
        {
            FinishAnimation();
        }
        else
        {
            animationTime = Mathf.SmoothStep(0f, 1f, animationTime);
            distance = Mathf.Lerp(animationStartDistance, animationEndDistance, animationTime);
            zoomHitPoint = Vector3.Lerp(animationZoomHitStartPoint, animationZoomHitEndPoint, animationTime);
            Vector3 animationEuler = Quaternion.Lerp(animationStartRotation, animationEndRotation, animationTime).eulerAngles;
            x = animationEuler.y;
            y = animationEuler.x;
        }

        if (Mathf.Approximately(animationTime, 1))
        {
            FinishAnimation();
        }
    }
    public void StartAnimation(float toDistance01, Vector3 toCenter, Quaternion toRotation)
    {
        animationStartDistance = distance;
        animationEndDistance = ExtensionMethods.Remap( toDistance01, 0,1, minDistance, maxDistance);
        animationZoomHitStartPoint = zoomHitPoint;
        animationZoomHitEndPoint = toCenter;
        animationStartRotation = transform.rotation;
        animationEndRotation = toRotation;
        animationDuration = (Quaternion.Angle(animationStartRotation, animationEndRotation) / 60f ) + Mathf.Abs(Distance01() - toDistance01) + (Vector3.Distance(animationZoomHitStartPoint, animationZoomHitEndPoint) / 1.5f);
        animationStartTime = Time.time;
        animating = true;
    }
    public void ResetCamera()
    {
        animating = false;
        StartAnimation(1, targetLookAtLocation.position, Quaternion.LookRotation(targetLookAtLocation.position - (Vector3)startPosition));
    }


    Vector2 PixelsTo01(Vector2 v)
    {
        v.x /= Screen.width;
        v.y /= Screen.height;
        return v;
    }
    float Distance01()
    {
        return ExtensionMethods.RemapClamp(distance, minDistance, maxDistance, 0, 1f);
    }
    void Update()
    {

        Vector2 ?currentDragPoninterPosition;


        if (isSetUp)
        {
            if (animating)
            {
                Animate();

            }
            else // if input is in bounds
                {
                    //Zooming with mouse
                    /*
                    distance += Input.GetAxis("Mouse ScrollWheel")*distance;
				
                    distance = Mathf.Clamp(distance, minDistance, maxDistance);
                    */




                    if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved)
                    {

                        //One finger touch does orbit

                        touch = Input.GetTouch(0);
                        if (IsPositionInView(touch.position))
                        {
                            x += touch.deltaPosition.x * xSpeed * 0.02f;
                            y -= touch.deltaPosition.y * ySpeed * 0.02f;
                        }

                }



                if (Input.touchCount > 1 && (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(1).phase == TouchPhase.Moved)) {
                    //Two finger touch does pinch to zoom

                    var touch1 = Input.GetTouch(0);
                    var touch2 = Input.GetTouch(1);
					
                    curDist = Vector2.Distance(PixelsTo01(touch1.position), PixelsTo01(touch2.position));
					float distDiff = Vector2.Distance(PixelsTo01(touch1.deltaPosition), PixelsTo01(touch2.deltaPosition)) * pinchSpeed / 10;
                    Vector2 averagePos = (touch1.position + touch2.position) / 2;
                    if (curDist > lastDist)
						
                        {

                        Zoom(-distDiff, averagePos);
						
                        }else{

                        Zoom(distDiff, averagePos);

                    }
					
					
					
                    lastDist = curDist;
                    currentDragPoninterPosition = (PixelsTo01(touch1.position) + PixelsTo01(touch2.position)) / 2;
                }
                else
                {
                    currentDragPoninterPosition = null;
                }
#if UNITY_EDITOR
                currentDragPoninterPosition = MouseInput();
#endif
#if UNITY_STANDALONE
                currentDragPoninterPosition = MouseInput();
#endif
                if (lastDragPointerPosition != null && currentDragPoninterPosition != null)
                {
                    Vector2 positionDiff = (Vector2)currentDragPoninterPosition - (Vector2)lastDragPointerPosition;
                    Vector3 drag = new Vector3(positionDiff.x, 0, positionDiff.y);
                    drag = Quaternion.Euler(0, x, 0) * drag;
                    zoomHitPoint -= drag * ExtensionMethods.Remap(distance, minDistance, maxDistance, 2, 10);
                    //zoomHitPoint.x += positionDiff.x;
                    //zoomHitPoint.z += positionDiff.y;
                }
                lastDragPointerPosition = currentDragPoninterPosition;
                //Detect mouse drag;
                /*
                if(Input.GetMouseButton(0))   {



                    x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;

                    y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;       

                }
                */
                y = ClampAngle(y, yMinLimit, yMaxLimit);

            }
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
            centerPoint = Vector3.Lerp(zoomHitPoint, targetLookAtLocation.position, (distance - minDistance) / (maxDistance - minDistance));// MoveTowards(centerPoint, zoomHitPoint, Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * 300);



            Quaternion rotation = Quaternion.Euler(y, x, 0);


            Vector3 vTemp = new Vector3(0.0f, 0.0f, -distance);
            rightPanelSlide = Mathf.Lerp(rightPanelSlide, rightPanel ? 0.1f : 0, Time.deltaTime * 5);

            vTemp.x = distance * rightPanelSlide;

            Vector3 position = rotation * vTemp + centerPoint;



            subject.transform.position = position; // Vector3.Lerp(subject.transform.position, position, cameraSpeed * Time.deltaTime);

            subject.transform.rotation = rotation;

            if (startPosition == null)
            {
                startPosition = position;
            }
        }
    }
}
