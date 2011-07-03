using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Text.Classification;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace CoffeeSyntax {

	[Export(typeof(IClassifierProvider))]
	[ContentType("coffee")]
	internal class ClassifierProvider : IClassifierProvider {

		[Import]
		internal IClassificationTypeRegistryService ClassificationRegistry = null;

		public IClassifier GetClassifier(ITextBuffer textBuffer) {
			return textBuffer.Properties.GetOrCreateSingletonProperty<Classifier>(() => {
				var overview = textBuffer.Properties.GetOrCreateSingletonProperty<CoffeeOverview>(() => new CoffeeOverview(textBuffer));
				return new Classifier(overview, this.ClassificationRegistry);
			});
		}

	}

}
