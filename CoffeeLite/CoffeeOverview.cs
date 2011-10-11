using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Text;

namespace CoffeeSyntax {

	using MlInfo = Tuple<string, string, CoffeeOverview.MultiLineType, string>;

	class CoffeeOverview {

		public enum MultiLineType {
			Comment, Backtick, StringSingle, StringDouble, HeredocSingle, HeredocDouble, Regex
		}

		public struct MultiLine {
			public MultiLine(ITrackingSpan span, MultiLineType type)
				: this() {
				this.Span = span;
				this.Type = type;
			}
			public ITrackingSpan Span { get; private set; }
			public MultiLineType Type { get; private set; }
			public SnapshotSpan GetSpan(ITextSnapshot ss) {
				return this.Span.GetSpan(ss);
			}
			public override string ToString() {
				return string.Format("{{ {0}, {1} }}", this.Span, this.Type);
			}
		}

		public class MultiLinesChangedEventArgs : EventArgs {
			public MultiLinesChangedEventArgs(IEnumerable<MultiLine> added, IEnumerable<MultiLine> removed) {
				this.Added = added;
				this.Removed = removed;
			}
			public IEnumerable<MultiLine> Added { get; private set; }
			public IEnumerable<MultiLine> Removed { get; private set; }
		}

		public CoffeeOverview(ITextBuffer textBuffer) {
			this.textBuffer = textBuffer;
			this.CalcMultiLines();
			textBuffer.Changed += (o, e) => {
				if (this.IsMultiLineChange(e)) {
					this.CalcMultiLines();
				}
			};
		}

		private ITextBuffer textBuffer;

		public IEnumerable<MultiLine> MultiLines { get; private set; }
		public event EventHandler<MultiLinesChangedEventArgs> MultiLinesChanged;

		private static MlInfo[] multiLinesDelimiters =
		{
			Tuple.Create("###", "###", MultiLineType.Comment, (string)null),
			//Tuple.Create("/*", "*/", MultiLineType.Comment, (string)null),
			Tuple.Create("`", "`", MultiLineType.Backtick, (string)null),
			Tuple.Create("'", "'", MultiLineType.StringSingle, "\\'"),
			Tuple.Create("\"", "\"", MultiLineType.StringDouble, "\\\""),
			Tuple.Create("'''", "'''", MultiLineType.HeredocSingle, (string)null),
			Tuple.Create("\"\"\"", "\"\"\"", MultiLineType.HeredocDouble, (string)null),
			Tuple.Create("///", "///", MultiLineType.Regex, (string)null),
		};

		private static string[] multiLinesSigTexts =
			multiLinesDelimiters.SelectMany(x => new[] { x.Item1, x.Item2 }).Distinct().ToArray();

		private bool IsMultiLineChange(TextContentChangedEventArgs e) {
			return e.Changes.Any(change => {
				return multiLinesSigTexts.Any(sigText => {
					var len1 = sigText.Length - 1;
					var beforeStart = Math.Max(0, change.OldPosition - len1);
					var beforeEnd = Math.Min(e.Before.Length, change.OldPosition + change.OldLength + len1);
					var textOld = beforeEnd > beforeStart ? e.Before.GetText(beforeStart, beforeEnd - beforeStart) : "";
					var afterStart = Math.Max(0, change.NewPosition - len1);
					var afterEnd = Math.Min(e.After.Length, change.NewPosition + change.NewLength + len1);
					var textNew = afterEnd > afterStart ? e.After.GetText(afterStart, afterEnd - afterStart) : "";
					return textOld.Contains(sigText) || textNew.Contains(sigText);
				});
			});
		}

		private void CalcMultiLines() {
			var spans = new List<MultiLine>();
			var ss = this.textBuffer.CurrentSnapshot;
			var text = ss.GetText();
			MlInfo curMultiline = null;
			int startPos = -1;
			bool inSingleLineComment = false;
			for (int i = 0, n = text.Length; i < n; i++) {
				if (inSingleLineComment) {
					if (text[i].In('\r', '\n')) {
						inSingleLineComment = false;
					}
				} else {
					if (curMultiline == null) {
                        if (text[i] == '#' && !text.IsAt(i, "###") && !text.IsAt(i - 1, "###") && !text.IsAt(i - 1, "###")) {
							inSingleLineComment = true;
						} else {
							curMultiline = multiLinesDelimiters.FirstOrDefault(x => text.IsAt(i, x.Item1));
							if (curMultiline != null) {
								startPos = i;
								i += curMultiline.Item1.Length - 1 + curMultiline.Item2.Length - 1;
							}
						}
					} else {
						if (curMultiline.Item4 != null && text.IsAt(i, curMultiline.Item4)) {
							i += curMultiline.Item4.Length - 1;
						} else if (text.IsAt(i - (curMultiline.Item2.Length - 1), curMultiline.Item2)) {
							var trackingSpan = ss.CreateTrackingSpan(startPos, i - startPos + 1, SpanTrackingMode.EdgeExclusive);
							spans.Add(new MultiLine(trackingSpan, curMultiline.Item3));
							curMultiline = null;
						}
					}
				}
			}
			if (curMultiline != null) {
				if (ss.GetLineNumberFromPosition(startPos) != ss.GetLineNumberFromPosition(text.Length - 1)) {
					var trackingSpan = ss.CreateTrackingSpan(startPos, text.Length - startPos, SpanTrackingMode.EdgeExclusive);
					spans.Add(new MultiLine(trackingSpan, curMultiline.Item3));
				}
			}
			this.RaiseMultiLinesChanged(spans, this.MultiLines);
			this.MultiLines = spans;
		}

		private void RaiseMultiLinesChanged(IEnumerable<MultiLine> added, IEnumerable<MultiLine> removed) {
			if (this.MultiLinesChanged != null) {
				var eventArgs = new MultiLinesChangedEventArgs(added, removed);
				this.MultiLinesChanged(this, eventArgs);
			}
		}

	}
}
