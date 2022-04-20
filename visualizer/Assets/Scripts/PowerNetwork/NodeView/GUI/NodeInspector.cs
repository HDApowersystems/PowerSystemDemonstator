using PowerNetwork.View;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class NodeInspector : MonoBehaviour
{
    private NodeView _nodeView;
    public BusView _busView;

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

                    NodeView n = hitInfo.transform.GetComponent<NodeView>();
                   
                    if (n != null)
                    {
                        SetNodeView(n);
                    }

                   // BusView b = hitInfo.transform.GetComponent<BusView>();

                  //  if (b != null)
                  //  {
                  //      SetBusView(b);
                  //  }


                }
            }
        }
    }

    private void SetBusView(BusView b)
    {
        for (int i = 0; i < _fieldDrawers.Count; i++)
        {
            _fieldDrawers[i].inputField.onValueChanged.RemoveAllListeners();
            _fieldDrawers[i].toggle.onValueChanged.RemoveAllListeners();
            Destroy(_fieldDrawers[i].gameObject);
        }
        _fieldDrawers.Clear();

        _busView = b;
        Type type = _busView.Bus.BusResult.GetType();
        print(type);

        cachedFields = type.GetFields();
        for (int i = 0; i < cachedFields.Length; i++)
        {
            FieldDrawer fieldDrawer = Instantiate(_fieldDrawerPrefab, transform);
            FieldInfo fieldInfo = cachedFields[i];
            fieldDrawer.label.text = fieldInfo.Name;
            fieldDrawer.toggle.gameObject.SetActive(false);
            fieldDrawer.inputField.gameObject.SetActive(true);
            fieldDrawer.inputField.SetTextWithoutNotify(fieldInfo.GetValue(_busView.Bus.BusResult).ToString());
            _fieldDrawers.Add(fieldDrawer);
            _submitButton.gameObject.SetActive(false);
        }
    }

    public void SetNodeView(NodeView nodeview)
    {
        for (int i = 0; i < _fieldDrawers.Count; i++)
        {
            _fieldDrawers[i].inputField.onValueChanged.RemoveAllListeners();
            _fieldDrawers[i].toggle.onValueChanged.RemoveAllListeners();
            Destroy(_fieldDrawers[i].gameObject);
        }
        _fieldDrawers.Clear();

       // _title.text = nodeview.node.name;
        _nodeView = nodeview;
        Type type = _nodeView.node.GetType();
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
                fieldDrawer.toggle.SetIsOnWithoutNotify((bool)fieldInfo.GetValue(_nodeView.node));
                fieldDrawer.toggle.onValueChanged.AddListener(b => OnFieldChanged(fieldInfo, b));
            }
            else
            {
                fieldDrawer.toggle.gameObject.SetActive(false);
                fieldDrawer.inputField.gameObject.SetActive(true);
                fieldDrawer.inputField.SetTextWithoutNotify(fieldInfo.GetValue(_nodeView.node).ToString());
                fieldDrawer.inputField.onValueChanged.AddListener(txt => OnFieldChanged(fieldInfo, txt));
            }

            _fieldDrawers.Add(fieldDrawer);
        }

        _submitButton.onClick.RemoveAllListeners();      
    }

    private void OnSubmitButtonClicked()
    {
        _nodeView.NotifyChange();
    }


    private void OnFieldChanged(FieldInfo fieldInfo, string txt)
    {
        print($"{fieldInfo.Name} = {txt}");
        fieldInfo.SetValue(_nodeView.node, Convert.ChangeType(txt, fieldInfo.FieldType));
        _submitButton.gameObject.SetActive(true);
        _submitButton.onClick.AddListener(OnSubmitButtonClicked);
    }

    private void  OnFieldChanged(FieldInfo fieldInfo, bool b)
    {
        print($"{fieldInfo.Name} = {b}");
        fieldInfo.SetValue(_nodeView.node, Convert.ChangeType(b, fieldInfo.FieldType));
        _submitButton.gameObject.SetActive(true);
        _submitButton.onClick.AddListener(() => OnSubmitButtonClicked());
    }

}
