using WindowKeys;

namespace WindowKeysTests;

[TestFixture]
public class CombinationGeneratorTests
{
	private static readonly char[] DefaultKeys = ['A', 'R', 'S', 'T', 'N', 'E', 'I', 'O'];

	[Test]
	public void Generate_GeneratesExactAmountOfUniqueCombinations()
	{
		for (var i = 0; i < 1000; i++)
		{
			var combinations = CombinationGenerator.Generate(i, DefaultKeys);
			Assert.That(combinations, Has.Count.EqualTo(i));
			Assert.That(combinations, Is.Unique);
		}
	}

	[Test]
	public void Generate_SingleWindow_ProducesSingleLetterCombination()
	{
		var combinations = CombinationGenerator.Generate(1, DefaultKeys);
		Assert.That(combinations, Has.Count.EqualTo(1));
		Assert.That(combinations[0], Has.Length.EqualTo(1));
	}
}
