using System;
using NHibernate.Validator.Engine;
using NUnit.Framework;

namespace NHibernate.Validator.Tests.ValidatorsTest
{
	/// <summary>
	/// Fixture to validate the existing validators
	/// </summary>
	[TestFixture]
	public class ValidatorsFixture : BaseValidatorFixture
	{
		[Test]
		public void LengthTest()
		{
			FooLength f1 = new FooLength(1, "hola");
			IClassValidator validator = GetClassValidator(typeof(FooLength));

			InvalidValue[] res = validator.GetInvalidValues(f1);
			Assert.AreEqual(0, res.Length);

			FooLength f2 = new FooLength(1, string.Empty);

			InvalidValue[] res2 = validator.GetInvalidValues(f2);
			Assert.AreEqual(1, res2.Length);

			FooLength f3 = new FooLength(1, null);
			InvalidValue[] res3 = validator.GetInvalidValues(f3);
			Assert.AreEqual(1, res3.Length);
		}

		[Test]
		public void NotEmptyTest()
		{
			IClassValidator validator = GetClassValidator(typeof(FooNotEmpty));

			FooNotEmpty f1 = new FooNotEmpty("hola");
			FooNotEmpty f2 = new FooNotEmpty(string.Empty);

			validator.AssertValid(f1);

			InvalidValue[] res = validator.GetInvalidValues(f2);
			Assert.AreEqual(1, res.Length);
		}

		[Test]
		public void PastAndFuture()
		{
			IClassValidator validator = GetClassValidator(typeof(FooDate));
			FooDate f = new FooDate();

			f.Past = DateTime.MinValue;
			f.Future = DateTime.MaxValue;
			Assert.AreEqual(0, validator.GetInvalidValues(f).Length);

			f.Future = DateTime.Today.AddDays(1); //tomorrow
			f.Past = DateTime.Today.AddDays(-1); //yesterday
			Assert.AreEqual(0, validator.GetInvalidValues(f).Length);

			f.Future = DateTime.Today.AddDays(-1); //yesterday
			Assert.AreEqual(1, validator.GetInvalidValues(f).Length);

			f.Future = DateTime.Today.AddDays(1); //tomorrow
			f.Past = DateTime.Today.AddDays(1); //tomorrow
			Assert.AreEqual(1, validator.GetInvalidValues(f).Length);

			f.Future = DateTime.Now.AddMilliseconds(-1);
			f.Past = DateTime.Now.AddMilliseconds(+1);
			Assert.AreEqual(2, validator.GetInvalidValues(f).Length);
		}

		[Test]
		public void NullString()
		{
			FooNotEmpty f = new FooNotEmpty(null);

			IClassValidator vtor = GetClassValidator(typeof (FooNotEmpty));

			Assert.AreEqual(1, vtor.GetInvalidValues(f).Length);
		}
	}
}