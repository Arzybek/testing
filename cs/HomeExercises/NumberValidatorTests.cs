using System;
using System.Text.RegularExpressions;

using FluentAssertions;

using NUnit.Framework;

namespace HomeExercises
{
    using FluentAssertions.Execution;

    [TestFixture]
    public class NumberValidatorTests
    {
        [Test]
        [Category("Refactored")]
        public void Should_ReturnFalse_WithEmptyString()
        {
            var number = new NumberValidator(4, 2, true);
            number.IsValidNumber(String.Empty).Should().BeFalse("its empty string");
        }

        [Test]
        [Category("Refactored")]
        public void Should_ReturnFalse_WithNull()
        {
            var number = new NumberValidator(4, 2, true);
            number.IsValidNumber(null).Should().BeFalse("its null");
        }

        [Test]
        [Category("Refactored")]
        public void Should_ReturnFalse_WithNAN()
        {
            var number = new NumberValidator(4, 2, true);
            number.IsValidNumber("a.sd").Should().BeFalse("its not a number");
            number.IsValidNumber("-a.sd").Should().BeFalse("its not a number");
            number.IsValidNumber("12,as").Should().BeFalse("its not a number");
            number.IsValidNumber(" \t\r\n").Should().BeFalse("its not a number");
        }

        [Test]
        [Category("Refactored")]
        public void Should_ReturnTrue_WithValidSignedNumbers()
        {
            var number = new NumberValidator(4, 2, false);
            number.IsValidNumber("-1.23").Should().BeTrue("its valid number");
            number.IsValidNumber("-1,0").Should().BeTrue("its valid number");
            number = new NumberValidator(4, 0, false);
            number.IsValidNumber("-1").Should().BeTrue("its valid number");
        }

        [Test]
        [Category("Refactored")]
        public void Should_ReturnTrue_WithWholeNumber()
        {
            var number = new NumberValidator(4, 0, true);
            number.IsValidNumber("0000").Should().BeTrue("its correct whole number");
        }

        [Test]
        [Category("Refactored")]
        public void Should_ReturnFalse_WithInvalidScale()
        {
            var number = new NumberValidator(4, 2, true);
            number.IsValidNumber("0.000").Should().BeFalse("its scale isnt correct");
        }

        [Test]
        [Category("Refactored")]
        public void Should_ReturnTrue_WithValidPositiveNumbers()
        {
            var number = new NumberValidator(4, 2, true);
            number.IsValidNumber("1.0").Should().BeTrue("its valid number");
            number.IsValidNumber("1,23").Should().BeTrue("its valid number");
            number.IsValidNumber("+1.23").Should().BeTrue("its valid number");
        }

        [Test]
        [Category("Refactored")]
        public void Should_ReturnTrue_WithValidZeros()
        {
            var number = new NumberValidator(17, 2, true);
            number.IsValidNumber("0.0").Should().BeTrue("its valid number");
            number.IsValidNumber("0").Should().BeTrue("its valid number");
        }

        [Test]
        [Category("Refactored")]
        public void Should_ReturnFalse_WithInvalidZPrecisions()
        {
            var number = new NumberValidator(3, 2, true);
            number.IsValidNumber("00.00").Should().BeFalse("precision isnt correct");
            number.IsValidNumber("-0,00").Should().BeFalse("precision isnt correct");
            number.IsValidNumber("+0.00").Should().BeFalse("precision isnt correct");
        }

        [Test]
        [Category("Refactored")]
        public void Should_ThrowException_IfPrecisionIsNegative()
        {
            var flag = true;
            Action action = () => new NumberValidator(-1, 2, flag);
            action.Should().Throw<Exception>("of realization").WithMessage("precision must be a positive number");
            flag = false;
            action.Should().Throw<Exception>("of realization").WithMessage("precision must be a positive number");
        }

        [Test]
        [Category("Refactored")]
        public void Should_NotThrowExceptions_WithCorrectParameters()
        {
            var flag = true;
            Action action = () => new NumberValidator(1, 0, flag);
            action.Should().NotThrow("we gave correcct parameters");
            flag = false;
            action.Should().NotThrow("we gave correcct parameters");
        }

        [Test]
        [Category("Refactored")]
        public void Should_ThrowArgumentException_When_ScaleIsIncorrect()
        {
            using (new AssertionScope())
            {
                var flag = true;
                Action negativeScale = () => new NumberValidator(3, -1, flag);
                Action incorrectScale = () => new NumberValidator(3, 3, flag);
                Action biggerScale = () => new NumberValidator(3, 4, flag);
                Action[] collection = new Action[] { negativeScale, incorrectScale, biggerScale };
                foreach (var action in collection)
                {
                    action.Should().Throw<ArgumentException>().WithMessage(
                        "precision must be a non-negative number less or equal than precision");
                }

                flag = false;
                foreach (var action in collection)
                {
                    action.Should().Throw<ArgumentException>().WithMessage(
                        "precision must be a non-negative number less or equal than precision");
                }
            }
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