#if NET_4_5
using System;
using System.Collections.Generic;
using NUnit.Framework;
using System.Threading.Tasks;

namespace NHibernate.Test.NHSpecificTest.NH706
{
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class Fixture : BugTestCase
	{
		[Test]
		public async Task BugAsync()
		{
			Parent parent = new Parent();
			parent.Name = "Parent";
			RelatedObject obj1 = new RelatedObject();
			obj1.Name = "Related Object 1";
			RelatedObject obj2 = new RelatedObject();
			obj2.Name = "Related Object 2";
			Child child1 = new Child();
			child1.Parent = parent;
			child1.RelatedObject = obj1;
			child1.Name = "Child 1";
			Child child2 = new Child();
			child2.Parent = parent;
			child2.RelatedObject = obj2;
			child2.Name = "Child 2";
			parent.Children = new SortedSet<Child>(new ChildComparer());
			parent.Children.Add(child1);
			parent.Children.Add(child2);
			DifferentChild dc1 = new DifferentChild();
			dc1.Parent = parent;
			dc1.Name = "Different Child 1";
			dc1.Child = child1;
			DifferentChild dc2 = new DifferentChild();
			dc2.Parent = parent;
			dc2.Name = "Different Child 2";
			dc2.Child = child2;
			parent.DifferentChildren = new HashSet<DifferentChild>();
			parent.DifferentChildren.Add(dc1);
			parent.DifferentChildren.Add(dc2);
			using (ISession session = OpenSession())
			{
				await (session.SaveAsync(obj1));
				await (session.SaveAsync(obj2));
				await (session.SaveAsync(parent));
				await (session.SaveAsync(child1));
				await (session.SaveAsync(child2));
				await (session.SaveAsync(dc1));
				await (session.SaveAsync(dc2));
				await (session.FlushAsync());
			}

			int dcId = 0;
			using (ISession session = OpenSession())
			{
				Parent loadedParent = (Parent)await (session.GetAsync(typeof (Parent), parent.ID));
				await (NHibernateUtil.InitializeAsync(loadedParent.DifferentChildren));
				foreach (DifferentChild dc in loadedParent.DifferentChildren)
				{
					dcId = dc.ID;
					break;
				}
			}

			using (ISession session = OpenSession())
			{
				DifferentChild dc = (DifferentChild)await (session.GetAsync(typeof (DifferentChild), dcId));
			}

			using (ISession session = OpenSession())
			{
				await (session.DeleteAsync(dc1));
				await (session.DeleteAsync(dc2));
				await (session.DeleteAsync(child1));
				await (session.DeleteAsync(child2));
				await (session.DeleteAsync(parent));
				await (session.DeleteAsync(obj1));
				await (session.DeleteAsync(obj2));
				await (session.FlushAsync());
			}
		}
	}
}
#endif
