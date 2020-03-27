using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gputempmon.UI.Chart
{
    class ChartState
    {
        public int[] Values { get; set; }
        public int[] Guides { get; set; }
        public int YMax { get; set; }
        public int YMin { get; set; }
        public int Step { get; set; }
    }

    class ChartPainter
    {
        public string PaintToString(ChartState state)
        {
            using (StringWriter stringWriter = new StringWriter())
            {
                Paint(state, stringWriter);
                return stringWriter.ToString();
            }
        }

        public void Paint(ChartState state, TextWriter writer)
        {
            int yMin = state.YMin;
            int yMax = state.YMax;
            int step = state.Step;
            double halfStep = step / 2.0;
            int lastValueIndex = state.Values.Length - 1;
            int maxGuideSize = state.Guides.Max(guide => (int)Math.Floor(Math.Log10(guide) + 1));
            string guidePadding = " ";
            string emptyGuide = "".PadLeft(guidePadding.Length + maxGuideSize, ' ');

            int[] values = state.Values;
            for (int y = yMax; y >= yMin; y -= step)
            {
                var guides = state.Guides.Where(guide => y - step < guide && guide <= y).ToArray();
                bool isGuide = guides.Any();
                bool lastValueWritten = false;
                int lastValue = 0;
                for (int i = 0; i < values.Length; i++)
                {
                    lastValue = values[i];
                    if (y - halfStep < lastValue && lastValue <= y)
                    {
                        writer.Write("▀");
                        lastValueWritten = i == lastValueIndex;
                    }
                    else if (y - step < lastValue && lastValue <= y - halfStep)
                    {
                        writer.Write("▄");
                        lastValueWritten = i == lastValueIndex;
                    }
                    else if (isGuide)
                        writer.Write("·");
                    else
                        writer.Write(" ");
                }

                writer.Write(" ");
                if (lastValueWritten)
                {
                    writer.Write(lastValue);
                }
                else if (isGuide)
                {
                    writer.Write(guides.First());
                }
                else
                {
                    writer.Write("".PadLeft(4));
                }

                writer.WriteLine();
            }
        }
    }
}
