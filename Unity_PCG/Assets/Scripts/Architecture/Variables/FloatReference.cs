using System;

namespace MED10.Architecture.Variables
{
    [Serializable]
    public class FloatReference
    {
        public bool UseConstant = true;
        public float ConstantValue;
        public FloatVariable VariableValue;

        public float Value
        {
            get { return UseConstant ? ConstantValue : VariableValue.Value; }
        }
    }

}