using System;

namespace Soft.Geometry.UI.FancyFe
{
    public static class NLToGraphTranslator
    {
        // Parses a natural language command into a graph delta (skeleton).
        // Example: "Create an extrude with height 3 from a rectangle 6x4"
        // For now, returns null to indicate no-op. Extend in future.
        public static GraphModel ApplyCommand(GraphModel currentGraph, string naturalLanguageCommand)
        {
            if (currentGraph == null) throw new ArgumentNullException(nameof(currentGraph));
            if (string.IsNullOrWhiteSpace(naturalLanguageCommand)) return currentGraph;

            // TODO: parse text and add nodes/edges accordingly.
            // Placeholder: no changes.
            return currentGraph;
        }
    }
}
