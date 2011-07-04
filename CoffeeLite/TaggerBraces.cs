using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text;
using CoffeeParser;

namespace CoffeeSyntax {
	class TaggerBraces : ITagger<TextMarkerTag> {

		public TaggerBraces(CoffeeOverview overview, ITextView view, ITextBuffer sourceBuffer) {
			this.overview = overview;
			this.view = view;
			this.sourceBuffer = sourceBuffer;
			this.currentChar = null;
			this.view.Caret.PositionChanged += (o, e) => {
				this.UpdateAtCaretPosition(e.NewPosition);
			};
			this.view.LayoutChanged += (o, e) => {
				if (e.NewSnapshot != e.OldSnapshot) {
					this.UpdateAtCaretPosition(this.view.Caret.Position);
				}
			};
		}

		private CoffeeOverview overview;
		private ITextView view;
		private ITextBuffer sourceBuffer;
		private SnapshotPoint? currentChar;

		public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

		private void UpdateAtCaretPosition(CaretPosition caretPosition) {
			this.currentChar = caretPosition.Point.GetPoint(this.sourceBuffer, caretPosition.Affinity);
			if (this.currentChar != null) {
				if (this.TagsChanged != null) {
					var curSs = this.sourceBuffer.CurrentSnapshot;
					var args = new SnapshotSpanEventArgs(new SnapshotSpan(curSs, 0, curSs.Length));
					this.TagsChanged(this, args);
				}
			}
		}

		private Tuple<char, char>[] braces =
		{
			Tuple.Create('(', ')'),
			Tuple.Create('{', '}'),
			Tuple.Create('[', ']'),
		};

		private bool ShouldTag(SnapshotPoint pt) {
			var ss = pt.Snapshot;
			if (this.overview.MultiLines.Any(x => x.GetSpan(ss).Contains(pt))) {
				return false;
			}
			var line = pt.GetContainingLine();
			var tokens = Parser.Parse(line.GetText());
			return !tokens.Any(x =>
				x.Token.In(Token.Comment) &&
				pt.Position >= line.Start.Position + x.Start && pt.Position < line.Start.Position + x.Start + x.Length);
		}

		public IEnumerable<ITagSpan<TextMarkerTag>> GetTags(NormalizedSnapshotSpanCollection spans) {
			if (spans.Count == 0) {
				yield break;
			}
			if (this.currentChar == null || this.currentChar.Value.Position >= this.currentChar.Value.Snapshot.Length) {
				yield break;
			}
			foreach (var span in spans) {
				var ss = span.Snapshot;
				var cc = ss == this.currentChar.Value.Snapshot ?
					this.currentChar.Value : this.currentChar.Value.TranslateTo(ss, PointTrackingMode.Positive);
				if (this.ShouldTag(cc)) {
					char cNext = cc.GetChar();
					var brace1 = braces.FirstOrDefault(x => x.Item1 == cNext);
					if (brace1 != null) {
						var match = this.FindMatchForwards(ss, brace1.Item1, brace1.Item2, cc.Position);
						if (match != null) {
							var tag = new TextMarkerTag("blue");
							yield return new TagSpan<TextMarkerTag>(new SnapshotSpan(cc, 1), tag);
							yield return new TagSpan<TextMarkerTag>(new SnapshotSpan(ss, match.Value, 1), tag);
						}
					}
				}
				var ccPrev = cc.Position > 0 ? (cc - 1) : cc;
				if (this.ShouldTag(ccPrev)) {
					char cPrev = ccPrev.GetChar();
					var brace2 = braces.FirstOrDefault(x => x.Item2 == cPrev);
					if (brace2 != null) {
						var match = this.FindMatchBackwards(ss, brace2.Item2, brace2.Item1, Math.Max(0, cc.Position - 1));
						if (match != null) {
							var tag = new TextMarkerTag("blue");
							yield return new TagSpan<TextMarkerTag>(new SnapshotSpan(ccPrev, 1), tag);
							yield return new TagSpan<TextMarkerTag>(new SnapshotSpan(ss, match.Value, 1), tag);
						}
					}
				}
			}
		}

