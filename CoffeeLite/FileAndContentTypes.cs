using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace CoffeeSyntax {

	internal static class FileAndContentTypes {

		[Export]
		[Name("coffee")]
		[BaseDefinition("text")]
		internal static ContentTypeDefinition coffeeContentTypeDefinition = null;

		[Export]
		[FileExtension(".coffee")]
		[ContentType("coffee")]
		internal static FileExtensionToContentTypeDefinition coffeeFileExtensionDefinition = null;

	}

}
