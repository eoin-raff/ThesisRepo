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
            NotifyOfUpdatedValued();
        }
    }

    public void NotifyOfUpdatedValued()
    {
        if (OnValuesUpdated != null)
        {
            OnValuesUpdated();
        }
    }

}
