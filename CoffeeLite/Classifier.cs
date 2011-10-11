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
            this.clsThis = classificationRegistry.GetClassificationType(VisualFormatNames.CoffeeThis);
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
        private IClassificationType clsThis;
		private ITextSnapshot latestSnapshot = null;

		public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;

		private IEnumerable<SnapshotSpan> Split(SnapshotSpan span, IEnumerable<SnapshotSpan> remove) {
			var removeNorm = new NormalizedSnapshotSpanCollection(remove);
			foreach (var r in removeNorm.OrderBy(x => x.Start.Position)) {
				if (r.Start < span.End) {
					if (r.Start > span.Start) {
						yield return new SnapshotSpan(span.Start, r.Start);
					}
					if (r.End < span.End) {
						span = new SnapshotSpan(r.End, span.End);
					} else {
						yield break;
					}
				}
			}
			yield return span;
		}

		public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span) {
			this.latestSnapshot = span.Snapshot;
			var ss = span.Snapshot;
			var line = ss.GetLineFromPosition(span.Start.Position);
			var mlOverlaps = this.overview.MultiLines.Where(x => x.GetSpan(ss).OverlapsWith(line.Extent)).ToArray();
			var withoutMls = Split(line.Extent, mlOverlaps.Select(x => x.GetSpan(ss)));
			var query =
				from part in withoutMls
				let text = part.GetText()
				let tokens = Parser.Parse(text)
				from token in tokens
				let cls = this.MapToken(token.Token)
				where cls != null
				let s = new SnapshotSpan(ss, part.Start.Position + token.Start, token.Length)
				select new ClassificationSpan(s, cls);
			var queryMl =
				from part in mlOverlaps
				let cls = this.MapMultiline(part.Type)
				where cls != null
				select new ClassificationSpan(part.GetSpan(ss), cls);
			var ret = query.Concat(queryMl).ToArray();
			return ret;
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
			case Token.Comment:
				return this.clsCoffeeComment;
            case Token.This:
                return this.clsThis;
			default:
				return null;
			}
		}

	}

}
