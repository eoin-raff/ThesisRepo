using UnityEngine;

namespace MED10.Architecture.Variables
{
	[CreateAssetMenu]
	public abstract class VariableObject<T> : ScriptableObject
	{
		public T Value;
		[SerializeField]
		private T FixedValue;
		private void Awake()
		{
			Value = FixedValue;
		}
	}
}