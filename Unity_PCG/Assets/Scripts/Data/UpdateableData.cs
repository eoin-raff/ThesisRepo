using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateableData : ScriptableObject
{
    public event System.Action OnValuesUpdated;
    public bool AutoUpdate;

    protected virtual void OnValidate()
    {
        if (AutoUpdate)
        {
            UnityEditor.EditorApplication.update += NotifyOfUpdatedValued;
        }
    }

    public void NotifyOfUpdatedValued()
    {
        UnityEditor.EditorApplication.update -= NotifyOfUpdatedValued;
        if (OnValuesUpdated != null)
        {
            OnValuesUpdated();
        }
    }

}
