namespace WindowKeys;

internal static class Geometry
{
    public static bool IsRectInside(Native.RECT outer, Native.RECT inner)
    {
        const int margin = 25;
        return inner.Left > outer.Left - margin && inner.Top > outer.Top - margin &&
               inner.Right < outer.Right + margin && inner.Bottom < outer.Bottom + margin;
    }

    public static Point? GetActivationStringPosition(Native.RECT windowRect, IReadOnlyList<Native.RECT> occludingRects,
        Size textSize)
    {
        var visibleRegions = GetVisibleRegions(windowRect, occludingRects);
        if (visibleRegions.Count == 0) return null;

        var rect = visibleRegions.MaxBy(r => r.Size);
        return new Point((rect.Left + rect.Right) / 2, (rect.Top + rect.Bottom) / 2);
    }

    public static List<Native.RECT> GetVisibleRegions(Native.RECT windowRect, IReadOnlyList<Native.RECT> occludingRects)
    {
        var regions = new List<Native.RECT> { windowRect };
        foreach (var occluder in occludingRects)
        {
            var newRegions = new List<Native.RECT>();
            foreach (var region in regions)
                newRegions.AddRange(SubtractRectangle(region, occluder));
            regions = newRegions;
        }
        return regions;
    }

    public static List<Native.RECT> GetOccludingRects(Native.RECT target, IEnumerable<Native.RECT> priorRects)
    {
        var rects = new List<Native.RECT>();
        foreach (var rect in priorRects)
        {
            if (rect.Left > target.Right) continue;
            if (rect.Right < target.Left) continue;
            if (rect.Top > target.Bottom) continue;
            if (rect.Bottom < target.Top) continue;
            rects.Add(rect);
        }
        return rects;
    }

    public static List<Native.RECT> SubtractRectangle(Native.RECT subject, Native.RECT occluder)
    {
        if (occluder.Left >= subject.Right || occluder.Right <= subject.Left ||
            occluder.Top >= subject.Bottom || occluder.Bottom <= subject.Top)
            return [subject];

        var cLeft = Math.Max(occluder.Left, subject.Left);
        var cRight = Math.Min(occluder.Right, subject.Right);
        var cTop = Math.Max(occluder.Top, subject.Top);
        var cBottom = Math.Min(occluder.Bottom, subject.Bottom);

        var result = new List<Native.RECT>();

        if (cLeft > subject.Left)
            result.Add(new Native.RECT
            { Left = subject.Left, Top = subject.Top, Right = cLeft, Bottom = subject.Bottom });
        if (cRight < subject.Right)
            result.Add(new Native.RECT
            { Left = cRight, Top = subject.Top, Right = subject.Right, Bottom = subject.Bottom });
        if (cTop > subject.Top)
            result.Add(new Native.RECT
            { Left = subject.Left, Top = subject.Top, Right = subject.Right, Bottom = cTop });
        if (cBottom < subject.Bottom)
            result.Add(new Native.RECT
            { Left = subject.Left, Top = cBottom, Right = subject.Right, Bottom = subject.Bottom });

        return result;
    }
}
