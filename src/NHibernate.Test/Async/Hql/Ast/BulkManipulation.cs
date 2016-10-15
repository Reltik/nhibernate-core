﻿#if NET_4_5
using System;
using System.Collections;
using System.Threading;
using NHibernate.Dialect;
using NHibernate.Hql.Ast.ANTLR;
using NHibernate.Id;
using NHibernate.Persister.Entity;
using NUnit.Framework;
using System.Threading.Tasks;
using NHibernate.Util;

namespace NHibernate.Test.Hql.Ast
{
	[TestFixture]
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class BulkManipulationAsync : BaseFixtureAsync
	{
		public ISession OpenNewSession()
		{
			return OpenSession();
		}

		[Test]
		public async Task DeleteNonExistentEntityAsync()
		{
			using (ISession s = OpenSession())
			{
				Assert.ThrowsAsync<QuerySyntaxException>(async () => await (s.CreateQuery("delete NonExistentEntity").ExecuteUpdateAsync()));
			}
		}

		[Test]
		public async Task UpdateNonExistentEntityAsync()
		{
			using (ISession s = OpenSession())
			{
				Assert.ThrowsAsync<QuerySyntaxException>(async () => await (s.CreateQuery("update NonExistentEntity e set e.someProp = ?").ExecuteUpdateAsync()));
			}
		}

		[Test]
		public async Task SimpleInsertAsync()
		{
			var data = new TestData(this);
			await (data.PrepareAsync());
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			await (s.CreateQuery("insert into Pickup (id, Vin, Owner) select id, Vin, Owner from Car").ExecuteUpdateAsync());
			await (t.CommitAsync());
			t = s.BeginTransaction();
			await (s.CreateQuery("delete Vehicle").ExecuteUpdateAsync());
			await (t.CommitAsync());
			s.Close();
			await (data.CleanupAsync());
		}

		[Test]
		public async Task InsertWithManyToOneAsync()
		{
			var data = new TestData(this);
			await (data.PrepareAsync());
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			await (s.CreateQuery("insert into Animal (description, bodyWeight, mother) select description, bodyWeight, mother from Human").ExecuteUpdateAsync());
			await (t.CommitAsync());
			t = s.BeginTransaction();
			await (t.CommitAsync());
			s.Close();
			await (data.CleanupAsync());
		}

		[Test]
		public async Task InsertWithMismatchedTypesAsync()
		{
			var data = new TestData(this);
			await (data.PrepareAsync());
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			Assert.ThrowsAsync<QueryException>(async () => await (s.CreateQuery("insert into Pickup (Owner, Vin, id) select id, Vin, Owner from Car").ExecuteUpdateAsync()), "mismatched types did not error");
			await (t.CommitAsync());
			t = s.BeginTransaction();
			await (s.CreateQuery("delete Vehicle").ExecuteUpdateAsync());
			await (t.CommitAsync());
			s.Close();
			await (data.CleanupAsync());
		}

		[Test]
		public async Task InsertIntoSuperclassPropertiesFailsAsync()
		{
			var data = new TestData(this);
			await (data.PrepareAsync());
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			Assert.ThrowsAsync<QueryException>(async () => await (s.CreateQuery("insert into Human (id, bodyWeight) select id, bodyWeight from Lizard").ExecuteUpdateAsync()), "superclass prop insertion did not error");
			await (t.CommitAsync());
			t = s.BeginTransaction();
			await (s.CreateQuery("delete Animal where mother is not null").ExecuteUpdateAsync());
			await (s.CreateQuery("delete Animal where father is not null").ExecuteUpdateAsync());
			await (s.CreateQuery("delete Animal").ExecuteUpdateAsync());
			await (t.CommitAsync());
			s.Close();
			await (data.CleanupAsync());
		}

		[Test]
		public async Task InsertAcrossMappedJoinFailsAsync()
		{
			var data = new TestData(this);
			await (data.PrepareAsync());
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			Assert.ThrowsAsync<QueryException>(async () => await (s.CreateQuery("insert into Joiner (name, joinedName) select vin, owner from Car").ExecuteUpdateAsync()), "mapped-join insertion did not error");
			await (t.CommitAsync());
			t = s.BeginTransaction();
			await (s.CreateQuery("delete Joiner").ExecuteUpdateAsync());
			await (s.CreateQuery("delete Vehicle").ExecuteUpdateAsync());
			await (t.CommitAsync());
			s.Close();
			await (data.CleanupAsync());
		}

		public async Task InsertWithGeneratedIdAsync()
		{
			// Make sure the env supports bulk inserts with generated ids...
			IEntityPersister persister = sessions.GetEntityPersister(typeof (PettingZoo).FullName);
			IIdentifierGenerator generator = persister.IdentifierGenerator;
			if (!HqlSqlWalker.SupportsIdGenWithBulkInsertion(generator))
			{
				return;
			}

			// create a Zoo
			var zoo = new Zoo{Name = "zoo"};
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			await (s.SaveAsync(zoo));
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			int count = await (s.CreateQuery("insert into PettingZoo (name) select name from Zoo").ExecuteUpdateAsync());
			await (t.CommitAsync());
			s.Close();
			Assert.That(count, Is.EqualTo(1), "unexpected insertion count");
			s = OpenSession();
			t = s.BeginTransaction();
			var pz = (PettingZoo)await (s.CreateQuery("from PettingZoo").UniqueResultAsync());
			await (t.CommitAsync());
			s.Close();
			Assert.That(zoo.Name, Is.EqualTo(pz.Name));
			Assert.That(zoo.Id != pz.Id);
			s = OpenSession();
			t = s.BeginTransaction();
			await (s.CreateQuery("delete Zoo").ExecuteUpdateAsync());
			await (t.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task InsertWithGeneratedVersionAndIdAsync()
		{
			// Make sure the env supports bulk inserts with generated ids...
			IEntityPersister persister = sessions.GetEntityPersister(typeof (IntegerVersioned).FullName);
			IIdentifierGenerator generator = persister.IdentifierGenerator;
			if (!HqlSqlWalker.SupportsIdGenWithBulkInsertion(generator))
			{
				return;
			}

			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			var entity = new IntegerVersioned{Name = "int-vers"};
			await (s.SaveAsync(entity));
			await (s.CreateQuery("select id, name, version from IntegerVersioned").ListAsync());
			await (t.CommitAsync());
			s.Close();
			long initialId = entity.Id;
			int initialVersion = entity.Version;
			s = OpenSession();
			t = s.BeginTransaction();
			int count = await (s.CreateQuery("insert into IntegerVersioned ( name, Data ) select name, Data from IntegerVersioned where id = :id").SetInt64("id", entity.Id).ExecuteUpdateAsync());
			await (t.CommitAsync());
			s.Close();
			Assert.That(count, Is.EqualTo(1), "unexpected insertion count");
			s = OpenSession();
			t = s.BeginTransaction();
			var created = (IntegerVersioned)await (s.CreateQuery("from IntegerVersioned where id <> :initialId").SetInt64("initialId", initialId).UniqueResultAsync());
			await (t.CommitAsync());
			s.Close();
			Assert.That(created.Version, Is.EqualTo(initialVersion), "version was not seeded");
			s = OpenSession();
			t = s.BeginTransaction();
			await (s.CreateQuery("delete IntegerVersioned").ExecuteUpdateAsync());
			await (t.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task InsertWithGeneratedTimestampVersionAsync()
		{
			// Make sure the env supports bulk inserts with generated ids...
			IEntityPersister persister = sessions.GetEntityPersister(typeof (TimestampVersioned).FullName);
			IIdentifierGenerator generator = persister.IdentifierGenerator;
			if (!HqlSqlWalker.SupportsIdGenWithBulkInsertion(generator))
			{
				return;
			}

			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			var entity = new TimestampVersioned{Name = "int-vers"};
			await (s.SaveAsync(entity));
			await (s.CreateQuery("select id, name, version from TimestampVersioned").ListAsync());
			await (t.CommitAsync());
			s.Close();
			long initialId = entity.Id;
			//Date initialVersion = entity.getVersion();
			s = OpenSession();
			t = s.BeginTransaction();
			int count = await (s.CreateQuery("insert into TimestampVersioned ( name, Data ) select name, Data from TimestampVersioned where id = :id").SetInt64("id", entity.Id).ExecuteUpdateAsync());
			await (t.CommitAsync());
			s.Close();
			Assert.That(count, Is.EqualTo(1), "unexpected insertion count");
			s = OpenSession();
			t = s.BeginTransaction();
			var created = (TimestampVersioned)await (s.CreateQuery("from TimestampVersioned where id <> :initialId").SetInt64("initialId", initialId).UniqueResultAsync());
			await (t.CommitAsync());
			s.Close();
			Assert.That(created.Version, Is.GreaterThan(DateTime.Today));
			s = OpenSession();
			t = s.BeginTransaction();
			await (s.CreateQuery("delete TimestampVersioned").ExecuteUpdateAsync());
			await (t.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task InsertWithSelectListUsingJoinsAsync()
		{
			// this is just checking parsing and syntax...
			ISession s = OpenSession();
			s.BeginTransaction();
			await (s.CreateQuery("insert into Animal (description, bodyWeight) select h.description, h.bodyWeight from Human h where h.mother.mother is not null").ExecuteUpdateAsync());
			await (s.CreateQuery("delete from Animal").ExecuteUpdateAsync());
			await (s.Transaction.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task UpdateWithWhereExistsSubqueryAsync()
		{
			// multi-table ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			var joe = new Human{Name = new Name{First = "Joe", Initial = 'Q', Last = "Public"}};
			await (s.SaveAsync(joe));
			var doll = new Human{Name = new Name{First = "Kyu", Initial = 'P', Last = "Doll"}, Friends = new[]{joe}};
			await (s.SaveAsync(doll));
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			string updateQryString = "update Human h " + "set h.description = 'updated' " + "where exists (" + "      select f.id " + "      from h.friends f " + "      where f.name.last = 'Public' " + ")";
			int count = await (s.CreateQuery(updateQryString).ExecuteUpdateAsync());
			Assert.That(count, Is.EqualTo(1));
			await (s.DeleteAsync(doll));
			await (s.DeleteAsync(joe));
			await (t.CommitAsync());
			s.Close();
			// single-table (one-to-many & many-to-many) ~~~~~~~~~~~~~~~~~~~~~~~~~~
			s = OpenSession();
			t = s.BeginTransaction();
			var entity = new SimpleEntityWithAssociation();
			var other = new SimpleEntityWithAssociation();
			entity.Name = "main";
			other.Name = "many-to-many-association";
			entity.ManyToManyAssociatedEntities.Add(other);
			entity.AddAssociation("one-to-many-association");
			await (s.SaveAsync(entity));
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			// one-to-many test
			updateQryString = "update SimpleEntityWithAssociation e set e.Name = 'updated' where " + "exists(select a.id from e.AssociatedEntities a " + "where a.Name = 'one-to-many-association')";
			count = await (s.CreateQuery(updateQryString).ExecuteUpdateAsync());
			Assert.That(count, Is.EqualTo(1));
			// many-to-many test
			if (Dialect.SupportsSubqueryOnMutatingTable)
			{
				updateQryString = "update SimpleEntityWithAssociation e set e.Name = 'updated' where " + "exists(select a.id from e.ManyToManyAssociatedEntities a " + "where a.Name = 'many-to-many-association')";
				count = await (s.CreateQuery(updateQryString).ExecuteUpdateAsync());
				Assert.That(count, Is.EqualTo(1));
			}

			IEnumerator mtm = entity.ManyToManyAssociatedEntities.GetEnumerator();
			mtm.MoveNext();
			await (s.DeleteAsync(mtm.Current));
			await (s.DeleteAsync(entity));
			await (t.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task IncrementCounterVersionAsync()
		{
			IntegerVersioned entity;
			using (ISession s = OpenSession())
				using (ITransaction t = s.BeginTransaction())
				{
					entity = new IntegerVersioned{Name = "int-vers", Data = "foo"};
					await (s.SaveAsync(entity));
					await (t.CommitAsync());
				}

			int initialVersion = entity.Version;
			using (ISession s = OpenSession())
			{
				using (ITransaction t = s.BeginTransaction())
				{
					// Note: Update more than one column to showcase NH-3624, which involved losing some columns. /2014-07-26
					int count = await (s.CreateQuery("update versioned IntegerVersioned set name = concat(name, 'upd'), Data = concat(Data, 'upd')").ExecuteUpdateAsync());
					Assert.That(count, Is.EqualTo(1), "incorrect exec count");
					await (t.CommitAsync());
				}

				using (ITransaction t = s.BeginTransaction())
				{
					entity = await (s.GetAsync<IntegerVersioned>(entity.Id));
					await (s.DeleteAsync(entity));
					await (t.CommitAsync());
				}
			}

			Assert.That(entity.Version, Is.EqualTo(initialVersion + 1), "version not incremented");
			Assert.That(entity.Name, Is.EqualTo("int-versupd"));
			Assert.That(entity.Data, Is.EqualTo("fooupd"));
		}

		[Test]
		public async Task IncrementTimestampVersionAsync()
		{
			TimestampVersioned entity;
			using (ISession s = OpenSession())
				using (ITransaction t = s.BeginTransaction())
				{
					entity = new TimestampVersioned{Name = "ts-vers", Data = "foo"};
					await (s.SaveAsync(entity));
					await (t.CommitAsync());
				}

			DateTime initialVersion = entity.Version;
			Thread.Sleep(1300);
			using (ISession s = OpenSession())
			{
				using (ITransaction t = s.BeginTransaction())
				{
					// Note: Update more than one column to showcase NH-3624, which involved losing some columns. /2014-07-26
					int count = await (s.CreateQuery("update versioned TimestampVersioned set name = concat(name, 'upd'), Data = concat(Data, 'upd')").ExecuteUpdateAsync());
					Assert.That(count, Is.EqualTo(1), "incorrect exec count");
					await (t.CommitAsync());
				}

				using (ITransaction t = s.BeginTransaction())
				{
					entity = await (s.LoadAsync<TimestampVersioned>(entity.Id));
					await (s.DeleteAsync(entity));
					await (t.CommitAsync());
				}
			}

			Assert.That(entity.Version, Is.GreaterThan(initialVersion), "version not incremented");
			Assert.That(entity.Name, Is.EqualTo("ts-versupd"));
			Assert.That(entity.Data, Is.EqualTo("fooupd"));
		}

		[Test]
		public async Task UpdateOnComponentAsync()
		{
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			var human = new Human{Name = new Name{First = "Stevee", Initial = 'X', Last = "Ebersole"}};
			await (s.SaveAsync(human));
			await (t.CommitAsync());
			string correctName = "Steve";
			t = s.BeginTransaction();
			int count = await (s.CreateQuery("update Human set name.first = :correction where id = :id").SetString("correction", correctName).SetInt64("id", human.Id).ExecuteUpdateAsync());
			Assert.That(count, Is.EqualTo(1), "incorrect update count");
			await (t.CommitAsync());
			t = s.BeginTransaction();
			await (s.RefreshAsync(human));
			Assert.That(human.Name.First, Is.EqualTo(correctName), "Update did not execute properly");
			await (s.CreateQuery("delete Human").ExecuteUpdateAsync());
			await (t.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task UpdateOnManyToOneAsync()
		{
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			await (s.CreateQuery("update Animal a set a.mother = null where a.id = 2").ExecuteUpdateAsync());
			if (!(Dialect is MySQLDialect))
			{
				// MySQL does not support (even un-correlated) subqueries against the update-mutating table
				await (s.CreateQuery("update Animal a set a.mother = (from Animal where id = 1) where a.id = 2").ExecuteUpdateAsync());
			}

			await (t.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task UpdateOnImplicitJoinFailsAsync()
		{
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			var human = new Human{Name = new Name{First = "Steve", Initial = 'E', Last = null}};
			var mother = new Human{Name = new Name{First = "Jane", Initial = 'E', Last = null}};
			human.Mother = (mother);
			await (s.SaveAsync(human));
			await (s.SaveAsync(mother));
			await (s.FlushAsync());
			await (t.CommitAsync());
			t = s.BeginTransaction();
			var e = Assert.ThrowsAsync<QueryException>(async () => await (s.CreateQuery("update Human set mother.name.initial = :initial").SetString("initial", "F").ExecuteUpdateAsync()));
			Assert.That(e.Message, Is.StringStarting("Implied join paths are not assignable in update"));
			await (s.CreateQuery("delete Human where mother is not null").ExecuteUpdateAsync());
			await (s.CreateQuery("delete Human").ExecuteUpdateAsync());
			await (t.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task UpdateOnDiscriminatorSubclassAsync()
		{
			var data = new TestData(this);
			await (data.PrepareAsync());
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			int count = await (s.CreateQuery("update PettingZoo set name = name").ExecuteUpdateAsync());
			Assert.That(count, Is.EqualTo(1), "Incorrect discrim subclass update count");
			t.Rollback();
			t = s.BeginTransaction();
			count = await (s.CreateQuery("update PettingZoo pz set pz.name = pz.name where pz.id = :id").SetInt64("id", data.PettingZoo.Id).ExecuteUpdateAsync());
			Assert.That(count, Is.EqualTo(1), "Incorrect discrim subclass update count");
			t.Rollback();
			t = s.BeginTransaction();
			count = await (s.CreateQuery("update Zoo as z set z.name = z.name").ExecuteUpdateAsync());
			Assert.That(count, Is.EqualTo(2), "Incorrect discrim subclass update count");
			t.Rollback();
			t = s.BeginTransaction();
			// TODO : not so sure this should be allowed.  Seems to me that if they specify an alias,
			// property-refs should be required to be qualified.
			count = await (s.CreateQuery("update Zoo as z set name = name where id = :id").SetInt64("id", data.Zoo.Id).ExecuteUpdateAsync());
			Assert.That(count, Is.EqualTo(1), "Incorrect discrim subclass update count");
			await (t.CommitAsync());
			s.Close();
			await (data.CleanupAsync());
		}

		[Test]
		public async Task UpdateOnAnimalAsync()
		{
			var data = new TestData(this);
			await (data.PrepareAsync());
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			int count = await (s.CreateQuery("update Animal set description = description where description = :desc").SetString("desc", data.Frog.Description).ExecuteUpdateAsync());
			Assert.That(count, Is.EqualTo(1), "Incorrect entity-updated count");
			count = await (s.CreateQuery("update Animal set description = :newDesc where description = :desc").SetString("desc", data.Polliwog.Description).SetString("newDesc", "Tadpole").ExecuteUpdateAsync());
			Assert.That(count, Is.EqualTo(1), "Incorrect entity-updated count");
			var tadpole = await (s.LoadAsync<Animal>(data.Polliwog.Id));
			Assert.That(tadpole.Description, Is.EqualTo("Tadpole"), "Update did not take effect");
			count = await (s.CreateQuery("update Animal set bodyWeight = bodyWeight + :w1 + :w2").SetSingle("w1", 1).SetSingle("w2", 2).ExecuteUpdateAsync());
			Assert.That(count, Is.EqualTo(6), "incorrect count on 'complex' update assignment");
			if (!(Dialect is MySQLDialect))
			{
				// MySQL does not support (even un-correlated) subqueries against the update-mutating table
				await (s.CreateQuery("update Animal set bodyWeight = ( select max(bodyWeight) from Animal )").ExecuteUpdateAsync());
			}

			await (t.CommitAsync());
			s.Close();
			await (data.CleanupAsync());
		}

		[Test]
		public async Task UpdateMultiplePropertyOnAnimalAsync()
		{
			var data = new TestData(this);
			await (data.PrepareAsync());
			using (ISession s = OpenSession())
				using (ITransaction t = s.BeginTransaction())
				{
					int count = await (s.CreateQuery("update Animal set description = :newDesc, bodyWeight = :w1 where description = :desc").SetString("desc", data.Polliwog.Description).SetString("newDesc", "Tadpole").SetSingle("w1", 3).ExecuteUpdateAsync());
					Assert.That(count, Is.EqualTo(1));
					await (t.CommitAsync());
				}

			using (ISession s = OpenSession())
				using (s.BeginTransaction())
				{
					var tadpole = await (s.GetAsync<Animal>(data.Polliwog.Id));
					Assert.That(tadpole.Description, Is.EqualTo("Tadpole"));
					Assert.That(tadpole.BodyWeight, Is.EqualTo(3));
				}

			await (data.CleanupAsync());
		}

		[Test]
		public async Task UpdateOnMammalAsync()
		{
			var data = new TestData(this);
			await (data.PrepareAsync());
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			int count = await (s.CreateQuery("update Mammal set description = description").ExecuteUpdateAsync());
			Assert.That(count, Is.EqualTo(2), "incorrect update count against 'middle' of joined-subclass hierarchy");
			count = await (s.CreateQuery("update Mammal set bodyWeight = 25").ExecuteUpdateAsync());
			Assert.That(count, Is.EqualTo(2), "incorrect update count against 'middle' of joined-subclass hierarchy");
			if (!(Dialect is MySQLDialect))
			{
				// MySQL does not support (even un-correlated) subqueries against the update-mutating table
				count = await (s.CreateQuery("update Mammal set bodyWeight = ( select max(bodyWeight) from Animal )").ExecuteUpdateAsync());
				Assert.That(count, Is.EqualTo(2), "incorrect update count against 'middle' of joined-subclass hierarchy");
			}

			await (t.CommitAsync());
			s.Close();
			await (data.CleanupAsync());
		}

		[Test]
		public async Task UpdateSetNullUnionSubclassAsync()
		{
			var data = new TestData(this);
			await (data.PrepareAsync());
			// These should reach out into *all* subclass tables...
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			int count = await (s.CreateQuery("update Vehicle set Owner = 'Steve'").ExecuteUpdateAsync());
			Assert.That(count, Is.EqualTo(4), "incorrect restricted update count");
			count = await (s.CreateQuery("update Vehicle set Owner = null where Owner = 'Steve'").ExecuteUpdateAsync());
			Assert.That(count, Is.EqualTo(4), "incorrect restricted update count");
			count = await (s.CreateQuery("delete Vehicle where Owner is null").ExecuteUpdateAsync());
			Assert.That(count, Is.EqualTo(4), "incorrect restricted update count");
			await (t.CommitAsync());
			s.Close();
			await (data.CleanupAsync());
		}

		[Test]
		public async Task WrongPropertyNameThrowQueryExceptionAsync()
		{
			using (ISession s = OpenSession())
			{
				var e = Assert.ThrowsAsync<QueryException>(async () => await (s.CreateQuery("update Vehicle set owner = null where owner = 'Steve'").ExecuteUpdateAsync()));
				Assert.That(e.Message, Is.StringStarting("Left side of assigment should be a case sensitive property or a field"));
			}
		}

		[Test]
		public async Task UpdateSetNullOnDiscriminatorSubclassAsync()
		{
			var data = new TestData(this);
			await (data.PrepareAsync());
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			int count = await (s.CreateQuery("update PettingZoo set address.city = null").ExecuteUpdateAsync());
			Assert.That(count, Is.EqualTo(1), "Incorrect discrim subclass delete count");
			count = await (s.CreateQuery("delete Zoo where address.city is null").ExecuteUpdateAsync());
			Assert.That(count, Is.EqualTo(1), "Incorrect discrim subclass delete count");
			count = await (s.CreateQuery("update Zoo set address.city = null").ExecuteUpdateAsync());
			Assert.That(count, Is.EqualTo(1), "Incorrect discrim subclass delete count");
			count = await (s.CreateQuery("delete Zoo where address.city is null").ExecuteUpdateAsync());
			Assert.That(count, Is.EqualTo(1), "Incorrect discrim subclass delete count");
			await (t.CommitAsync());
			s.Close();
			await (data.CleanupAsync());
		}

		[Test]
		public async Task UpdateSetNullOnJoinedSubclassAsync()
		{
			var data = new TestData(this);
			await (data.PrepareAsync());
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			int count = await (s.CreateQuery("update Mammal set bodyWeight = null").ExecuteUpdateAsync());
			Assert.That(count, Is.EqualTo(2), "Incorrect deletion count on joined subclass");
			count = await (s.CreateQuery("delete Animal where bodyWeight = null").ExecuteUpdateAsync());
			Assert.That(count, Is.EqualTo(2), "Incorrect deletion count on joined subclass");
			await (t.CommitAsync());
			s.Close();
			await (data.CleanupAsync());
		}

		[Test]
		public async Task DeleteWithSubqueryAsync()
		{
			// setup the test data...
			ISession s = OpenSession();
			s.BeginTransaction();
			var owner = new SimpleEntityWithAssociation{Name = "myEntity-1"};
			owner.AddAssociation("assoc-1");
			owner.AddAssociation("assoc-2");
			owner.AddAssociation("assoc-3");
			await (s.SaveAsync(owner));
			var owner2 = new SimpleEntityWithAssociation{Name = "myEntity-2"};
			owner2.AddAssociation("assoc-1");
			owner2.AddAssociation("assoc-2");
			owner2.AddAssociation("assoc-3");
			owner2.AddAssociation("assoc-4");
			await (s.SaveAsync(owner2));
			var owner3 = new SimpleEntityWithAssociation{Name = "myEntity-3"};
			await (s.SaveAsync(owner3));
			await (s.Transaction.CommitAsync());
			s.Close();
			// now try the bulk delete
			s = OpenSession();
			s.BeginTransaction();
			int count = await (s.CreateQuery("delete SimpleEntityWithAssociation e where size(e.AssociatedEntities ) = 0 and e.Name like '%'").ExecuteUpdateAsync());
			Assert.That(count, Is.EqualTo(1), "Incorrect delete count");
			await (s.Transaction.CommitAsync());
			s.Close();
			// finally, clean up
			s = OpenSession();
			s.BeginTransaction();
			await (s.CreateQuery("delete SimpleAssociatedEntity").ExecuteUpdateAsync());
			await (s.CreateQuery("delete SimpleEntityWithAssociation").ExecuteUpdateAsync());
			await (s.Transaction.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task SimpleDeleteOnAnimalAsync()
		{
			if (Dialect.HasSelfReferentialForeignKeyBug)
			{
				Assert.Ignore("self referential FK bug", "HQL delete testing");
				return;
			}

			var data = new TestData(this);
			await (data.PrepareAsync());
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			int count = await (s.CreateQuery("delete from Animal as a where a.id = :id").SetInt64("id", data.Polliwog.Id).ExecuteUpdateAsync());
			Assert.That(count, Is.EqualTo(1), "Incorrect delete count");
			count = await (s.CreateQuery("delete Animal where id = :id").SetInt64("id", data.Catepillar.Id).ExecuteUpdateAsync());
			Assert.That(count, Is.EqualTo(1), "Incorrect delete count");
			// HHH-873...
			if (Dialect.SupportsSubqueryOnMutatingTable)
			{
				count = await (s.CreateQuery("delete from User u where u not in (select u from User u)").ExecuteUpdateAsync());
				Assert.That(count, Is.EqualTo(0));
			}

			count = await (s.CreateQuery("delete Animal a").ExecuteUpdateAsync());
			Assert.That(count, Is.EqualTo(4), "Incorrect delete count");
			IList list = await (s.CreateQuery("select a from Animal as a").ListAsync());
			Assert.That(list, Is.Empty, "table not empty");
			await (t.CommitAsync());
			s.Close();
			await (data.CleanupAsync());
		}

		[Test]
		public async Task DeleteOnDiscriminatorSubclassAsync()
		{
			var data = new TestData(this);
			await (data.PrepareAsync());
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			int count = await (s.CreateQuery("delete PettingZoo").ExecuteUpdateAsync());
			Assert.That(count, Is.EqualTo(1), "Incorrect discrim subclass delete count");
			count = await (s.CreateQuery("delete Zoo").ExecuteUpdateAsync());
			Assert.That(count, Is.EqualTo(1), "Incorrect discrim subclass delete count");
			await (t.CommitAsync());
			s.Close();
			await (data.CleanupAsync());
		}

		[Test]
		public async Task DeleteOnJoinedSubclassAsync()
		{
			var data = new TestData(this);
			await (data.PrepareAsync());
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			int count = await (s.CreateQuery("delete Mammal where bodyWeight > 150").ExecuteUpdateAsync());
			Assert.That(count, Is.EqualTo(1), "Incorrect deletion count on joined subclass");
			count = await (s.CreateQuery("delete Mammal").ExecuteUpdateAsync());
			Assert.That(count, Is.EqualTo(1), "Incorrect deletion count on joined subclass");
			count = await (s.CreateQuery("delete SubMulti").ExecuteUpdateAsync());
			Assert.That(count, Is.EqualTo(0), "Incorrect deletion count on joined subclass");
			await (t.CommitAsync());
			s.Close();
			await (data.CleanupAsync());
		}

		[Test]
		public async Task DeleteOnMappedJoinAsync()
		{
			var data = new TestData(this);
			await (data.PrepareAsync());
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			int count = await (s.CreateQuery("delete Joiner where joinedName = :joinedName").SetString("joinedName", "joined-name").ExecuteUpdateAsync());
			Assert.That(count, Is.EqualTo(1), "Incorrect deletion count on joined class");
			await (t.CommitAsync());
			s.Close();
			await (data.CleanupAsync());
		}

		[Test]
		public async Task DeleteUnionSubclassAbstractRootAsync()
		{
			var data = new TestData(this);
			await (data.PrepareAsync());
			// These should reach out into *all* subclass tables...
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			int count = await (s.CreateQuery("delete Vehicle where Owner = :owner").SetString("owner", "Steve").ExecuteUpdateAsync());
			Assert.That(count, Is.EqualTo(1), "incorrect restricted update count");
			count = await (s.CreateQuery("delete Vehicle").ExecuteUpdateAsync());
			Assert.That(count, Is.EqualTo(3), "incorrect update count");
			await (t.CommitAsync());
			s.Close();
			await (data.CleanupAsync());
		}

		[Test]
		public async Task DeleteUnionSubclassConcreteSubclassAsync()
		{
			var data = new TestData(this);
			await (data.PrepareAsync());
			// These should only affect the given table
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			int count = await (s.CreateQuery("delete Truck where Owner = :owner").SetString("owner", "Steve").ExecuteUpdateAsync());
			Assert.That(count, Is.EqualTo(1), "incorrect restricted update count");
			count = await (s.CreateQuery("delete Truck").ExecuteUpdateAsync());
			Assert.That(count, Is.EqualTo(2), "incorrect update count");
			await (t.CommitAsync());
			s.Close();
			await (data.CleanupAsync());
		}

		[Test]
		public async Task DeleteUnionSubclassLeafSubclassAsync()
		{
			var data = new TestData(this);
			await (data.PrepareAsync());
			// These should only affect the given table
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			int count = await (s.CreateQuery("delete Car where Owner = :owner").SetString("owner", "Kirsten").ExecuteUpdateAsync());
			Assert.That(count, Is.EqualTo(1), "incorrect restricted update count");
			count = await (s.CreateQuery("delete Car").ExecuteUpdateAsync());
			Assert.That(count, Is.EqualTo(0), "incorrect update count");
			await (t.CommitAsync());
			s.Close();
			await (data.CleanupAsync());
		}

		[Test]
		public async Task DeleteRestrictedOnManyToOneAsync()
		{
			var data = new TestData(this);
			await (data.PrepareAsync());
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			int count = await ((await (s.CreateQuery("delete Animal where mother = :mother").SetEntityAsync("mother", data.Butterfly))).ExecuteUpdateAsync());
			Assert.That(count, Is.EqualTo(1));
			await (t.CommitAsync());
			s.Close();
			await (data.CleanupAsync());
		}

		[Test]
		public async Task DeleteSyntaxWithCompositeIdAsync()
		{
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			await (s.CreateQuery("delete EntityWithCrazyCompositeKey where Id.Id = 1 and Id.OtherId = 2").ExecuteUpdateAsync());
			await (s.CreateQuery("delete from EntityWithCrazyCompositeKey where Id.Id = 1 and Id.OtherId = 2").ExecuteUpdateAsync());
			await (s.CreateQuery("delete from EntityWithCrazyCompositeKey e where e.Id.Id = 1 and e.Id.OtherId = 2").ExecuteUpdateAsync());
			await (t.CommitAsync());
			s.Close();
		}

		[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
		private partial class TestData
		{
			private readonly BulkManipulationAsync tc;
			public Animal Polliwog;
			public Animal Catepillar;
			public Animal Frog;
			public Animal Butterfly;
			public Zoo Zoo;
			public Zoo PettingZoo;
			public TestData(BulkManipulationAsync tc)
			{
				this.tc = tc;
			}

			public async Task PrepareAsync()
			{
				ISession s = tc.OpenNewSession();
				ITransaction txn = s.BeginTransaction();
				Polliwog = new Animal{BodyWeight = 12, Description = "Polliwog"};
				Catepillar = new Animal{BodyWeight = 10, Description = "Catepillar"};
				Frog = new Animal{BodyWeight = 34, Description = "Frog"};
				Polliwog.Father = Frog;
				Frog.AddOffspring(Polliwog);
				Butterfly = new Animal{BodyWeight = 9, Description = "Butterfly"};
				Catepillar.Mother = Butterfly;
				Butterfly.AddOffspring(Catepillar);
				await (s.SaveAsync(Frog));
				await (s.SaveAsync(Polliwog));
				await (s.SaveAsync(Butterfly));
				await (s.SaveAsync(Catepillar));
				var dog = new Dog{BodyWeight = 200, Description = "dog"};
				await (s.SaveAsync(dog));
				var cat = new Cat{BodyWeight = 100, Description = "cat"};
				await (s.SaveAsync(cat));
				Zoo = new Zoo{Name = "Zoo"};
				var add = new Address{City = "MEL", Country = "AU", Street = "Main st", PostalCode = "3000"};
				Zoo.Address = add;
				PettingZoo = new PettingZoo{Name = "Petting Zoo"};
				var addr = new Address{City = "Sydney", Country = "AU", Street = "High st", PostalCode = "2000"};
				PettingZoo.Address = addr;
				await (s.SaveAsync(Zoo));
				await (s.SaveAsync(PettingZoo));
				var joiner = new Joiner{JoinedName = "joined-name", Name = "name"};
				await (s.SaveAsync(joiner));
				var car = new Car{Vin = "123c", Owner = "Kirsten"};
				await (s.SaveAsync(car));
				var truck = new Truck{Vin = "123t", Owner = "Steve"};
				await (s.SaveAsync(truck));
				var suv = new SUV{Vin = "123s", Owner = "Joe"};
				await (s.SaveAsync(suv));
				var pickup = new Pickup{Vin = "123p", Owner = "Cecelia"};
				await (s.SaveAsync(pickup));
				var b = new BooleanLiteralEntity();
				await (s.SaveAsync(b));
				await (txn.CommitAsync());
				s.Close();
			}

			public async Task CleanupAsync()
			{
				ISession s = tc.OpenNewSession();
				ITransaction txn = s.BeginTransaction();
				// workaround awesome HSQLDB "feature"
				await (s.CreateQuery("delete from Animal where mother is not null or father is not null").ExecuteUpdateAsync());
				await (s.CreateQuery("delete from Animal").ExecuteUpdateAsync());
				await (s.CreateQuery("delete from Zoo").ExecuteUpdateAsync());
				await (s.CreateQuery("delete from Joiner").ExecuteUpdateAsync());
				await (s.CreateQuery("delete from Vehicle").ExecuteUpdateAsync());
				await (s.CreateQuery("delete from BooleanLiteralEntity").ExecuteUpdateAsync());
				await (txn.CommitAsync());
				s.Close();
			}
		}
	}
}
#endif