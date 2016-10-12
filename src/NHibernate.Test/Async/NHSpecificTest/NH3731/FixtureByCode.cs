﻿#if NET_4_5
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Linq;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;
using System.Threading.Tasks;

namespace NHibernate.Test.NHSpecificTest.NH3731
{
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class ByCodeFixtureAsync : TestCaseMappingByCodeAsync
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();
			mapper.Class<Parent>(rc =>
			{
				rc.Id(x => x.Id, m => m.Generator(Generators.GuidComb));
				rc.Property(x => x.Name);
				rc.List(x => x.ChildrenList, c => c.Cascade(Mapping.ByCode.Cascade.All | Mapping.ByCode.Cascade.DeleteOrphans), x => x.OneToMany());
				rc.Map(x => x.ChildrenMap, c => c.Cascade(Mapping.ByCode.Cascade.All | Mapping.ByCode.Cascade.DeleteOrphans), x => x.OneToMany());
			}

			);
			mapper.Class<ListChild>(rc =>
			{
				rc.Id(x => x.Id, m => m.Generator(Generators.GuidComb));
				rc.Property(x => x.Name);
			}

			);
			mapper.Class<MapChild>(rc =>
			{
				rc.Id(x => x.Id, m => m.Generator(Generators.GuidComb));
				rc.Property(x => x.Name);
			}

			);
			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override async Task OnSetUpAsync()
		{
			using (ISession session = OpenSession())
				using (ITransaction transaction = session.BeginTransaction())
				{
					var p = new Parent{Name = "Parent"};
					p.ChildrenList.Add(new ListChild{Name = "ListChild 1"});
					p.ChildrenList.Add(new ListChild{Name = "ListChild 2"});
					p.ChildrenList.Add(new ListChild{Name = "ListChild 3"});
					p.ChildrenMap.Add("first", new MapChild{Name = "MapChild 1"});
					p.ChildrenMap.Add("second", new MapChild{Name = "MapChild 2"});
					p.ChildrenMap.Add("third", new MapChild{Name = "MapChild 3"});
					await (session.SaveAsync(p));
					await (session.FlushAsync());
					await (transaction.CommitAsync());
				}
		}

		protected override async Task OnTearDownAsync()
		{
			using (ISession session = OpenSession())
				using (ITransaction transaction = session.BeginTransaction())
				{
					await (session.DeleteAsync("from System.Object"));
					await (session.FlushAsync());
					await (transaction.CommitAsync());
				}
		}

		[Test]
		public async Task Serializing_Session_After_Reordering_ChildrenList_Should_WorkAsync()
		{
			using (ISession session = OpenSession())
			{
				using (ITransaction transaction = session.BeginTransaction())
				{
					var p = await (session.Query<Parent>().SingleAsync());
					var c = p.ChildrenList.Last();
					p.ChildrenList.Remove(c);
					p.ChildrenList.Insert(p.ChildrenList.Count - 1, c);
					await (session.FlushAsync());
					await (transaction.CommitAsync());
				}

				using (MemoryStream stream = new MemoryStream())
				{
					BinaryFormatter formatter = new BinaryFormatter();
					formatter.Serialize(stream, session);
					Assert.AreNotEqual(0, stream.Length);
				}
			}
		}

		[Test]
		public async Task Serializing_Session_After_Deleting_First_Child_In_List_Should_WorkAsync()
		{
			using (ISession session = OpenSession())
			{
				using (ITransaction transaction = session.BeginTransaction())
				{
					var p = await (session.Query<Parent>().SingleAsync());
					p.ChildrenList.RemoveAt(0);
					await (session.FlushAsync());
					await (transaction.CommitAsync());
				}

				using (MemoryStream stream = new MemoryStream())
				{
					BinaryFormatter formatter = new BinaryFormatter();
					formatter.Serialize(stream, session);
					Assert.AreNotEqual(0, stream.Length);
				}
			}
		}

		[Test]
		public async Task Serializing_Session_After_Changing_Key_ChildrenMap_Should_WorkAsync()
		{
			using (ISession session = OpenSession())
			{
				using (ITransaction transaction = session.BeginTransaction())
				{
					var p = await (session.Query<Parent>().SingleAsync());
					var firstChild = p.ChildrenMap["first"];
					var secondChild = p.ChildrenMap["second"];
					p.ChildrenMap.Remove("first");
					p.ChildrenMap.Remove("second");
					p.ChildrenMap.Add("first", secondChild);
					p.ChildrenMap.Add("second", firstChild);
					await (session.FlushAsync());
					await (transaction.CommitAsync());
				}

				using (MemoryStream stream = new MemoryStream())
				{
					BinaryFormatter formatter = new BinaryFormatter();
					formatter.Serialize(stream, session);
					Assert.AreNotEqual(0, stream.Length);
				}
			}
		}
	}
}
#endif
