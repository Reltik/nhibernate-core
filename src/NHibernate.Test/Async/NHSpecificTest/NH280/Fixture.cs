#if NET_4_5
using System;
using System.Collections;
using NUnit.Framework;
using System.Threading.Tasks;
using NHibernate.Util;

namespace NHibernate.Test.NHSpecificTest.NH280
{
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class Fixture : TestCase
	{
		[Test]
		public async Task ConstInSelectAsync()
		{
			using (ISession s = OpenSession())
			{
				Foo f = new Foo("Fiammy");
				await (s.SaveAsync(f));
				await (s.FlushAsync());
				IList l = await (s.CreateQuery("select 'TextConst', 123, 123.5, .5 from Foo").ListAsync());
				IList result = l[0] as IList;
				Assert.AreEqual(typeof (string), result[0].GetType());
				Assert.AreEqual(typeof (Int32), result[1].GetType());
				Assert.AreEqual(typeof (Double), result[2].GetType());
				Assert.AreEqual(typeof (Double), result[3].GetType());
				Assert.AreEqual("TextConst", result[0]);
				Assert.AreEqual(123, result[1]);
				Assert.AreEqual(123.5D, result[2]);
				Assert.AreEqual(0.5D, result[3]);
				l = await (s.CreateQuery("select 123, f from Foo f").ListAsync());
				result = l[0] as IList;
				Assert.AreEqual(typeof (Int32), result[0].GetType());
				Assert.AreEqual(typeof (Foo), result[1].GetType());
				Assert.AreEqual(123, result[0]);
				Assert.AreEqual("Fiammy", (result[1] as Foo).Description);
				l = await (s.CreateQuery("select 123, f.Description from Foo f").ListAsync());
				result = l[0] as IList;
				Assert.AreEqual(123, result[0]);
				Assert.AreEqual("Fiammy", result[1]);
				await (s.DeleteAsync(f));
				await (s.FlushAsync());
			}
		}

		[Test]
		public Task TokenUnknowAsync()
		{
			try
			{
				TokenUnknow();
				return TaskHelper.CompletedTask;
			}
			catch (Exception ex)
			{
				return TaskHelper.FromException<object>(ex);
			}
		}
	}
}
#endif
