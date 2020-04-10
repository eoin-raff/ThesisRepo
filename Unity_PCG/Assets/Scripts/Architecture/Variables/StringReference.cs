using System;
using UnityEngine;

namespace MED10.Architecture.Variables
{
    [Serializable]
    public class StringReference : MonoBehaviour
    {
        public bool UseConstant = true;
        public string ConstantValue;
        public StringVariable VariableValue;
        public string Value
        {
            get { return UseConstant ? ConstantValue : VariableValue.Value; }
        }
    } 
}
