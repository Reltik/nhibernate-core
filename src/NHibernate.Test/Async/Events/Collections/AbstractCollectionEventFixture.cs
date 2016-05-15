#if NET_4_5
using System.Collections;
using System.Collections.Generic;
using NHibernate.Collection;
using NHibernate.Collection.Generic;
using NHibernate.Event;
using NHibernate.Test.Events.Collections.Association.Bidirectional.ManyToMany;
using NUnit.Framework;
using System.Threading.Tasks;

namespace NHibernate.Test.Events.Collections
{
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public abstract partial class AbstractCollectionEventFixture : TestCase
	{
		[Test]
		public async Task SaveParentEmptyChildrenAsync()
		{
			CollectionListeners listeners = new CollectionListeners(sessions);
			IParentWithCollection parent = await (CreateParentWithNoChildrenAsync("parent"));
			Assert.That(parent.Children.Count, Is.EqualTo(0));
			int index = 0;
			CheckResult(listeners, listeners.PreCollectionRecreate, parent, index++);
			CheckResult(listeners, listeners.PostCollectionRecreate, parent, index++);
			CheckNumberOfResults(listeners, index);
			listeners.Clear();
			using (ISession s = OpenSession())
			{
				using (ITransaction tx = s.BeginTransaction())
				{
					parent = (IParentWithCollection)await (s.GetAsync(parent.GetType(), parent.Id));
					await (tx.CommitAsync());
				}
			}

			Assert.That(parent.Children, Is.Not.Null);
			CheckNumberOfResults(listeners, 0);
		}

		[Test]
		public virtual async Task SaveParentOneChildAsync()
		{
			CollectionListeners listeners = new CollectionListeners(sessions);
			IParentWithCollection parent = await (CreateParentWithOneChildAsync("parent", "child"));
			int index = 0;
			CheckResult(listeners, listeners.PreCollectionRecreate, parent, index++);
			CheckResult(listeners, listeners.PostCollectionRecreate, parent, index++);
			ChildWithBidirectionalManyToMany child = GetFirstChild(parent.Children) as ChildWithBidirectionalManyToMany;
			if (child != null)
			{
				CheckResult(listeners, listeners.PreCollectionRecreate, child, index++);
				CheckResult(listeners, listeners.PostCollectionRecreate, child, index++);
			}

			CheckNumberOfResults(listeners, index);
		}

		[Test]
		public async Task UpdateParentNullToOneChildAsync()
		{
			CollectionListeners listeners = new CollectionListeners(sessions);
			IParentWithCollection parent = await (CreateParentWithNullChildrenAsync("parent"));
			listeners.Clear();
			Assert.That(parent.Children, Is.Null);
			ISession s = OpenSession();
			ITransaction tx = s.BeginTransaction();
			parent = (IParentWithCollection)await (s.GetAsync(parent.GetType(), parent.Id));
			Assert.That(parent.Children, Is.Not.Null);
			ChildWithBidirectionalManyToMany newChild = parent.AddChild("new") as ChildWithBidirectionalManyToMany;
			await (tx.CommitAsync());
			s.Close();
			int index = 0;
			if (((IPersistentCollection)parent.Children).WasInitialized)
			{
				CheckResult(listeners, listeners.InitializeCollection, parent, index++);
			}

			CheckResult(listeners, listeners.PreCollectionUpdate, parent, index++);
			CheckResult(listeners, listeners.PostCollectionUpdate, parent, index++);
			if (newChild != null)
			{
				CheckResult(listeners, listeners.PreCollectionRecreate, newChild, index++);
				CheckResult(listeners, listeners.PostCollectionRecreate, newChild, index++);
			}

			CheckNumberOfResults(listeners, index);
		}

		[Test]
		public async Task UpdateParentNoneToOneChildAsync()
		{
			CollectionListeners listeners = new CollectionListeners(sessions);
			IParentWithCollection parent = await (CreateParentWithNoChildrenAsync("parent"));
			listeners.Clear();
			Assert.That(parent.Children.Count, Is.EqualTo(0));
			ISession s = OpenSession();
			ITransaction tx = s.BeginTransaction();
			parent = (IParentWithCollection)await (s.GetAsync(parent.GetType(), parent.Id));
			ChildWithBidirectionalManyToMany newChild = parent.AddChild("new") as ChildWithBidirectionalManyToMany;
			await (tx.CommitAsync());
			s.Close();
			int index = 0;
			if (((IPersistentCollection)parent.Children).WasInitialized)
			{
				CheckResult(listeners, listeners.InitializeCollection, parent, index++);
			}

			CheckResult(listeners, listeners.PreCollectionUpdate, parent, index++);
			CheckResult(listeners, listeners.PostCollectionUpdate, parent, index++);
			if (newChild != null)
			{
				CheckResult(listeners, listeners.PreCollectionRecreate, newChild, index++);
				CheckResult(listeners, listeners.PostCollectionRecreate, newChild, index++);
			}

			CheckNumberOfResults(listeners, index);
		}

		[Test]
		public async Task UpdateParentOneToTwoChildrenAsync()
		{
			CollectionListeners listeners = new CollectionListeners(sessions);
			IParentWithCollection parent = await (CreateParentWithOneChildAsync("parent", "child"));
			Assert.That(parent.Children.Count, Is.EqualTo(1));
			listeners.Clear();
			ISession s = OpenSession();
			ITransaction tx = s.BeginTransaction();
			parent = (IParentWithCollection)await (s.GetAsync(parent.GetType(), parent.Id));
			ChildWithBidirectionalManyToMany newChild = parent.AddChild("new2") as ChildWithBidirectionalManyToMany;
			await (tx.CommitAsync());
			s.Close();
			int index = 0;
			if (((IPersistentCollection)parent.Children).WasInitialized)
			{
				CheckResult(listeners, listeners.InitializeCollection, parent, index++);
			}

			CheckResult(listeners, listeners.PreCollectionUpdate, parent, index++);
			CheckResult(listeners, listeners.PostCollectionUpdate, parent, index++);
			if (newChild != null)
			{
				CheckResult(listeners, listeners.PreCollectionRecreate, newChild, index++);
				CheckResult(listeners, listeners.PostCollectionRecreate, newChild, index++);
			}

			CheckNumberOfResults(listeners, index);
		}

		[Test]
		public virtual async Task UpdateParentOneToTwoSameChildrenAsync()
		{
			CollectionListeners listeners = new CollectionListeners(sessions);
			IParentWithCollection parent = await (CreateParentWithOneChildAsync("parent", "child"));
			IChild child = GetFirstChild(parent.Children);
			Assert.That(parent.Children.Count, Is.EqualTo(1));
			listeners.Clear();
			using (ISession s = OpenSession())
				using (ITransaction tx = s.BeginTransaction())
				{
					parent = (IParentWithCollection)await (s.GetAsync(parent.GetType(), parent.Id));
					IEntity e = child as IEntity;
					if (e != null)
					{
						child = (IChild)await (s.GetAsync(child.GetType(), e.Id));
					}

					parent.AddChild(child);
					await (tx.CommitAsync());
				}

			int index = 0;
			if (((IPersistentCollection)parent.Children).WasInitialized)
			{
				CheckResult(listeners, listeners.InitializeCollection, parent, index++);
			}

			ChildWithBidirectionalManyToMany childWithManyToMany = child as ChildWithBidirectionalManyToMany;
			if (childWithManyToMany != null)
			{
				if (((IPersistentCollection)childWithManyToMany.Parents).WasInitialized)
				{
					CheckResult(listeners, listeners.InitializeCollection, childWithManyToMany, index++);
				}
			}

			if (!(parent.Children is PersistentGenericSet<IChild>))
			{
				CheckResult(listeners, listeners.PreCollectionUpdate, parent, index++);
				CheckResult(listeners, listeners.PostCollectionUpdate, parent, index++);
			}

			if (childWithManyToMany != null && !(childWithManyToMany.Parents is PersistentGenericSet<ParentWithBidirectionalManyToMany>))
			{
				CheckResult(listeners, listeners.PreCollectionUpdate, childWithManyToMany, index++);
				CheckResult(listeners, listeners.PostCollectionUpdate, childWithManyToMany, index++);
			}

			CheckNumberOfResults(listeners, index);
		}

		[Test]
		public async Task UpdateParentNullToOneChildDiffCollectionAsync()
		{
			CollectionListeners listeners = new CollectionListeners(sessions);
			IParentWithCollection parent = await (CreateParentWithNullChildrenAsync("parent"));
			listeners.Clear();
			Assert.That(parent.Children, Is.Null);
			ISession s = OpenSession();
			ITransaction tx = s.BeginTransaction();
			parent = (IParentWithCollection)await (s.GetAsync(parent.GetType(), parent.Id));
			ICollection<IChild> collectionOrig = parent.Children;
			parent.NewChildren(CreateCollection());
			ChildWithBidirectionalManyToMany newChild = parent.AddChild("new") as ChildWithBidirectionalManyToMany;
			await (tx.CommitAsync());
			s.Close();
			int index = 0;
			if (((IPersistentCollection)collectionOrig).WasInitialized)
			{
				CheckResult(listeners, listeners.InitializeCollection, parent, collectionOrig, index++);
			}

			CheckResult(listeners, listeners.PreCollectionRemove, parent, collectionOrig, index++);
			CheckResult(listeners, listeners.PostCollectionRemove, parent, collectionOrig, index++);
			if (newChild != null)
			{
				CheckResult(listeners, listeners.PreCollectionRecreate, newChild, index++);
				CheckResult(listeners, listeners.PostCollectionRecreate, newChild, index++);
			}

			CheckResult(listeners, listeners.PreCollectionRecreate, parent, index++);
			CheckResult(listeners, listeners.PostCollectionRecreate, parent, index++);
			CheckNumberOfResults(listeners, index);
		}

		[Test]
		public async Task UpdateParentNoneToOneChildDiffCollectionAsync()
		{
			CollectionListeners listeners = new CollectionListeners(sessions);
			IParentWithCollection parent = await (CreateParentWithNoChildrenAsync("parent"));
			listeners.Clear();
			Assert.That(parent.Children.Count, Is.EqualTo(0));
			ISession s = OpenSession();
			ITransaction tx = s.BeginTransaction();
			parent = (IParentWithCollection)await (s.GetAsync(parent.GetType(), parent.Id));
			ICollection<IChild> oldCollection = parent.Children;
			parent.NewChildren(CreateCollection());
			ChildWithBidirectionalManyToMany newChild = parent.AddChild("new") as ChildWithBidirectionalManyToMany;
			await (tx.CommitAsync());
			s.Close();
			int index = 0;
			if (((IPersistentCollection)oldCollection).WasInitialized)
			{
				CheckResult(listeners, listeners.InitializeCollection, parent, oldCollection, index++);
			}

			CheckResult(listeners, listeners.PreCollectionRemove, parent, oldCollection, index++);
			CheckResult(listeners, listeners.PostCollectionRemove, parent, oldCollection, index++);
			if (newChild != null)
			{
				CheckResult(listeners, listeners.PreCollectionRecreate, newChild, index++);
				CheckResult(listeners, listeners.PostCollectionRecreate, newChild, index++);
			}

			CheckResult(listeners, listeners.PreCollectionRecreate, parent, index++);
			CheckResult(listeners, listeners.PostCollectionRecreate, parent, index++);
			CheckNumberOfResults(listeners, index);
		}

		[Test]
		public async Task UpdateParentOneChildDiffCollectionSameChildAsync()
		{
			CollectionListeners listeners = new CollectionListeners(sessions);
			IParentWithCollection parent = await (CreateParentWithOneChildAsync("parent", "child"));
			IChild child = GetFirstChild(parent.Children);
			listeners.Clear();
			Assert.That(parent.Children.Count, Is.EqualTo(1));
			ISession s = OpenSession();
			ITransaction tx = s.BeginTransaction();
			parent = (IParentWithCollection)await (s.GetAsync(parent.GetType(), parent.Id));
			IEntity e = child as IEntity;
			if (e != null)
			{
				child = (IChild)await (s.GetAsync(child.GetType(), e.Id));
			}

			ICollection<IChild> oldCollection = parent.Children;
			parent.NewChildren(CreateCollection());
			parent.AddChild(child);
			await (tx.CommitAsync());
			s.Close();
			int index = 0;
			if (((IPersistentCollection)oldCollection).WasInitialized)
			{
				CheckResult(listeners, listeners.InitializeCollection, parent, oldCollection, index++);
			}

			ChildWithBidirectionalManyToMany childWithManyToMany = child as ChildWithBidirectionalManyToMany;
			if (childWithManyToMany != null)
			{
				if (((IPersistentCollection)childWithManyToMany.Parents).WasInitialized)
				{
					CheckResult(listeners, listeners.InitializeCollection, childWithManyToMany, index++);
				}
			}

			CheckResult(listeners, listeners.PreCollectionRemove, parent, oldCollection, index++);
			CheckResult(listeners, listeners.PostCollectionRemove, parent, oldCollection, index++);
			if (childWithManyToMany != null)
			{
				// hmmm, the same parent was removed and re-added to the child's collection;
				// should this be considered an update?
				CheckResult(listeners, listeners.PreCollectionUpdate, childWithManyToMany, index++);
				CheckResult(listeners, listeners.PostCollectionUpdate, childWithManyToMany, index++);
			}

			CheckResult(listeners, listeners.PreCollectionRecreate, parent, index++);
			CheckResult(listeners, listeners.PostCollectionRecreate, parent, index++);
			CheckNumberOfResults(listeners, index);
		}

		[Test]
		public async Task UpdateParentOneChildDiffCollectionDiffChildAsync()
		{
			CollectionListeners listeners = new CollectionListeners(sessions);
			IParentWithCollection parent = await (CreateParentWithOneChildAsync("parent", "child"));
			IChild oldChild = GetFirstChild(parent.Children);
			listeners.Clear();
			Assert.That(parent.Children.Count, Is.EqualTo(1));
			ISession s = OpenSession();
			ITransaction tx = s.BeginTransaction();
			parent = (IParentWithCollection)await (s.GetAsync(parent.GetType(), parent.Id));
			IEntity e = oldChild as IEntity;
			ChildWithBidirectionalManyToMany oldChildWithManyToMany = null;
			if (e != null)
			{
				oldChildWithManyToMany = await (s.GetAsync(oldChild.GetType(), e.Id)) as ChildWithBidirectionalManyToMany;
			}

			ICollection<IChild> oldCollection = parent.Children;
			parent.NewChildren(CreateCollection());
			IChild newChild = parent.AddChild("new1");
			await (tx.CommitAsync());
			s.Close();
			int index = 0;
			if (((IPersistentCollection)oldCollection).WasInitialized)
			{
				CheckResult(listeners, listeners.InitializeCollection, parent, oldCollection, index++);
			}

			if (oldChildWithManyToMany != null)
			{
				if (((IPersistentCollection)oldChildWithManyToMany.Parents).WasInitialized)
				{
					CheckResult(listeners, listeners.InitializeCollection, oldChildWithManyToMany, index++);
				}
			}

			CheckResult(listeners, listeners.PreCollectionRemove, parent, oldCollection, index++);
			CheckResult(listeners, listeners.PostCollectionRemove, parent, oldCollection, index++);
			if (oldChildWithManyToMany != null)
			{
				CheckResult(listeners, listeners.PreCollectionUpdate, oldChildWithManyToMany, index++);
				CheckResult(listeners, listeners.PostCollectionUpdate, oldChildWithManyToMany, index++);
				CheckResult(listeners, listeners.PreCollectionRecreate, (ChildWithBidirectionalManyToMany)newChild, index++);
				CheckResult(listeners, listeners.PostCollectionRecreate, (ChildWithBidirectionalManyToMany)newChild, index++);
			}

			CheckResult(listeners, listeners.PreCollectionRecreate, parent, index++);
			CheckResult(listeners, listeners.PostCollectionRecreate, parent, index++);
			CheckNumberOfResults(listeners, index);
		}

		[Test]
		public async Task UpdateParentOneChildToNoneByRemoveAsync()
		{
			CollectionListeners listeners = new CollectionListeners(sessions);
			IParentWithCollection parent = await (CreateParentWithOneChildAsync("parent", "child"));
			Assert.That(parent.Children.Count, Is.EqualTo(1));
			IChild child = GetFirstChild(parent.Children);
			listeners.Clear();
			ISession s = OpenSession();
			ITransaction tx = s.BeginTransaction();
			parent = (IParentWithCollection)await (s.GetAsync(parent.GetType(), parent.Id));
			IEntity e = child as IEntity;
			if (e != null)
			{
				child = (IChild)await (s.GetAsync(child.GetType(), e.Id));
			}

			parent.RemoveChild(child);
			await (tx.CommitAsync());
			s.Close();
			int index = 0;
			if (((IPersistentCollection)parent.Children).WasInitialized)
			{
				CheckResult(listeners, listeners.InitializeCollection, parent, index++);
			}

			ChildWithBidirectionalManyToMany childWithManyToMany = child as ChildWithBidirectionalManyToMany;
			if (childWithManyToMany != null)
			{
				if (((IPersistentCollection)childWithManyToMany.Parents).WasInitialized)
				{
					CheckResult(listeners, listeners.InitializeCollection, childWithManyToMany, index++);
				}
			}

			CheckResult(listeners, listeners.PreCollectionUpdate, parent, index++);
			CheckResult(listeners, listeners.PostCollectionUpdate, parent, index++);
			if (childWithManyToMany != null)
			{
				CheckResult(listeners, listeners.PreCollectionUpdate, childWithManyToMany, index++);
				CheckResult(listeners, listeners.PostCollectionUpdate, childWithManyToMany, index++);
			}

			CheckNumberOfResults(listeners, index);
		}

		[Test]
		public async Task UpdateParentOneChildToNoneByClearAsync()
		{
			CollectionListeners listeners = new CollectionListeners(sessions);
			IParentWithCollection parent = await (CreateParentWithOneChildAsync("parent", "child"));
			Assert.That(parent.Children.Count, Is.EqualTo(1));
			IChild child = GetFirstChild(parent.Children);
			listeners.Clear();
			ISession s = OpenSession();
			ITransaction tx = s.BeginTransaction();
			parent = (IParentWithCollection)await (s.GetAsync(parent.GetType(), parent.Id));
			IEntity e = child as IEntity;
			if (e != null)
			{
				child = (IChild)await (s.GetAsync(child.GetType(), e.Id));
			}

			parent.ClearChildren();
			await (tx.CommitAsync());
			s.Close();
			int index = 0;
			if (((IPersistentCollection)parent.Children).WasInitialized)
			{
				CheckResult(listeners, listeners.InitializeCollection, parent, index++);
			}

			ChildWithBidirectionalManyToMany childWithManyToMany = child as ChildWithBidirectionalManyToMany;
			if (childWithManyToMany != null)
			{
				if (((IPersistentCollection)childWithManyToMany.Parents).WasInitialized)
				{
					CheckResult(listeners, listeners.InitializeCollection, childWithManyToMany, index++);
				}
			}

			CheckResult(listeners, listeners.PreCollectionUpdate, parent, index++);
			CheckResult(listeners, listeners.PostCollectionUpdate, parent, index++);
			if (childWithManyToMany != null)
			{
				CheckResult(listeners, listeners.PreCollectionUpdate, childWithManyToMany, index++);
				CheckResult(listeners, listeners.PostCollectionUpdate, childWithManyToMany, index++);
			}

			CheckNumberOfResults(listeners, index);
		}

		[Test]
		public async Task UpdateParentTwoChildrenToOneAsync()
		{
			CollectionListeners listeners = new CollectionListeners(sessions);
			IParentWithCollection parent = await (CreateParentWithOneChildAsync("parent", "child"));
			Assert.That(parent.Children.Count, Is.EqualTo(1));
			IChild oldChild = GetFirstChild(parent.Children);
			listeners.Clear();
			ISession s = OpenSession();
			ITransaction tx = s.BeginTransaction();
			parent = (IParentWithCollection)await (s.GetAsync(parent.GetType(), parent.Id));
			parent.AddChild("new");
			await (tx.CommitAsync());
			s.Close();
			listeners.Clear();
			s = OpenSession();
			tx = s.BeginTransaction();
			parent = (IParentWithCollection)await (s.GetAsync(parent.GetType(), parent.Id));
			IEntity e = oldChild as IEntity;
			if (e != null)
			{
				oldChild = (IChild)await (s.GetAsync(oldChild.GetType(), e.Id));
			}

			parent.RemoveChild(oldChild);
			await (tx.CommitAsync());
			s.Close();
			int index = 0;
			if (((IPersistentCollection)parent.Children).WasInitialized)
			{
				CheckResult(listeners, listeners.InitializeCollection, parent, index++);
			}

			ChildWithBidirectionalManyToMany oldChildWithManyToMany = oldChild as ChildWithBidirectionalManyToMany;
			if (oldChildWithManyToMany != null)
			{
				if (((IPersistentCollection)oldChildWithManyToMany.Parents).WasInitialized)
				{
					CheckResult(listeners, listeners.InitializeCollection, oldChildWithManyToMany, index++);
				}
			}

			CheckResult(listeners, listeners.PreCollectionUpdate, parent, index++);
			CheckResult(listeners, listeners.PostCollectionUpdate, parent, index++);
			if (oldChildWithManyToMany != null)
			{
				CheckResult(listeners, listeners.PreCollectionUpdate, oldChildWithManyToMany, index++);
				CheckResult(listeners, listeners.PostCollectionUpdate, oldChildWithManyToMany, index++);
			}

			CheckNumberOfResults(listeners, index);
		}

		[Test]
		public async Task DeleteParentWithNullChildrenAsync()
		{
			CollectionListeners listeners = new CollectionListeners(sessions);
			IParentWithCollection parent = await (CreateParentWithNullChildrenAsync("parent"));
			listeners.Clear();
			ISession s = OpenSession();
			ITransaction tx = s.BeginTransaction();
			parent = (IParentWithCollection)await (s.GetAsync(parent.GetType(), parent.Id));
			await (s.DeleteAsync(parent));
			await (tx.CommitAsync());
			s.Close();
			int index = 0;
			CheckResult(listeners, listeners.InitializeCollection, parent, index++);
			CheckResult(listeners, listeners.PreCollectionRemove, parent, index++);
			CheckResult(listeners, listeners.PostCollectionRemove, parent, index++);
			CheckNumberOfResults(listeners, index);
		}

		[Test]
		public async Task DeleteParentWithNoChildrenAsync()
		{
			CollectionListeners listeners = new CollectionListeners(sessions);
			IParentWithCollection parent = await (CreateParentWithNoChildrenAsync("parent"));
			listeners.Clear();
			ISession s = OpenSession();
			ITransaction tx = s.BeginTransaction();
			parent = (IParentWithCollection)await (s.GetAsync(parent.GetType(), parent.Id));
			await (s.DeleteAsync(parent));
			await (tx.CommitAsync());
			s.Close();
			int index = 0;
			CheckResult(listeners, listeners.InitializeCollection, parent, index++);
			CheckResult(listeners, listeners.PreCollectionRemove, parent, index++);
			CheckResult(listeners, listeners.PostCollectionRemove, parent, index++);
			CheckNumberOfResults(listeners, index);
		}

		[Test]
		public async Task DeleteParentAndChildAsync()
		{
			CollectionListeners listeners = new CollectionListeners(sessions);
			IParentWithCollection parent = await (CreateParentWithOneChildAsync("parent", "child"));
			IChild child = GetFirstChild(parent.Children);
			listeners.Clear();
			ISession s = OpenSession();
			ITransaction tx = s.BeginTransaction();
			parent = (IParentWithCollection)await (s.GetAsync(parent.GetType(), parent.Id));
			IEntity e = child as IEntity;
			if (e != null)
			{
				child = (IChild)await (s.GetAsync(child.GetType(), e.Id));
			}

			parent.RemoveChild(child);
			if (e != null)
			{
				await (s.DeleteAsync(child));
			}

			await (s.DeleteAsync(parent));
			await (tx.CommitAsync());
			s.Close();
			int index = 0;
			CheckResult(listeners, listeners.InitializeCollection, parent, index++);
			ChildWithBidirectionalManyToMany childWithManyToMany = child as ChildWithBidirectionalManyToMany;
			if (childWithManyToMany != null)
			{
				CheckResult(listeners, listeners.InitializeCollection, childWithManyToMany, index++);
			}

			CheckResult(listeners, listeners.PreCollectionRemove, parent, index++);
			CheckResult(listeners, listeners.PostCollectionRemove, parent, index++);
			if (childWithManyToMany != null)
			{
				CheckResult(listeners, listeners.PreCollectionRemove, childWithManyToMany, index++);
				CheckResult(listeners, listeners.PostCollectionRemove, childWithManyToMany, index++);
			}

			CheckNumberOfResults(listeners, index);
		}

		[Test]
		public async Task MoveChildToDifferentParentAsync()
		{
			CollectionListeners listeners = new CollectionListeners(sessions);
			IParentWithCollection parent = await (CreateParentWithOneChildAsync("parent", "child"));
			IParentWithCollection otherParent = await (CreateParentWithOneChildAsync("otherParent", "otherChild"));
			IChild child = GetFirstChild(parent.Children);
			listeners.Clear();
			ISession s = OpenSession();
			ITransaction tx = s.BeginTransaction();
			parent = (IParentWithCollection)await (s.GetAsync(parent.GetType(), parent.Id));
			otherParent = (IParentWithCollection)await (s.GetAsync(otherParent.GetType(), otherParent.Id));
			IEntity e = child as IEntity;
			if (e != null)
			{
				child = (IChild)await (s.GetAsync(child.GetType(), e.Id));
			}

			parent.RemoveChild(child);
			otherParent.AddChild(child);
			await (tx.CommitAsync());
			s.Close();
			int index = 0;
			if (((IPersistentCollection)parent.Children).WasInitialized)
			{
				CheckResult(listeners, listeners.InitializeCollection, parent, index++);
			}

			ChildWithBidirectionalManyToMany childWithManyToMany = child as ChildWithBidirectionalManyToMany;
			if (childWithManyToMany != null)
			{
				CheckResult(listeners, listeners.InitializeCollection, childWithManyToMany, index++);
			}

			if (((IPersistentCollection)otherParent.Children).WasInitialized)
			{
				CheckResult(listeners, listeners.InitializeCollection, otherParent, index++);
			}

			CheckResult(listeners, listeners.PreCollectionUpdate, parent, index++);
			CheckResult(listeners, listeners.PostCollectionUpdate, parent, index++);
			CheckResult(listeners, listeners.PreCollectionUpdate, otherParent, index++);
			CheckResult(listeners, listeners.PostCollectionUpdate, otherParent, index++);
			if (childWithManyToMany != null)
			{
				CheckResult(listeners, listeners.PreCollectionUpdate, childWithManyToMany, index++);
				CheckResult(listeners, listeners.PostCollectionUpdate, childWithManyToMany, index++);
			}

			CheckNumberOfResults(listeners, index);
		}

		[Test]
		public async Task MoveAllChildrenToDifferentParentAsync()
		{
			CollectionListeners listeners = new CollectionListeners(sessions);
			IParentWithCollection parent = await (CreateParentWithOneChildAsync("parent", "child"));
			IParentWithCollection otherParent = await (CreateParentWithOneChildAsync("otherParent", "otherChild"));
			IChild child = GetFirstChild(parent.Children);
			listeners.Clear();
			ISession s = OpenSession();
			ITransaction tx = s.BeginTransaction();
			parent = (IParentWithCollection)await (s.GetAsync(parent.GetType(), parent.Id));
			otherParent = (IParentWithCollection)await (s.GetAsync(otherParent.GetType(), otherParent.Id));
			IEntity e = child as IEntity;
			if (e != null)
			{
				child = (IChild)await (s.GetAsync(child.GetType(), e.Id));
			}

			otherParent.AddAllChildren(parent.Children);
			parent.ClearChildren();
			await (tx.CommitAsync());
			s.Close();
			int index = 0;
			if (((IPersistentCollection)parent.Children).WasInitialized)
			{
				CheckResult(listeners, listeners.InitializeCollection, parent, index++);
			}

			if (((IPersistentCollection)otherParent.Children).WasInitialized)
			{
				CheckResult(listeners, listeners.InitializeCollection, otherParent, index++);
			}

			ChildWithBidirectionalManyToMany childWithManyToMany = child as ChildWithBidirectionalManyToMany;
			if (childWithManyToMany != null)
			{
				CheckResult(listeners, listeners.InitializeCollection, childWithManyToMany, index++);
			}

			CheckResult(listeners, listeners.PreCollectionUpdate, parent, index++);
			CheckResult(listeners, listeners.PostCollectionUpdate, parent, index++);
			CheckResult(listeners, listeners.PreCollectionUpdate, otherParent, index++);
			CheckResult(listeners, listeners.PostCollectionUpdate, otherParent, index++);
			if (childWithManyToMany != null)
			{
				CheckResult(listeners, listeners.PreCollectionUpdate, childWithManyToMany, index++);
				CheckResult(listeners, listeners.PostCollectionUpdate, childWithManyToMany, index++);
			}

			CheckNumberOfResults(listeners, index);
		}

		[Test]
		public async Task MoveCollectionToDifferentParentAsync()
		{
			CollectionListeners listeners = new CollectionListeners(sessions);
			IParentWithCollection parent = await (CreateParentWithOneChildAsync("parent", "child"));
			IParentWithCollection otherParent = await (CreateParentWithOneChildAsync("otherParent", "otherChild"));
			listeners.Clear();
			ISession s = OpenSession();
			ITransaction tx = s.BeginTransaction();
			parent = (IParentWithCollection)await (s.GetAsync(parent.GetType(), parent.Id));
			otherParent = (IParentWithCollection)await (s.GetAsync(otherParent.GetType(), otherParent.Id));
			ICollection<IChild> otherCollectionOrig = otherParent.Children;
			otherParent.NewChildren(parent.Children);
			parent.NewChildren(null);
			await (tx.CommitAsync());
			s.Close();
			int index = 0;
			ChildWithBidirectionalManyToMany otherChildOrig = null;
			if (((IPersistentCollection)otherCollectionOrig).WasInitialized)
			{
				CheckResult(listeners, listeners.InitializeCollection, otherParent, otherCollectionOrig, index++);
				otherChildOrig = GetFirstChild(otherCollectionOrig) as ChildWithBidirectionalManyToMany;
				if (otherChildOrig != null)
				{
					CheckResult(listeners, listeners.InitializeCollection, otherChildOrig, index++);
				}
			}

			CheckResult(listeners, listeners.InitializeCollection, parent, otherParent.Children, index++);
			ChildWithBidirectionalManyToMany otherChild = GetFirstChild(otherParent.Children) as ChildWithBidirectionalManyToMany;
			if (otherChild != null)
			{
				CheckResult(listeners, listeners.InitializeCollection, otherChild, index++);
			}

			CheckResult(listeners, listeners.PreCollectionRemove, parent, otherParent.Children, index++);
			CheckResult(listeners, listeners.PostCollectionRemove, parent, otherParent.Children, index++);
			CheckResult(listeners, listeners.PreCollectionRemove, otherParent, otherCollectionOrig, index++);
			CheckResult(listeners, listeners.PostCollectionRemove, otherParent, otherCollectionOrig, index++);
			if (otherChild != null)
			{
				CheckResult(listeners, listeners.PreCollectionUpdate, otherChildOrig, index++);
				CheckResult(listeners, listeners.PostCollectionUpdate, otherChildOrig, index++);
				CheckResult(listeners, listeners.PreCollectionUpdate, otherChild, index++);
				CheckResult(listeners, listeners.PostCollectionUpdate, otherChild, index++);
			}

			CheckResult(listeners, listeners.PreCollectionRecreate, otherParent, index++);
			CheckResult(listeners, listeners.PostCollectionRecreate, otherParent, index++);
			// there should also be pre- and post-recreate collection events for parent, but thats broken now;
			// this is covered in BrokenCollectionEventTest
			CheckNumberOfResults(listeners, index);
		}

		[Test]
		public async Task MoveCollectionToDifferentParentFlushMoveToDifferentParentAsync()
		{
			CollectionListeners listeners = new CollectionListeners(sessions);
			IParentWithCollection parent = await (CreateParentWithOneChildAsync("parent", "child"));
			IParentWithCollection otherParent = await (CreateParentWithOneChildAsync("otherParent", "otherChild"));
			IParentWithCollection otherOtherParent = await (CreateParentWithNoChildrenAsync("otherParent"));
			listeners.Clear();
			ISession s = OpenSession();
			ITransaction tx = s.BeginTransaction();
			parent = (IParentWithCollection)await (s.GetAsync(parent.GetType(), parent.Id));
			otherParent = (IParentWithCollection)await (s.GetAsync(otherParent.GetType(), otherParent.Id));
			otherOtherParent = (IParentWithCollection)await (s.GetAsync(otherOtherParent.GetType(), otherOtherParent.Id));
			ICollection<IChild> otherCollectionOrig = otherParent.Children;
			ICollection<IChild> otherOtherCollectionOrig = otherOtherParent.Children;
			otherParent.NewChildren(parent.Children);
			parent.NewChildren(null);
			await (s.FlushAsync());
			otherOtherParent.NewChildren(otherParent.Children);
			otherParent.NewChildren(null);
			await (tx.CommitAsync());
			s.Close();
			int index = 0;
			ChildWithBidirectionalManyToMany otherChildOrig = null;
			if (((IPersistentCollection)otherCollectionOrig).WasInitialized)
			{
				CheckResult(listeners, listeners.InitializeCollection, otherParent, otherCollectionOrig, index++);
				otherChildOrig = GetFirstChild(otherCollectionOrig) as ChildWithBidirectionalManyToMany;
				if (otherChildOrig != null)
				{
					CheckResult(listeners, listeners.InitializeCollection, otherChildOrig, index++);
				}
			}

			CheckResult(listeners, listeners.InitializeCollection, parent, otherOtherParent.Children, index++);
			ChildWithBidirectionalManyToMany otherOtherChild = GetFirstChild(otherOtherParent.Children) as ChildWithBidirectionalManyToMany;
			if (otherOtherChild != null)
			{
				CheckResult(listeners, listeners.InitializeCollection, otherOtherChild, index++);
			}

			CheckResult(listeners, listeners.PreCollectionRemove, parent, otherOtherParent.Children, index++);
			CheckResult(listeners, listeners.PostCollectionRemove, parent, otherOtherParent.Children, index++);
			CheckResult(listeners, listeners.PreCollectionRemove, otherParent, otherCollectionOrig, index++);
			CheckResult(listeners, listeners.PostCollectionRemove, otherParent, otherCollectionOrig, index++);
			if (otherOtherChild != null)
			{
				CheckResult(listeners, listeners.PreCollectionUpdate, otherChildOrig, index++);
				CheckResult(listeners, listeners.PostCollectionUpdate, otherChildOrig, index++);
				CheckResult(listeners, listeners.PreCollectionUpdate, otherOtherChild, index++);
				CheckResult(listeners, listeners.PostCollectionUpdate, otherOtherChild, index++);
			}

			CheckResult(listeners, listeners.PreCollectionRecreate, otherParent, otherOtherParent.Children, index++);
			CheckResult(listeners, listeners.PostCollectionRecreate, otherParent, otherOtherParent.Children, index++);
			if (((IPersistentCollection)otherOtherCollectionOrig).WasInitialized)
			{
				CheckResult(listeners, listeners.InitializeCollection, otherOtherParent, otherOtherCollectionOrig, index++);
			}

			CheckResult(listeners, listeners.PreCollectionRemove, otherParent, otherOtherParent.Children, index++);
			CheckResult(listeners, listeners.PostCollectionRemove, otherParent, otherOtherParent.Children, index++);
			CheckResult(listeners, listeners.PreCollectionRemove, otherOtherParent, otherOtherCollectionOrig, index++);
			CheckResult(listeners, listeners.PostCollectionRemove, otherOtherParent, otherOtherCollectionOrig, index++);
			if (otherOtherChild != null)
			{
				CheckResult(listeners, listeners.PreCollectionUpdate, otherOtherChild, index++);
				CheckResult(listeners, listeners.PostCollectionUpdate, otherOtherChild, index++);
			}

			CheckResult(listeners, listeners.PreCollectionRecreate, otherOtherParent, index++);
			CheckResult(listeners, listeners.PostCollectionRecreate, otherOtherParent, index++);
			// there should also be pre- and post-recreate collection events for parent, and otherParent
			// but thats broken now; this is covered in BrokenCollectionEventTest
			CheckNumberOfResults(listeners, index);
		}

		protected async Task<IParentWithCollection> CreateParentWithNullChildrenAsync(string parentName)
		{
			using (ISession s = OpenSession())
			{
				using (ITransaction tx = s.BeginTransaction())
				{
					IParentWithCollection parent = CreateParent(parentName);
					await (s.SaveAsync(parent));
					await (tx.CommitAsync());
					return parent;
				}
			}
		}

		protected async Task<IParentWithCollection> CreateParentWithNoChildrenAsync(string parentName)
		{
			using (ISession s = OpenSession())
			{
				using (ITransaction tx = s.BeginTransaction())
				{
					IParentWithCollection parent = CreateParent(parentName);
					parent.NewChildren(CreateCollection());
					await (s.SaveAsync(parent));
					await (tx.CommitAsync());
					return parent;
				}
			}
		}

		protected async Task<IParentWithCollection> CreateParentWithOneChildAsync(string parentName, string ChildName)
		{
			using (ISession s = OpenSession())
			{
				using (ITransaction tx = s.BeginTransaction())
				{
					IParentWithCollection parent = CreateParent(parentName);
					parent.NewChildren(CreateCollection());
					parent.AddChild(ChildName);
					await (s.SaveAsync(parent));
					await (tx.CommitAsync());
					return parent;
				}
			}
		}
	}
}
#endif