		private int? FindMatchForwards(ITextSnapshot ss, char cInc, char cDec, int startPos) {
			var mlsByPosition = this.overview.MultiLines
				.Select(x => new {x.Span.GetStartPoint(ss).Position, x.Span.GetSpan(ss).Length});
			int lineNumber = ss.GetLineNumberFromPosition(startPos);
			var line = ss.GetLineFromLineNumber(lineNumber);
			var lineText = line.GetText();
			int lineOfs = startPos - line.Start.Position;
			int foundPos = startPos;
			int depth = 0;

			for (; ; ) {
				var infos = Parser.Parse(lineText);
				var ignores = infos
					.Where(x => x.Token.In(Token.StringLiteral, Token.Comment))
					.Select(x => new { Position = x.Start + line.Start.Position, x.Length })
					.Concat(mlsByPosition)
					.Distinct(x => x.Position, (a, b) => a.Length > b.Length ? a : b)
					.ToDictionary(x => x.Position, x => x.Length);
				while (lineOfs < lineText.Length) {
					int skipLength = ignores.ValueOrDefault(foundPos);
					if (skipLength == 0) {
						char c = lineText[lineOfs];
						if (c == cInc) {
							depth++;
						} else if (c == cDec) {
							depth--;
							if (depth == 0) {
								return foundPos;
							}
						}
						lineOfs++;
						foundPos++;
					} else {
						lineOfs += skipLength;
						foundPos += skipLength;
					}
				}
				if (foundPos >= ss.Length) {
					return null;
				}
				while (lineOfs >= line.Length) {
					lineOfs -= line.LengthIncludingLineBreak;
					lineNumber++;
					line = ss.GetLineFromLineNumber(lineNumber);
					lineText = line.GetText();
				}
				if (lineOfs < 0) {
					foundPos -= lineOfs;
					lineOfs = 0;
				}
				if (foundPos >= ss.Length) {
					return null;
				}
			}

		}

		private int? FindMatchBackwards(ITextSnapshot ss, char cInc, char cDec, int startPos) {
			var mlsByPosition = this.overview.MultiLines
				.Select(x => new { x.Span.GetStartPoint(ss).Position, x.Span.GetSpan(ss).Length });
			int lineNumber = ss.GetLineNumberFromPosition(startPos);
			var line = ss.GetLineFromLineNumber(lineNumber);
			var lineText = line.GetText();
			int lineOfs = startPos - line.Start.Position;
			int foundPos = startPos;
			int depth = 0;

			for (; ; ) {
				var infos = Parser.Parse(lineText);
				var ignores = infos
					.Where(x => x.Token == Token.StringLiteral)
					.Select(x => new { Position = x.Start + line.Start.Position, x.Length })
					.Concat(mlsByPosition)
					.Distinct(x => x.Position, (a, b) => a.Length > b.Length ? a : b)
					.ToDictionary(x => x.Position + x.Length - 1, x => x.Length);
				while (lineOfs >= 0) {
					int skipLength = ignores.ValueOrDefault(foundPos);
					if (skipLength == 0) {
						char c = lineText[lineOfs];
						if (c == cInc) {
							depth++;
						} else if (c == cDec) {
							depth--;
							if (depth == 0) {
								return foundPos;
							}
						}
						lineOfs--;
						foundPos--;
					} else {
						lineOfs -= skipLength;
						foundPos -= skipLength;
					}
				}
				if (foundPos < 0) {
					return null;
				}
				while (lineOfs < 0) {
					lineNumber--;
					line = ss.GetLineFromLineNumber(lineNumber);
					lineText = line.GetText();
					lineOfs += line.LengthIncludingLineBreak;
				}
				if (lineOfs >= line.Length) {
					foundPos -= lineOfs - line.Length + 1;
					lineOfs = line.Length - 1;
				}
				if (foundPos < 0) {
					return null;
				}
			}

		}
	
	}
}
