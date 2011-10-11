using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Text.Classification;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace CoffeeSyntax {
	static class ClassificationTypes {

		[Export]
		[Name(VisualFormatNames.Coffee)]
		internal static ClassificationTypeDefinition CoffeeClassificationDefinition = null;

		[Export]
		[Name(VisualFormatNames.CoffeeString)]
		[BaseDefinition(VisualFormatNames.Coffee)]
		internal static ClassificationTypeDefinition StringClassificationDefinition = null;

		[Export]
		[Name(VisualFormatNames.CoffeeIdentifier)]
		[BaseDefinition(VisualFormatNames.Coffee)]
		internal static ClassificationTypeDefinition IdentifierClassificationDefinition = null;

		[Export]
		[Name(VisualFormatNames.CoffeeKeyword)]
		[BaseDefinition(VisualFormatNames.Coffee)]
		internal static ClassificationTypeDefinition KeywordClassificationDefinition = null;

		[Export]
		[Name(VisualFormatNames.CoffeeNumericLiteral)]
		[BaseDefinition(VisualFormatNames.Coffee)]
		internal static ClassificationTypeDefinition NumericLiteralClassificationDefinition = null;

		[Export]
		[Name(VisualFormatNames.CoffeeComment)]
		[BaseDefinition(VisualFormatNames.Coffee)]
		internal static ClassificationTypeDefinition CommentClassificationDefinition = null;

        [Export]
        [Name(VisualFormatNames.CoffeeThis)]
        [BaseDefinition(VisualFormatNames.Coffee)]
        internal static ClassificationTypeDefinition ThisClassificationDefinition = null;

    }
}
