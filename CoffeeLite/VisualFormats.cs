using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Text.Classification;
using System.Windows.Media;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace CoffeeSyntax {
	static class VisualFormats {

		[Export(typeof(EditorFormatDefinition))]
		[ClassificationType(ClassificationTypeNames = VisualFormatNames.CoffeeString)]
		[Name(VisualFormatNames.CoffeeString)]
		[UserVisible(true)]
		[DisplayName("Coffee string")]
		[Order(Before = Priority.Default)]
		sealed class StringFormat : ClassificationFormatDefinition {
			public StringFormat() {
				this.ForegroundColor = Colors.DarkRed;
			}
		}

		[Export(typeof(EditorFormatDefinition))]
		[ClassificationType(ClassificationTypeNames = VisualFormatNames.CoffeeIdentifier)]
		[Name(VisualFormatNames.CoffeeIdentifier)]
		[UserVisible(true)]
		[DisplayName("Coffee identifier")]
		[Order(Before = Priority.Default)]
		sealed class IdentifierFormat : ClassificationFormatDefinition {
			public IdentifierFormat() {
				this.ForegroundColor = Colors.Black;
			}
		}

		[Export(typeof(EditorFormatDefinition))]
		[ClassificationType(ClassificationTypeNames = VisualFormatNames.CoffeeKeyword)]
		[Name(VisualFormatNames.CoffeeKeyword)]
		[UserVisible(true)]
		[DisplayName("Coffee keyword")]
		[Order(Before = Priority.Default)]
		sealed class KeywordFormat : ClassificationFormatDefinition {
			public KeywordFormat() {
				this.ForegroundColor = Colors.Blue;
			}
		}

		[Export(typeof(EditorFormatDefinition))]
		[ClassificationType(ClassificationTypeNames = VisualFormatNames.CoffeeNumericLiteral)]
		[Name(VisualFormatNames.CoffeeNumericLiteral)]
		[UserVisible(true)]
		[DisplayName("Coffee numeric literal")]
		[Order(Before = Priority.Default)]
		sealed class NumericLiteralFormat : ClassificationFormatDefinition {
			public NumericLiteralFormat() {
				this.ForegroundColor = Colors.Black;
			}
		}

		[Export(typeof(EditorFormatDefinition))]
		[ClassificationType(ClassificationTypeNames = VisualFormatNames.CoffeeComment)]
		[Name(VisualFormatNames.CoffeeComment)]
		[UserVisible(true)]
		[DisplayName("Coffee comment")]
		[Order(Before = Priority.Default)]
		sealed class CommentFormat : ClassificationFormatDefinition {
			public CommentFormat() {
				this.ForegroundColor = Colors.Green;
			}
		}

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = VisualFormatNames.CoffeeThis)]
        [Name(VisualFormatNames.CoffeeThis)]
        [UserVisible(true)]
        [DisplayName("Coffee this")]
        [Order(Before = Priority.Default)]
        sealed class CommentThis : ClassificationFormatDefinition {
            public CommentThis() {
                this.ForegroundColor = Colors.Orange;
            }
        }

    }
}
