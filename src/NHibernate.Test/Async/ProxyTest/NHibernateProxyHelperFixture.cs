﻿#if NET_4_5
using System;
using System.Collections;
using NHibernate.Proxy;
using NUnit.Framework;
using System.Threading.Tasks;

namespace NHibernate.Test.ProxyTest
{
	[TestFixture]
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class NHibernateProxyHelperFixtureAsync : TestCaseAsync
	{
		protected override string MappingsAssembly
		{
			get
			{
				return "NHibernate.Test";
			}
		}

		protected override IList Mappings
		{
			get
			{
				return new string[]{"ProxyTest.AProxy.hbm.xml"};
			}
		}

		[Test]
		public async Task GetClassOfProxyAsync()
		{
			ISession s = null;
			AProxy a = new AProxy();
			try
			{
				s = OpenSession();
				a.Name = "a proxy";
				await (s.SaveAsync(a));
				await (s.FlushAsync());
			}
			finally
			{
				if (s != null)
				{
					s.Close();
				}
			}

			try
			{
				s = OpenSession();
				System.Type type = NHibernateProxyHelper.GetClassWithoutInitializingProxy(a);
				Assert.AreEqual(typeof (AProxy), type, "Should have returned 'A' for a non-proxy");
				AProxy aProxied = (AProxy)await (s.LoadAsync(typeof (AProxy), a.Id));
				Assert.IsFalse(NHibernateUtil.IsInitialized(aProxied), "should be a proxy");
				type = NHibernateProxyHelper.GetClassWithoutInitializingProxy(aProxied);
				Assert.AreEqual(typeof (AProxy), type, "even though aProxied was a Proxy it should have returned the correct type.");
				await (s.DeleteAsync(aProxied));
				await (s.FlushAsync());
			}
			finally
			{
				if (s != null)
				{
					s.Close();
				}
			}
		}
	}
}
#endif