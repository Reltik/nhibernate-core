﻿#if NET_4_5
using System;
using System.Linq;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Linq;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;
using System.Threading.Tasks;

namespace NHibernate.Test.NHSpecificTest.NH2905
{
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class FixtureAsync : TestCaseMappingByCodeAsync
	{
		private Guid _entity3Id;
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();
			mapper.Class<Entity1>(rc =>
			{
				rc.Id(x => x.Id, m =>
				{
					m.Generator(Generators.Guid);
					m.Column("id");
				}

				);
				rc.ManyToOne(x => x.Entity2, m => m.Column("entity2_id"));
			}

			);
			mapper.Class<Entity2>(rc =>
			{
				rc.Id(x => x.Id, m =>
				{
					m.Generator(Generators.Guid);
					m.Column("id");
				}

				);
				rc.Set(x => x.Entity3s, m =>
				{
					m.Inverse(true);
					m.Key(km => km.Column("entity2_id"));
				}

				, e => e.OneToMany(m => m.Class((typeof (Entity3)))));
			}

			);
			mapper.Class<Entity3>(rc =>
			{
				rc.Id(x => x.Id, m =>
				{
					m.Generator(Generators.Guid);
					m.Column("id");
				}

				);
				rc.ManyToOne(x => x.Entity2, m => m.Column("entity2_id"));
			}

			);
			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override async Task OnSetUpAsync()
		{
			using (ISession session = OpenSession())
				using (ITransaction tx = session.BeginTransaction())
				{
					var entity1 = new Entity1();
					var entity2 = new Entity2();
					var entity3 = new Entity3();
					entity1.Entity2 = entity2;
					entity2.Entity3s.Add(entity3);
					entity3.Entity2 = entity2;
					await (session.SaveAsync(entity1));
					await (session.SaveAsync(entity2));
					await (session.SaveAsync(entity3));
					_entity3Id = entity3.Id;
					await (tx.CommitAsync());
				}
		}

		protected override async Task OnTearDownAsync()
		{
			using (ISession session = OpenSession())
				using (ITransaction tx = session.BeginTransaction())
				{
					await (session.DeleteAsync("from Entity3"));
					await (session.DeleteAsync("from Entity1"));
					await (session.DeleteAsync("from Entity2"));
					await (tx.CommitAsync());
				}
		}

		[Test]
		public async Task JoinOverMultipleSteps_MethodSyntax_SelectAndSelectManyAsync()
		{
			using (ISession session = OpenSession())
				using (ITransaction tx = session.BeginTransaction())
				{
					var result = await (session.Query<Entity1>().Select(x => x.Entity2).SelectMany(x => x.Entity3s).Where(x => x.Id == _entity3Id).ToListAsync());
					await (tx.CommitAsync());
					Assert.That(result.Count, Is.EqualTo(1));
					Assert.That(result[0].Id, Is.EqualTo(_entity3Id));
				}
		}

		[Test]
		public async Task JoinOverMultipleSteps_MethodSyntax_OnlySelectManyAsync()
		{
			using (ISession session = OpenSession())
				using (ITransaction tx = session.BeginTransaction())
				{
					var result = await (session.Query<Entity1>().SelectMany(x => x.Entity2.Entity3s).Where(x => x.Id == _entity3Id).ToListAsync());
					await (tx.CommitAsync());
					Assert.That(result.Count, Is.EqualTo(1));
					Assert.That(result[0].Id, Is.EqualTo(_entity3Id));
				}
		}

		[Test]
		public async Task JoinOverMultipleSteps_QuerySyntax_LetAndFromAsync()
		{
			using (ISession session = OpenSession())
				using (ITransaction tx = session.BeginTransaction())
				{
					var result = await ((
						from e1 in session.Query<Entity1>()let e2 = e1.Entity2
						from e3 in e2.Entity3s
						where e3.Id == _entity3Id
						select e3).ToListAsync());
					await (tx.CommitAsync());
					Assert.That(result.Count, Is.EqualTo(1));
					Assert.That(result[0].Id, Is.EqualTo(_entity3Id));
				}
		}

		[Test]
		public async Task JoinOverMultipleSteps_QuerySyntax_OnlyFromAsync()
		{
			using (ISession session = OpenSession())
				using (ITransaction tx = session.BeginTransaction())
				{
					var result = await ((
						from e1 in session.Query<Entity1>()from e3 in e1.Entity2.Entity3s
						where e3.Id == _entity3Id
						select e3).ToListAsync());
					await (tx.CommitAsync());
					Assert.That(result.Count, Is.EqualTo(1));
					Assert.That(result[0].Id, Is.EqualTo(_entity3Id));
				}
		}
	}
}
#endif