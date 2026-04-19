using System.Drawing;
using WindowKeys;

namespace WindowKeysTests;

[TestFixture]
public class GeometryTests
{
	[Test]
	public void InnerCompletelyInsideOuter_ReturnsTrue()
	{
		var outer = new Native.RECT { Left = 10, Top = 10, Right = 50, Bottom = 50 };
		var inner = new Native.RECT { Left = 20, Top = 20, Right = 40, Bottom = 40 };

		Assert.That(Geometry.IsRectInside(outer, inner), Is.True);
	}

	[Test]
	public void InnerPartiallyOutsideOuter_ReturnsFalse()
	{
		var outer = new Native.RECT { Left = 10, Top = 10, Right = 50, Bottom = 50 };
		var inner = new Native.RECT { Left = -20, Top = -20, Right = 40, Bottom = 40 };

		Assert.That(Geometry.IsRectInside(outer, inner), Is.False);
	}

	[Test]
	public void InnerCompletelyOutsideOuter_ReturnsFalse()
	{
		var outer = new Native.RECT { Left = 10, Top = 10, Right = 50, Bottom = 50 };
		var inner = new Native.RECT { Left = 60, Top = 60, Right = 100, Bottom = 100 };

		Assert.That(Geometry.IsRectInside(outer, inner), Is.False);
	}

	[Test]
	public void InnerWithNegativeCoordinates_ReturnsFalse()
	{
		var outer = new Native.RECT { Left = 30, Top = 30, Right = 60, Bottom = 60 };
		var inner = new Native.RECT { Left = -10, Top = -10, Right = 30, Bottom = 30 };

		Assert.That(Geometry.IsRectInside(outer, inner), Is.False);
	}

	[Test]
	public void SubtractRectangle_NoIntersection_ReturnsOriginal()
	{
		var subject = new Native.RECT { Left = 0, Top = 0, Right = 100, Bottom = 100 };
		var occluder = new Native.RECT { Left = 200, Top = 200, Right = 300, Bottom = 300 };

		var result = Geometry.SubtractRectangle(subject, occluder);

		Assert.That(result, Has.Count.EqualTo(1));
		Assert.That(result[0].Left, Is.EqualTo(0));
		Assert.That(result[0].Top, Is.EqualTo(0));
		Assert.That(result[0].Right, Is.EqualTo(100));
		Assert.That(result[0].Bottom, Is.EqualTo(100));
	}

	[Test]
	public void SubtractRectangle_CenterOcclusion_ReturnsFourPieces()
	{
		var subject = new Native.RECT { Left = 0, Top = 0, Right = 100, Bottom = 100 };
		var occluder = new Native.RECT { Left = 25, Top = 25, Right = 75, Bottom = 75 };

		var result = Geometry.SubtractRectangle(subject, occluder);

		Assert.That(result, Has.Count.EqualTo(4));
		Assert.That(result.Any(r => r.Left == 0 && r.Top == 0 && r.Right == 25 && r.Bottom == 100));
		Assert.That(result.Any(r => r.Left == 75 && r.Top == 0 && r.Right == 100 && r.Bottom == 100));
		Assert.That(result.Any(r => r.Left == 0 && r.Top == 0 && r.Right == 100 && r.Bottom == 25));
		Assert.That(result.Any(r => r.Left == 0 && r.Top == 75 && r.Right == 100 && r.Bottom == 100));
	}

	[Test]
	public void SubtractRectangle_TopOcclusion_ReturnsThreePieces()
	{
		var subject = new Native.RECT { Left = 0, Top = 0, Right = 100, Bottom = 100 };
		var occluder = new Native.RECT { Left = 25, Top = 0, Right = 75, Bottom = 50 };

		var result = Geometry.SubtractRectangle(subject, occluder);

		Assert.That(result, Has.Count.EqualTo(3));
		Assert.That(result.Any(r => r.Left == 0 && r.Top == 0 && r.Right == 25 && r.Bottom == 100));
		Assert.That(result.Any(r => r.Left == 75 && r.Top == 0 && r.Right == 100 && r.Bottom == 100));
		Assert.That(result.Any(r => r.Left == 0 && r.Top == 50 && r.Right == 100 && r.Bottom == 100));
	}

