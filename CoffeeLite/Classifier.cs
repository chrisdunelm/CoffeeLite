using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text;
using CoffeeParser;

namespace CoffeeSyntax {
	
	internal class Classifier : IClassifier {

		public Classifier(CoffeeOverview overview, IClassificationTypeRegistryService classificationRegistry) {
			this.overview = overview;
			this.clsCoffeeString = classificationRegistry.GetClassificationType(VisualFormatNames.CoffeeString);
			this.clsCoffeeIdentifier = classificationRegistry.GetClassificationType(VisualFormatNames.CoffeeIdentifier);
			this.clsCoffeeKeyword = classificationRegistry.GetClassificationType(VisualFormatNames.CoffeeKeyword);
			this.clsCoffeeNumericLiteral = classificationRegistry.GetClassificationType(VisualFormatNames.CoffeeNumericLiteral);
			this.clsCoffeeComment = classificationRegistry.GetClassificationType(VisualFormatNames.CoffeeComment);
			overview.MultiLinesChanged += (o, e) => {
				if (this.latestSnapshot != null && this.ClassificationChanged != null) {
					var spans = new NormalizedSnapshotSpanCollection(
						e.Added.Concat(e.Removed)
						.Select(x => x.Span.GetSpan(this.latestSnapshot)));
					foreach (var span in spans) {
						var args = new ClassificationChangedEventArgs(span);
						this.ClassificationChanged(this, args);
					}
				}
			};
		}

		private CoffeeOverview overview;
		private IClassificationType clsCoffeeString;
		private IClassificationType clsCoffeeIdentifier;
		private IClassificationType clsCoffeeKeyword;
		private IClassificationType clsCoffeeNumericLiteral;
		private IClassificationType clsCoffeeComment;
		private ITextSnapshot latestSnapshot = null;

		public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;

		public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span) {
			this.latestSnapshot = span.Snapshot;
			var ss = span.Snapshot;
			var mlOverlaps = this.overview.MultiLines.Where(x => x.GetSpan(ss).OverlapsWith(span)).ToArray();
			if (mlOverlaps.Any()) {
				var c =
					from s in mlOverlaps
					let cls = this.MapMultiline(s.Type)
					where cls != null
					select new ClassificationSpan(s.GetSpan(ss), cls);
				return c.ToArray();
			} else {
				var line = ss.GetLineFromPosition(span.Start.Position);
				var lineText = line.GetText();
				var tokens = Parser.Parse(lineText);
				var c =
					from token in tokens
					let cls = this.MapToken(token.Token)
					where cls != null
					let s = new SnapshotSpan(ss, line.Start.Position + token.Start, token.Length)
					select new ClassificationSpan(s, cls);
				return c.ToArray();
			}
		}

		private IClassificationType MapMultiline(CoffeeOverview.MultiLineType type) {
			switch (type) {
			case CoffeeOverview.MultiLineType.Comment:
				return this.clsCoffeeComment;
			case CoffeeOverview.MultiLineType.HeredocDouble:
			case CoffeeOverview.MultiLineType.HeredocSingle:
			case CoffeeOverview.MultiLineType.StringDouble:
			case CoffeeOverview.MultiLineType.StringSingle:
				return this.clsCoffeeString;
			default:
				return null;
			}
		}

		private IClassificationType MapToken(Token token) {
			switch (token) {
			case Token.Identifier:
				return this.clsCoffeeIdentifier;
			case Token.Keyword:
				return this.clsCoffeeKeyword;
			case Token.NumericLiteral:
				return this.clsCoffeeNumericLiteral;
			case Token.StringLiteral:
				return this.clsCoffeeString;
			default:
				return null;
			}
		}

	}

}
