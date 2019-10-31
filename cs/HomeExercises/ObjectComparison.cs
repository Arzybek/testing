using System.Text.RegularExpressions;
using FluentAssertions;
using NUnit.Framework;

namespace HomeExercises
{
	[TestFixture]
	public class ObjectComparison
	{
		[Test]
		[Description("Проверка текущего царя")]
		[Category("ToRefactor")]
		public void CheckCurrentTsar()
		{
			var actualTsar = TsarRegistry.GetCurrentTsar();

			var expectedTsar = new Person(
				"Ivan IV The Terrible",
				54,
				170,
				70,
				new Person("Vasili III of Russia", 28, 170, 60, null));
			
			actualTsar.Should().BeEquivalentTo(expectedTsar, options =>
				options.Excluding(ctx => Regex.IsMatch(ctx.SelectedMemberPath, @"^(Parent\.)*Id$")));
		}

		[Test]
		[Description("Альтернативное решение. Какие у него недостатки?")]
		public void CheckCurrentTsar_WithCustomEquality()
		{
			/* По сути это решение ни чем не отличается от изначального в CheckCurrentTsar.
			 * Главный его недостаток - не масштабируемость. При добавлении новых полей и свойств их придется
			 * прописывать вручную в метод сравнения.
			 * Преимущество решения через Fluent Assertions в том что при помощи рефлексии
			 * происходит сравнение графов объектов, т.е при расширении нам нужно будет лишь указать, какие
			 * поля или методы мы не хотим сравнивать, и, возможно, немного изменить структуру сравнения
			 */
			var actualTsar = TsarRegistry.GetCurrentTsar();
			var expectedTsar = new Person(
				"Ivan IV The Terrible",
				54,
				170,
				70,
				new Person("Vasili III of Russia", 28, 170, 60, null));

			// Какие недостатки у такого подхода? 
			/*
			 * Не считая того, что я уже написал, можно было реализовать это сравнение в самом классе Person.
			 * Недостаток - может произойти переполнение стека. Fluent Assertion по стандарту поднимается только до 10 уровней глубины.
			 */
			Assert.True(AreEqual(actualTsar, expectedTsar));
		}

		private bool AreEqual(Person actual, Person expected)
		{
			if (actual == expected) return true;
			if (actual == null || expected == null) return false;
			return actual.Name == expected.Name && actual.Age == expected.Age && actual.Height == expected.Height
			       && actual.Weight == expected.Weight && AreEqual(actual.Parent, expected.Parent);
		}
	}

	public class TsarRegistry
	{
		public static Person GetCurrentTsar()
		{
			return new Person(
				"Ivan IV The Terrible",
				54,
				170,
				70,
				new Person("Vasili III of Russia", 28, 170, 60, null));
		}
	}

	public class Person
	{
		public static int IdCounter = 0;

		public int Age;

		public int Height;

		public int Weight;

		public string Name;

		public Person Parent;

		public int Id;

		public Person(string name, int age, int height, int weight, Person parent)
		{
			Id = IdCounter++;
			Name = name;
			Age = age;
			Height = height;
			Weight = weight;
			Parent = parent;
		}
	}
}