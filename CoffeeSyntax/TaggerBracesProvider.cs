using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace CoffeeSyntax {

	[Export(typeof(IViewTaggerProvider))]
	[ContentType("coffee")]
	[TagType(typeof(TextMarkerTag))]
	class TaggerBracesProvider : IViewTaggerProvider {

		public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer textBuffer) where T : ITag {
			if (textView == null) {
				return null;
			}
			if (textView.TextBuffer != textBuffer) {
				return null;
			}

			var overview = textBuffer.Properties.GetOrCreateSingletonProperty<CoffeeOverview>(() => new CoffeeOverview(textBuffer));
			return new TaggerBraces(overview, textView, textBuffer) as ITagger<T>;
		}

	}

}
