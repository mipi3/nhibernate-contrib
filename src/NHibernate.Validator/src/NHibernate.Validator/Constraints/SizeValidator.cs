using System;
using System.Collections;
using NHibernate.Validator.Engine;

namespace NHibernate.Validator.Constraints
{
	[Serializable]
	public class SizeValidator : IInitializableValidator<SizeAttribute>
	{
		private int max;
		private int min;

		#region IInitializableValidator<SizeAttribute> Members

		public bool IsValid(object value, IConstraintValidatorContext validatorContext)
		{
			if (value == null)
			{
				return true;
			}

			var collection = value as ICollection;
			if (collection == null)
			{
				return false;
			}

			return collection.Count >= min && collection.Count <= max;
		}

		public void Initialize(SizeAttribute parameters)
		{
			min = parameters.Min;
			max = parameters.Max;
		}

		#endregion
	}
}