	[Test]
	public void SubtractRectangle_FullOcclusion_ReturnsEmpty()
	{
		var subject = new Native.RECT { Left = 25, Top = 25, Right = 75, Bottom = 75 };
		var occluder = new Native.RECT { Left = 0, Top = 0, Right = 100, Bottom = 100 };

		Assert.That(Geometry.SubtractRectangle(subject, occluder), Is.Empty);
	}

	[Test]
	public void GetVisibleRegions_NoOccluders_ReturnsOriginal()
	{
		var windowRect = new Native.RECT { Left = 0, Top = 0, Right = 100, Bottom = 100 };

		var result = Geometry.GetVisibleRegions(windowRect, new List<Native.RECT>());

		Assert.That(result, Has.Count.EqualTo(1));
		Assert.That(result[0].Left, Is.EqualTo(0));
		Assert.That(result[0].Right, Is.EqualTo(100));
	}

	[Test]
	public void GetVisibleRegions_PartialOcclusion_ReturnsVisiblePieces()
	{
		var windowRect = new Native.RECT { Left = 0, Top = 0, Right = 100, Bottom = 100 };
		var occluders = new List<Native.RECT>
		{
			new() { Left = 25, Top = 25, Right = 75, Bottom = 75 }
		};

		var result = Geometry.GetVisibleRegions(windowRect, occluders);

		Assert.That(result, Has.Count.EqualTo(4));
		Assert.That(result.Max(r => r.Size), Is.EqualTo(2500));
	}

	[Test]
	public void GetVisibleRegions_FullOcclusion_ReturnsEmpty()
	{
		var windowRect = new Native.RECT { Left = 25, Top = 25, Right = 75, Bottom = 75 };
		var occluders = new List<Native.RECT>
		{
			new() { Left = 0, Top = 0, Right = 100, Bottom = 100 }
		};

		Assert.That(Geometry.GetVisibleRegions(windowRect, occluders), Is.Empty);
	}

	[Test]
	public void GetVisibleRegions_MultipleOccluders_ReturnsRemainingPieces()
	{
		var windowRect = new Native.RECT { Left = 0, Top = 0, Right = 100, Bottom = 100 };
		var occluders = new List<Native.RECT>
		{
			new() { Left = 0, Top = 0, Right = 50, Bottom = 50 },
			new() { Left = 50, Top = 50, Right = 100, Bottom = 100 }
		};

		var result = Geometry.GetVisibleRegions(windowRect, occluders);

		Assert.That(result, Is.Not.Empty);
		Assert.That(result.Sum(r => r.Size), Is.EqualTo(5000));
	}

	[Test]
	public void GetActivationStringPosition_NoOcclusion_ReturnsCenterOfWindow()
	{
		var windowRect = new Native.RECT { Left = 0, Top = 0, Right = 100, Bottom = 100 };
		var textSize = new Size(10, 10);

		var result = Geometry.GetActivationStringPosition(windowRect, new List<Native.RECT>(), textSize);

		Assert.That(result, Is.Not.Null);
		Assert.That(result!.Value.X, Is.EqualTo(50));
		Assert.That(result.Value.Y, Is.EqualTo(50));
	}

	[Test]
	public void GetActivationStringPosition_PartialOcclusion_ReturnsLargestVisibleArea()
	{
		var windowRect = new Native.RECT { Left = 0, Top = 0, Right = 100, Bottom = 100 };
		var occluders = new List<Native.RECT>
		{
			new() { Left = 0, Top = 0, Right = 100, Bottom = 75 }
		};
		var textSize = new Size(10, 10);

		var result = Geometry.GetActivationStringPosition(windowRect, occluders, textSize);

		Assert.That(result, Is.Not.Null);
		Assert.That(result!.Value.X, Is.EqualTo(50));
		Assert.That(result.Value.Y, Is.EqualTo(87));
	}

	[Test]
	public void GetActivationStringPosition_FullOcclusion_ReturnsNull()
	{
		var windowRect = new Native.RECT { Left = 25, Top = 25, Right = 75, Bottom = 75 };
		var occluders = new List<Native.RECT>
		{
			new() { Left = 0, Top = 0, Right = 100, Bottom = 100 }
		};

		Assert.That(Geometry.GetActivationStringPosition(windowRect, occluders, new Size(10, 10)), Is.Null);
	}
}
