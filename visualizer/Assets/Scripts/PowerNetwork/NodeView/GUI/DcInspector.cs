using PowerNetwork.View;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class DcInspector : MonoBehaviour
{
    private DcLineView _dclineView;
    private LineView _aclineView;
    [SerializeField] private FieldDrawer _fieldDrawerPrefab;
    private List<FieldDrawer> _fieldDrawers = new List<FieldDrawer>();


    private FieldInfo[] cachedFields;

    [SerializeField] private Text _title;
    [SerializeField] private Button _submitButton;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                if (hitInfo.transform != null)
                {
                    DcLineView dcL = hitInfo.transform.GetComponent<DcLineView>();
                    if (dcL != null)
                    {
                        SetDcLineView(dcL);                              
                    }
                    LineView acL = hitInfo.transform.GetComponent<LineView>();

                    if(acL != null)
                    {
                        setAcLineView(acL);
                    }
                }
            }
        }
    }

    private void setAcLineView(LineView acLineView)
    {
        for (int i = 0; i < _fieldDrawers.Count; i++)
        {
            _fieldDrawers[i].inputField.onValueChanged.RemoveAllListeners();
            _fieldDrawers[i].toggle.onValueChanged.RemoveAllListeners();
            Destroy(_fieldDrawers[i].gameObject);           
        }
        _fieldDrawers.Clear();

        _aclineView = acLineView;
        Type type = acLineView.Line.GetType();
        print(type);
        cachedFields = type.GetFields();

        for (int i = 0; i < cachedFields.Length; i++)
        {
            FieldDrawer fieldDrawer = Instantiate(_fieldDrawerPrefab, transform);
            FieldInfo fieldInfo = cachedFields[i];
            fieldDrawer.label.text = fieldInfo.Name;
            if (fieldInfo.FieldType == typeof(bool))
            {
                fieldDrawer.toggle.gameObject.SetActive(true);
                fieldDrawer.inputField.gameObject.SetActive(false);
                fieldDrawer.toggle.SetIsOnWithoutNotify((bool)fieldInfo.GetValue(acLineView.Line));
                fieldDrawer.toggle.onValueChanged.AddListener(bo => OnaclineFieldChanged(fieldInfo, bo));
            }
            else
            {
                fieldDrawer.toggle.gameObject.SetActive(false);
                fieldDrawer.inputField.gameObject.SetActive(true);
                fieldDrawer.inputField.SetTextWithoutNotify(fieldInfo.GetValue(acLineView.Line).ToString());
                fieldDrawer.inputField.onValueChanged.AddListener(txt => OnaclineFieldChanged(fieldInfo, txt));
            }

            _fieldDrawers.Add(fieldDrawer);
        }

        _submitButton.onClick.RemoveAllListeners();
    }

    public void SetDcLineView(DcLineView dclineview)
    {
        for (int i = 0; i < _fieldDrawers.Count; i++)
        {
            _fieldDrawers[i].inputField.onValueChanged.RemoveAllListeners();
            _fieldDrawers[i].toggle.onValueChanged.RemoveAllListeners();
            Destroy(_fieldDrawers[i].gameObject);
        }
        _fieldDrawers.Clear();

        _dclineView = dclineview;
        Type type = dclineview.dcLine.GetType();
        print(type);
        cachedFields = type.GetFields();

        for (int i = 0; i < cachedFields.Length; i++)
        {
            FieldDrawer fieldDrawer = Instantiate(_fieldDrawerPrefab, transform);
            FieldInfo fieldInfo = cachedFields[i];
            fieldDrawer.label.text = fieldInfo.Name;
            if (fieldInfo.FieldType == typeof(bool))
            {
                fieldDrawer.toggle.gameObject.SetActive(true);
                fieldDrawer.inputField.gameObject.SetActive(false);
                fieldDrawer.toggle.SetIsOnWithoutNotify((bool)fieldInfo.GetValue(_dclineView.dcLine));
                fieldDrawer.toggle.onValueChanged.AddListener(b => OnFieldChanged(fieldInfo, b));
            }
            else
            {
                fieldDrawer.toggle.gameObject.SetActive(false);
                fieldDrawer.inputField.gameObject.SetActive(true);
                fieldDrawer.inputField.SetTextWithoutNotify(fieldInfo.GetValue(_dclineView.dcLine).ToString());
                fieldDrawer.inputField.onValueChanged.AddListener(txt => OnFieldChanged(fieldInfo, txt));
                
                
            }

            _fieldDrawers.Add(fieldDrawer);
        }
    
        _submitButton.onClick.RemoveAllListeners();
    }

    private void OnSubmitDCButtonClicked()
    {

        _dclineView.NotifyChange();

    }

    private void OnSubmitACButtonClicked()
    {

        _aclineView.NotifyChange();

    }
    private void OnFieldChanged(FieldInfo fieldInfo, string txt)
    {
        print($"{fieldInfo.Name} = {txt}");
        fieldInfo.SetValue(_dclineView.dcLine, Convert.ChangeType(txt, fieldInfo.FieldType));
        _submitButton.gameObject.SetActive(true);
        _submitButton.onClick.AddListener(() => OnSubmitDCButtonClicked());
       
    }
    private void OnFieldChanged(FieldInfo fieldInfo, bool b)
    {
        print($"{fieldInfo.Name} = {b}");
        fieldInfo.SetValue(_dclineView.dcLine, Convert.ChangeType(b, fieldInfo.FieldType));
        _submitButton.gameObject.SetActive(true);
        _submitButton.onClick.AddListener(() => OnSubmitDCButtonClicked());
    }

    private void OnaclineFieldChanged(FieldInfo fieldInfo, string txt)
    {
        print($"{fieldInfo.Name} = {txt}");
        fieldInfo.SetValue(_aclineView.Line, Convert.ChangeType(txt, fieldInfo.FieldType));
        _submitButton.gameObject.SetActive(true);
        _submitButton.onClick.AddListener(() => OnSubmitACButtonClicked());
    }
    private void OnaclineFieldChanged(FieldInfo fieldInfo, bool b)
    {
        print($"{fieldInfo.Name} = {b}");
        fieldInfo.SetValue(_aclineView.Line, Convert.ChangeType(b, fieldInfo.FieldType));
        _submitButton.gameObject.SetActive(true);
        _submitButton.onClick.AddListener(() => OnSubmitACButtonClicked());
    }

}