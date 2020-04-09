using System;

namespace MED10.Variables
{
    [Serializable]
    public class BoolReference
    {
        public bool UseConstant = true;
        public bool ConstantValue;
        public BoolVariable VariableValue;

        public bool Value
        {
            get { return UseConstant ? ConstantValue : VariableValue.Value; }
        }
    } 
}
