using System;
using System.Collections;
using System.Text.RegularExpressions;
using FluentAssertions;
using NUnit.Framework;

namespace HomeExercises
{
	[TestFixture]
	public class NumberValidatorTests
	{
		[TestCase("", TestName = "Empty string")]
		[TestCase(null, TestName = "null argument")]
		public void Should_NotBeValid_When_EmptyOrNull(string arg)
		{
			var number = new NumberValidator(4, 2, true);

			number.IsValidNumber(arg).Should().BeFalse();
		}
		
		[TestCase("a.sd", TestName = "word string")]
		[TestCase("-a.sdf", TestName = "word string starts with -")]
		[TestCase(" \t\r\n", TestName = "special symbols")]
		[TestCase("12,as", TestName = "halfway number, halway string")]
		public void ShouldNot_BeValid_When_NAN(string arg)
		{
			var number = new NumberValidator(4, 2, true);
			number.IsValidNumber(arg).Should().BeFalse();
		}

		[TestCase("-1.23", TestName = "negative real")]
		[TestCase("-1,0", TestName = "negative real with comma")]
		[TestCase("+1,0", TestName = "positive real with comma")]
		public void Should_BeValid_WithRealNumbers(string arg)
		{
			var number = new NumberValidator(4, 2, false);

			number.IsValidNumber(arg).Should().BeTrue();
		}

		[TestCase("+1", TestName = "natural with plus")]
		[TestCase("1", TestName = "natural withour plus")]
		[TestCase("000", TestName = "zero is also works")]
		public void Should_BeValid_WithNaturalNumbers(string arg)
		{
			var number = new NumberValidator(4, 0, true);
			number.IsValidNumber(arg).Should().BeTrue();
		}

		[TestCase("+1", TestName = "+whole")]
		[TestCase("-1", TestName = "negative whole")]
		[TestCase("-000", TestName = "negative zeros, all precision")]
		public void Should_BeValid_WithWholeNumbers(string arg)
		{
			var number = new NumberValidator(4, 0, false);

			number.IsValidNumber(arg).Should().BeTrue();
		}
		
		[TestCase("0.000", TestName = "given scale is bigger than in number")]
		[TestCase("-0.000", TestName = "bigger scale with negative")]
		[TestCase("0,", TestName = "num with comma but without scale part")]
		public void ShouldNot_BeValid_When_InvalidScale(string arg)
		{
			var number = new NumberValidator(4, 2, true);

			number.IsValidNumber(arg).Should().BeFalse();
		}
		
		[TestCase("-0", TestName = "negative number")]
		public void ShouldNot_BeValid_When_OnlyPositive_WithNegativeNumber(string arg)
		{
			var number = new NumberValidator(4, 2, true);

			number.IsValidNumber(arg).Should().BeFalse();
		}

		[TestCase("0.0", TestName = "real zero")]
		[TestCase("-0.0", TestName = "negative")]
		[TestCase("+0,00", TestName = "plus zero with comma")]
		public void Should_BeValid_WithZeros(string arg)
		{
			var number = new NumberValidator(17, 2, false);

			number.IsValidNumber(arg).Should().BeTrue();
		}

		[TestCase("01.00", TestName = "precision with scale")]
		[TestCase("1000", TestName = "natural precision")]
		[TestCase("-0.00", TestName = "precision with minus")]
		[TestCase("+0,00", TestName = "precision with plus")]
		public void ShouldNot_BeValid_WithInvalidPrecisions(string arg)
		{
			var number = new NumberValidator(3, 2, true);

			number.IsValidNumber("arg").Should().BeFalse();
		}

		[Test]
		public void Should_ThrowException_IfPrecisionInConstructorIsNegative()
		{
			Action action = () => new NumberValidator(-1, 2, true);
			action.Should().Throw<Exception>("of realization").WithMessage("precision must be a positive number");
		}

		[Test]
		public void Should_NotThrowExceptions_WithCorrectParameters()
		{
			Action action = () => new NumberValidator(1, 0, true);
			action.Should().NotThrow("we gave correcct parameters");
		}

		private static IEnumerable ValidNumberScaleTestCases
		{
			get
			{
				Action negativeScale = () => new NumberValidator(3, -1, true);
				Action incorrectScale = () => new NumberValidator(3, 3, true);
				Action biggerScale = () => new NumberValidator(3, 4, true);

				yield return new TestCaseData(negativeScale).SetName("negative scale");
				yield return new TestCaseData(incorrectScale).SetName("incorrect scale");
				yield return new TestCaseData(biggerScale).SetName("bigger scale");
			}
		}
	
		[Test, TestCaseSource(nameof(ValidNumberScaleTestCases))]
		public void Should_ThrowArgumentException_When_ScaleIsIncorrect(Action action)
		{
			action.Should().Throw<ArgumentException>().WithMessage(
				"precision must be a non-negative number less or equal than precision");
		}
	}

	public class NumberValidator
	{
		private readonly Regex numberRegex;

		private readonly bool onlyPositive;

		private readonly int precision;

		private readonly int scale; // дробная часть без точки

		public NumberValidator(int precision, int scale = 0, bool onlyPositive = false)
		{
			this.precision = precision;
			this.scale = scale;
			this.onlyPositive = onlyPositive;
			if (precision <= 0)
				throw new Exception("precision must be a positive number");
			if (scale < 0 || scale >= precision)
				throw new ArgumentException("precision must be a non-negative number less or equal than precision");
			numberRegex = new Regex(@"^([+-]?)(\d+)([.,](\d+))?$", RegexOptions.IgnoreCase);
		}

		public bool IsValidNumber(string value)
		{
			// Проверяем соответствие входного значения формату N(m,k), в соответствии с правилом, 
			// описанным в Формате описи документов, направляемых в налоговый орган в электронном виде по телекоммуникационным каналам связи:
			// Формат числового значения указывается в виде N(m.к), где m – максимальное количество знаков в числе, включая знак (для отрицательного числа), 
			// целую и дробную часть числа без разделяющей десятичной точки, k – максимальное число знаков дробной части числа. 
			// Если число знаков дробной части числа равно 0 (т.е. число целое), то формат числового значения имеет вид N(m).

			if (string.IsNullOrEmpty(value))
				return false;

			var match = numberRegex.Match(value);
			if (!match.Success)
				return false;

			// Знак и целая часть
			var intPart = match.Groups[1].Value.Length + match.Groups[2].Value.Length;
			// Дробная часть
			var fracPart = match.Groups[4].Value.Length;

			if (intPart + fracPart > precision || fracPart > scale)
				return false;

			if (onlyPositive && match.Groups[1].Value == "-")
				return false;
			return true;
		}
	}
}