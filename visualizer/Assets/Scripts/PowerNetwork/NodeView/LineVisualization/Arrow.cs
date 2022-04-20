using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class Arrow : MonoBehaviour
{

    public Transform Origin;
    public Vector3 OffSet;
    public float Duration;
    public Ease Ease;
    public Color StartColor;
    public Color EndColor;
    public Material material;
    private Rigidbody _rigidbody;


    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    public void MoveTo(Vector3 pos, Action onFinish)
    {
        if (material != null)
        {
            material.color = StartColor;
            material.DOColor(EndColor, Duration)
                .SetEase(Ease);
        }

        _rigidbody.position = Origin.position + OffSet;
        _rigidbody.DOMove(pos + OffSet, Duration)
            .SetEase(Ease)
            .OnComplete(() => { onFinish?.Invoke(); });
    }


}
