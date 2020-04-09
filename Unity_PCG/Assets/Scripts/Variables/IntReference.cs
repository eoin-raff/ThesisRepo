using System;

namespace MED10.Variables
{
    [Serializable]
    public class IntReference
    {
        public bool UseConstant = true;
        public int ConstantValue;
        public IntVariable VariableValue;

        public int Value
        {
            get { return UseConstant ? ConstantValue : VariableValue.Value; }
        }
    }

}